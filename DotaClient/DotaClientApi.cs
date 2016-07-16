using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using AE.Net.Mail;
using RestClientHelper;
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

        private static readonly SteamClient SteamClient;
        private static readonly SteamUser User;
        private static readonly SteamGameCoordinator GameCoordinator;
        private static readonly CallbackManager CallbackManager;
        private static string _authCode;

        private static bool _gotProfile;
        private static CMsgClientToGCGetProfileCard _profile;

        private const int MaxDisconnectedCount = 2;
        private static int _disconnectedCount;

        // dota2's appid
        private const int Appid = 570;

        static DotaClientApi()
        {
            SteamClient = new SteamClient();

            DebugLog.AddListener(new DebugListener());
    
            // get our handlers
            User = SteamClient.GetHandler<SteamUser>();
            GameCoordinator = SteamClient.GetHandler<SteamGameCoordinator>();
    
            // setup callbacks
            CallbackManager = new CallbackManager(SteamClient);
    
            CallbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            CallbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            CallbackManager.Subscribe<SteamGameCoordinator.MessageCallback>(OnGcMessage);
        }

        private static void GetLastCodeFromMail()
        {
            var mailClient = new ImapClient(ImapHost, ImapLogin, ImapPass, AuthMethods.Login, ImapPort, ImapUseSsl);

            var msgs = mailClient.SearchMessages(SearchCondition.From("noreply@steampowered.com"));
            var text = msgs.Last().Value.Body;

            var match = Regex.Match(text, $"to account {SteamUser}:\r\n\r\n(?<code>\\S+)\r\n", RegexOptions.Singleline);

            if (match.Success)
                _authCode = match.Groups["code"].Value;
        }

    
        private static void Connect()
        {
            DebugListener.WriteLine("Connecting to Steam...");
    
            // begin the connection to steam
            SteamClient.Connect();
        }
    
        private static CMsgClientToGCGetProfileCard Wait(int timeout = 30)
        {
            var sw = Stopwatch.StartNew();

            while (!_gotProfile && _disconnectedCount < MaxDisconnectedCount && sw.Elapsed.TotalSeconds < timeout)
            {
                // continue running callbacks until we get profile details
                CallbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            if(!_gotProfile)
                throw new Exception("No response...");

            return _profile;
        }

        public static void GetMmr(out int? solo, out int? party)
        {
            Connect();
            var result = Wait();

            solo = null;
            party = null;
            foreach (var slot in result.slots)
            {
                if (slot.stat != null && slot.stat.stat_id == CMsgDOTAProfileCard.EStatID.k_eStat_SoloRank)
                    solo = (int) slot.stat.stat_score;

                if (slot.stat != null && slot.stat.stat_id == CMsgDOTAProfileCard.EStatID.k_eStat_PartyRank)
                    party = (int) slot.stat.stat_score;
            }
        }

        // called when the client successfully (or unsuccessfully) connects to steam
        private static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                // connection failed (steam servers down, at capacity, etc)
                // here you would delay your next connection attempt by say 30 seconds
                // steamkit doesn't automatically reconnect, you have to perform the connection yourself

                DebugListener.WriteLine($"Unable to connect to Steam: {callback.Result}");
    
                _gotProfile = true; // we didn't actually get the profile details, but we need to jump out of the callback loop
                return;
            }
    
            DebugListener.WriteLine($"Connected! Logging '{SteamUser}' into Steam...");
    
            // we've successfully connected, so now attempt to logon
            User.LogOn(new SteamUser.LogOnDetails
            {
                Username = SteamUser,
                Password = SteamPass,
                AuthCode = _authCode
            });
        }

        private static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            DebugListener.WriteLine("Disconnected from Steam, reconnecting...");

            if (_disconnectedCount < MaxDisconnectedCount)
            {
                _disconnectedCount++;
                SteamClient.Connect();
            }
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
                // an EResult of AccountLogonDenied means the account has SteamGuard enabled and an email containing the authcode was sent
                // in that case, you would get the auth code from the email and provide it in the LogOnDetails

                DebugListener.WriteLine($"Unable to logon to Steam: {callback.Result}");
    
                _gotProfile = true; // we didn't actually get the profile details, but we need to jump out of the callback loop
                return;
            }

            DebugListener.WriteLine("Logged in! Launching DOTA...");
    
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
            // in order to get at the contents of the message, we need to create a ClientGCMsgProtobuf from the packet message we recieve
            // note here the difference between ClientGCMsgProtobuf and the ClientMsgProtobuf used when sending ClientGamesPlayed
            // this message is used for the GC, while the other is used for general steam messages
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);

            DebugListener.WriteLine($"GC is welcoming us. Version: {msg.Body.version}");
    
            // at this point, the GC is now ready to accept messages from us
            // so now we'll request the details of the profile we're looking for

            var requestProfile = new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>((uint) EDOTAGCMsg.k_EMsgClientToGCGetProfileCard)
            {
                Body = {account_id = SteamId }
            };

            GameCoordinator.Send(requestProfile, Appid);
        }
    
        // this message arrives after we've requested the details for a profile
        private static void OnProfileDetails(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>(packetMsg);

            _profile = msg.Body;
            _gotProfile = true;
    
            // we've got everything we need, we can disconnect from steam now
            SteamClient.Disconnect();
        }

        private class DebugListener : IDebugListener
        {
            public void WriteLine(string category, string msg)
            {
                Debug.WriteLine($"DebugListener - {category}: {msg}");
            }

            public static void WriteLine(string format, params object[] args)
            {
                Debug.WriteLine(format, args);
            }
        }
    }

}
