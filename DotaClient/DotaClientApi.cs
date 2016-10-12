using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AE.Net.Mail;
using CommonHelper;
using Domain.Repositories;
using DotaClient.Friend;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.Internal;

namespace DotaClient
{
    public static class DotaClientApi
    {
        private static readonly uint SteamId = uint.Parse(Configuration.GetSetting("SteamID"));
        private static readonly string SteamUser = Configuration.GetSetting("SteamUser");
        private static readonly string SteamPass = Configuration.GetSetting("SteamPass");

        private static readonly string ImapHost = Configuration.GetSetting("ImapHost");
        private static readonly int ImapPort = int.Parse(Configuration.GetSetting("ImapPort"));
        private static readonly string ImapLogin = Configuration.GetSetting("ImapLogin");
        private static readonly string ImapPass = Configuration.GetSetting("ImapPass");
        private static readonly bool ImapUseSsl = bool.Parse(Configuration.GetSetting("ImapUseSsl"));

        private static ClientState _state = ClientState.None;
        private static readonly Stopwatch ConnectProceed = new Stopwatch();
        private static DateTime _loginDate = DateTime.Now;
        private const int MaxWaitForConnect = 60;
        private const int MaxWaitForMail = 30;

        private static CancellationTokenSource _checkForCallbacksToken;

        private static readonly SteamClient SteamClient;
        private static readonly SteamUser User;
        private static readonly SteamFriends SteamFriends;
        private static readonly SteamGameCoordinator GameCoordinator;
        private static readonly CallbackManager CallbackManager;
        private static string _authCode;
        private static readonly SteamListener Listener = new SteamListener();

        private static bool _needWaitFriendList = true;

        private static readonly Dictionary<string, object>  ResponseHandler = new Dictionary<string, object>();

        private const int MaxDisconnectedCount = 2;
        private static int _disconnectedCount;

        // dota2's appid
        private const int Appid = 570;

        static DotaClientApi()
        {
            SteamClient = new SteamClient();

            DebugLog.AddListener(Listener);
    
            // get our handlers
            User = SteamClient.GetHandler<SteamUser>();
            // get the steam friends handler, which is used for interacting with friends on the network after logging on
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            GameCoordinator = SteamClient.GetHandler<SteamGameCoordinator>();
    
            // setup callbacks
            CallbackManager = new CallbackManager(SteamClient);
    
            CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            CallbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            CallbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            CallbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
            CallbackManager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);


            CallbackManager.Subscribe<SteamGameCoordinator.MessageCallback>(OnGcMessage);
            CallbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            ConnectProceed.Start();
            
        }

        public static void Init()
        {
            if(_state == ClientState.Disconnected || _state == ClientState.None)
                Connect();

            while (_state != ClientState.Logged && ConnectProceed.Elapsed.TotalSeconds < MaxWaitForConnect)
                CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));

            if (_state == ClientState.Logged)
            {
                _checkForCallbacksToken = new CancellationTokenSource();
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (_checkForCallbacksToken.Token.IsCancellationRequested)
                            break;

                        CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                    }
                });
            }
        }
    
        private static void Connect()
        {
            _state = ClientState.Connecting;
            Listener.WriteLine("Connecting to Steam...");
    
            // begin the connection to steam
            SteamClient.Connect();
        }
    
        private static T Wait<T>(Action command = null, int timeout = 30) where T : class
        {
            while (_state != ClientState.Logged && ConnectProceed.Elapsed.TotalSeconds < MaxWaitForConnect)
                CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));

            if (_disconnectedCount >= MaxDisconnectedCount || ConnectProceed.Elapsed.TotalSeconds > MaxWaitForConnect)
                throw new Exception($"Connect to steam client failed with state: {_state}");

            command?.Invoke();

            var sw = Stopwatch.StartNew();
            var name = typeof(T).FullName;

            Listener.WriteLine($"Getting response for {name}");

            while (!ResponseHandler.ContainsKey(name) && sw.Elapsed.TotalSeconds < timeout)
            {
                // continue running callbacks until we get profile details
                CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            if(!ResponseHandler.ContainsKey(name))
                throw new Exception($"No response for type {name}");

            var response = ResponseHandler[name] as T;
            ResponseHandler.Remove(name);
            return response;
        }

        public static void GetMmr(out int? solo, out int? party)
        {
            solo = null;
            party = null;

            try
            {
                var result = Wait<CMsgClientToGCGetProfileCard>(GetProfile);

                foreach (var slot in result.slots)
                {
                    if (slot.stat != null && slot.stat.stat_id == CMsgDOTAProfileCard.EStatID.k_eStat_SoloRank)
                        solo = (int)slot.stat.stat_score;

                    if (slot.stat != null && slot.stat.stat_id == CMsgDOTAProfileCard.EStatID.k_eStat_PartyRank)
                        party = (int)slot.stat.stat_score;
                }
            }
            catch (Exception ex)
            {
                LogRepository.Instance.LogException("Error while try get MMR", ex);
                Listener.WriteLine($"Error while try get MMR: {ex}");
            }
        }

        public static FriendResponseContainer AddFriend(uint id)
        {
            try
            {
                var steamId = new SteamID();
                steamId.Set(id, EUniverse.Public, EAccountType.Individual);

                var friends = GetFriendList();
                var exist = friends.FirstOrDefault(t => t.Key.AccountID == steamId.AccountID);

                if (!exist.Equals(default(KeyValuePair<SteamID, EFriendRelationship>)))
                {
                    if (exist.Value == EFriendRelationship.RequestRecipient)
                        return AddFriendRequest(steamId);

                    return new FriendResponseContainer((int)EResult.OK, exist.Value.Map(), FriendResponseStatus.AlreadyAdded);
                }

                return AddFriendRequest(steamId);
            }
            catch (Exception ex)
            {
                LogRepository.Instance.LogException("Error while try add steam friend", ex);
                Listener.WriteLine($"Error while try add steam friend: {ex}");
            }

            return new FriendResponseContainer((int)EResult.Fail, FriendResponseRelationship.None, FriendResponseStatus.Error);
        }

        public static FriendResponseContainer RemoveFriend(uint id, bool ignoreBug = false)
        {
            try
            {
                var steamId = new SteamID();
                steamId.Set(id, EUniverse.Public, EAccountType.Individual);

                var friends = GetFriendList();
                var exist = friends.FirstOrDefault(t => t.Key.AccountID == steamId.AccountID);

                if (!exist.Equals(default(KeyValuePair<SteamID, EFriendRelationship>)))
                {
                    if(exist.Value == EFriendRelationship.Friend || exist.Value == EFriendRelationship.RequestRecipient)
                        return RemoveFriendRequest(steamId);

                    if (exist.Value == EFriendRelationship.RequestInitiator)
                    {
                        if(ignoreBug)
                            return RemoveFriendRequest(steamId);

                        return new FriendResponseContainer((int)EResult.OK, exist.Value.Map(), FriendResponseStatus.CantRemove);
                    }

                    return new FriendResponseContainer((int)EResult.OK, exist.Value.Map(), FriendResponseStatus.NotInFriends);
                }

                return new FriendResponseContainer((int)EResult.OK, FriendResponseRelationship.None, FriendResponseStatus.NotInFriends);
            }
            catch (Exception ex)
            {
                LogRepository.Instance.LogException("Error while try remove steam friend", ex);
                Listener.WriteLine($"Error while try remove steam friend: {ex}");
            }

            return new FriendResponseContainer((int)EResult.Fail, FriendResponseRelationship.Error, FriendResponseStatus.Error);
        }

        private static FriendResponseContainer AddFriendRequest(SteamID steamId)
        {
            var response = Wait<SteamFriends.FriendAddedCallback>(() => SteamFriends.AddFriend(steamId));

            if (response.Result == EResult.OK)
            {
                var friends = GetFriendList();
                var exist = friends.FirstOrDefault(t => t.Key.AccountID == steamId.AccountID);

                if (!exist.Equals(default(KeyValuePair<SteamID, EFriendRelationship>)))
                    return new FriendResponseContainer((int)response.Result, exist.Value.Map(), FriendResponseStatus.Added);
            }

            return new FriendResponseContainer((int)response.Result, FriendResponseRelationship.Error, FriendResponseStatus.Error);
        }

        private static FriendResponseContainer RemoveFriendRequest(SteamID steamId)
        {
            SteamFriends.RemoveFriend(steamId);
            Wait<ReadOnlyCollection<SteamFriends.FriendsListCallback.Friend>>();
            _needWaitFriendList = false;

            var friends = GetFriendList();
            var exist = friends.FirstOrDefault(t => t.Key.AccountID == steamId.AccountID);

            if (exist.Equals(default(KeyValuePair<SteamID, EFriendRelationship>)))
                return new FriendResponseContainer((int)EResult.OK, FriendResponseRelationship.None, FriendResponseStatus.Removed);

            return new FriendResponseContainer((int)EResult.Fail, FriendResponseRelationship.Error, FriendResponseStatus.Error);
        }

        private static Dictionary<SteamID, EFriendRelationship> GetFriendList()
        {
            if(_needWaitFriendList)
                Wait<ReadOnlyCollection<SteamFriends.FriendsListCallback.Friend>>();

            _needWaitFriendList = false;

            var count = SteamFriends.GetFriendCount();

            var friends = new Dictionary<SteamID, EFriendRelationship>();
            for (var i = 0; i < count; i++)
            {
                var steamId = SteamFriends.GetFriendByIndex(i);
                var relationship = SteamFriends.GetFriendRelationship(steamId);
                friends.Add(steamId, relationship);
            }

            return friends;
        }

        // called when the client successfully (or unsuccessfully) connects to steam
        private static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                // connection failed (steam servers down, at capacity, etc)
                // here you would delay your next connection attempt by say 30 seconds
                // steamkit doesn't automatically reconnect, you have to perform the connection yourself

                _state = ClientState.Error;

                Listener.WriteLine($"Unable to connect to Steam: {callback.Result}");
    
                return;
            }

            _state = ClientState.Logging;
            _loginDate = DateTime.Now;

            Listener.WriteLine($"Connected! Logging '{SteamUser}' into Steam...");

            var logOnDetails = new SteamUser.LogOnDetails
            {
                Username = SteamUser,
                Password = SteamPass
            };

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                var sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            if (sentryHash != null)
                logOnDetails.SentryFileHash = sentryHash;
            else
                logOnDetails.AuthCode = _authCode;

            // we've successfully connected, so now attempt to logon
            User.LogOn(logOnDetails);
        }

        private static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            Listener.WriteLine("Disconnected from Steam, reconnecting...");

            if (_disconnectedCount < MaxDisconnectedCount && _state != ClientState.Error && _state != ClientState.Disconnected)
            {
                _disconnectedCount++;
                Connect();
            }

            if(_state != ClientState.Error)
                _state = ClientState.Disconnected;
        }

        private static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Listener.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            User.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });

            Listener.WriteLine("Done!");
        }

        // called when the client successfully (or unsuccessfully) logs onto an account
        private static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    GetLastCodeFromMail();
                    return;
                }

                // logon failed (password incorrect, steamguard enabled, etc)

                _state = ClientState.Error;

                Listener.WriteLine($"Unable to logon to Steam: {callback.Result}");
    
                return;
            }

            _state = ClientState.GettingSession;

            Listener.WriteLine("Logged in! Launching DOTA...");
    
            // we've logged into the account
            // now we need to inform the steam server that we're playing dota (in order to receive GC messages)
    
            // steamkit doesn't expose the "play game" message through any handler, so we'll just send the message manually
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
    
            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(Appid), // or game_id = APPID,
            });
    
            // send it off
            // notice here we're sending this message directly using the SteamClient
            SteamClient.Send(playGame);
    
            // delay a little to give steam some time to establish a GC connection to us
            Thread.Sleep(5000);
    
            // inform the dota GC that we want a session
            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello)
            {
                Body = {engine = ESourceEngine.k_ESE_Source2}
            };
            GameCoordinator.Send(clientHello, Appid);
        }

        /// <summary>
        /// Get code for auth from email
        /// </summary>
        private static void GetLastCodeFromMail()
        {
            var mailClient = new ImapClient(ImapHost, ImapLogin, ImapPass, AuthMethods.Login, ImapPort, ImapUseSsl);

            MailMessage message = null;

            var sw = Stopwatch.StartNew();

            while (message == null && sw.Elapsed.TotalSeconds < MaxWaitForMail)
            {
                var msgs = mailClient.SearchMessages(SearchCondition.From("noreply@steampowered.com"));

                foreach (var msg in msgs)
                {
                    if (msg.Value.Date > _loginDate)
                        message = msg.Value;
                }
            }

            if (message == null)
            {
                _state = ClientState.Error;
                return;
            }

            var match = Regex.Match(message.Body, $"to account {SteamUser}:\r\n\r\n(?<code>\\S+)\r\n", RegexOptions.Singleline);

            if (match.Success)
                _authCode = match.Groups["code"].Value;
        }

        private static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        private static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            _needWaitFriendList = true;
            Listener.WriteLine("Getted friends list");
            AddToResponseHandler(callback.FriendList);
        }

        private static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            AddToResponseHandler(callback);
        }

        private static void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            // this callback is received when the persona state (friend information) of a friend changes

            // for this sample we'll simply display the names of the friends
            Listener.WriteLine($"State change: {callback.Name}");
        }

        // called when a gamecoordinator (GC) message arrives
        // these kinds of messages are designed to be game-specific
        // in this case, we'll be handling dota's GC messages
        private static void OnGcMessage(SteamGameCoordinator.MessageCallback callback)
        {
            // setup our dispatch table for messages
            // this makes the code cleaner and easier to maintain
            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { ( uint )EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome },
                { ( uint )EDOTAGCMsg.k_EMsgClientToGCGetProfileCardResponse, OnProfileDetails },
            };

            Listener.WriteLine($"Received msg {callback.EMsg}");

            Action<IPacketGCMsg> func;
            if (!messageMap.TryGetValue(callback.EMsg, out func))
            {
                // this will happen when we recieve some GC messages that we're not handling
                // this is okay because we're handling every essential message, and the rest can be ignored
                return;
            }
    
            func(callback.Message);
        }
    
        // this message arrives when the GC welcomes a client
        // this happens after telling steam that we launched dota (with the ClientGamesPlayed message)
        // this can also happen after the GC has restarted (due to a crash or new version)
        private static void OnClientWelcome(IPacketGCMsg packetMsg)
        {
            _state = ClientState.Logged;
            ConnectProceed.Stop();
            // in order to get at the contents of the message, we need to create a ClientGCMsgProtobuf from the packet message we recieve
            // note here the difference between ClientGCMsgProtobuf and the ClientMsgProtobuf used when sending ClientGamesPlayed
            // this message is used for the GC, while the other is used for general steam messages
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);

            Listener.WriteLine($"GC is welcoming us. Version: {msg.Body.version}");
    
            // at this point, the GC is now ready to accept messages from us
        }
    
        // this message arrives after we've requested the details for a profile
        private static void OnProfileDetails(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>(packetMsg);

            AddToResponseHandler(msg.Body);
        }

        private static void GetProfile()
        {
            var requestProfile = new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>((uint)EDOTAGCMsg.k_EMsgClientToGCGetProfileCard)
            {
                Body = { account_id = SteamId }
            };

            GameCoordinator.Send(requestProfile, Appid);
        }

        private static void AddToResponseHandler<T>(T value)
        {
            var name = typeof(T).FullName;

            if (ResponseHandler.ContainsKey(name))
                ResponseHandler[name] = value;
            else
                ResponseHandler.Add(name, value);
        }

        public static void Disconnect()
        {
            if (_state == ClientState.Logged)
            {
                _checkForCallbacksToken.Cancel();
                _state = ClientState.Disconnected;
                SteamClient.Disconnect();
            }
        }
    }
}
