using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.Internal;

namespace DotaClient
{
    public class DotaClientApi
    {
        private readonly SteamClient _client;
    
        private readonly SteamUser _user;
        private readonly SteamGameCoordinator _gameCoordinator;
    
        private readonly CallbackManager _callbackMgr;
    
        private readonly string _userName;
        private readonly string _password;
        private readonly long _accountId;

        private bool _gotProfile;
        private CMsgClientToGCGetProfileCard _profile;

        // dota2's appid
        private const int Appid = 570;

        public DotaClientApi(string user, string pass, long accountId)
        {
            _userName = user;
            _password = pass;
            _accountId = accountId;

            _client = new SteamClient();

            //DebugLog.AddListener(new MyListener());
    
            // get our handlers
            _user = _client.GetHandler<SteamUser>();
            _gameCoordinator = _client.GetHandler<SteamGameCoordinator>();
    
            // setup callbacks
            _callbackMgr = new CallbackManager(_client);
    
            _callbackMgr.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            _callbackMgr.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            _callbackMgr.Subscribe<SteamGameCoordinator.MessageCallback>(OnGcMessage);
        }
    
        private void Connect()
        {
            Debug.WriteLine("Connecting to Steam...");
    
            // begin the connection to steam
            _client.Connect();
        }
    
        private CMsgClientToGCGetProfileCard Wait(int timeout = 30)
        {
            var sw = Stopwatch.StartNew();

            while (!_gotProfile && sw.Elapsed.TotalSeconds < timeout)
            {
                // continue running callbacks until we get profile details
                _callbackMgr.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            if(!_gotProfile)
                throw new Exception("No response...");

            return _profile;
        }

        public void GetMmr(out int? solo, out int? party)
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
        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                // connection failed (steam servers down, at capacity, etc)
                // here you would delay your next connection attempt by say 30 seconds
                // steamkit doesn't automatically reconnect, you have to perform the connection yourself
    
                Debug.WriteLine("Unable to connect to Steam: {0}", callback.Result);
    
                _gotProfile = true; // we didn't actually get the profile details, but we need to jump out of the callback loop
                return;
            }
    
            Debug.WriteLine("Connected! Logging '{0}' into Steam...", _userName);
    
            // we've successfully connected, so now attempt to logon
            _user.LogOn(new SteamUser.LogOnDetails
            {
                Username = _userName,
                Password = _password,
            });
        }
    
        // called when the client successfully (or unsuccessfully) logs onto an account
        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                // logon failed (password incorrect, steamguard enabled, etc)
                // an EResult of AccountLogonDenied means the account has SteamGuard enabled and an email containing the authcode was sent
                // in that case, you would get the auth code from the email and provide it in the LogOnDetails
    
                Debug.WriteLine("Unable to logon to Steam: {0}", callback.Result);
    
                _gotProfile = true; // we didn't actually get the profile details, but we need to jump out of the callback loop
                return;
            }
    
            Debug.WriteLine("Logged in! Launching DOTA...");
    
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
            _client.Send(playGame);
    
            // delay a little to give steam some time to establish a GC connection to us
            Thread.Sleep(5000);
    
            // inform the dota GC that we want a session
            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint) EGCBaseClientMsg.k_EMsgGCClientHello)
            {
                Body = {engine = ESourceEngine.k_ESE_Source2}
            };
            _gameCoordinator.Send(clientHello, Appid);
        }
    
        // called when a gamecoordinator (GC) message arrives
        // these kinds of messages are designed to be game-specific
        // in this case, we'll be handling dota's GC messages
        private void OnGcMessage(SteamGameCoordinator.MessageCallback callback)
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
        private void OnClientWelcome(IPacketGCMsg packetMsg)
        {
            // in order to get at the contents of the message, we need to create a ClientGCMsgProtobuf from the packet message we recieve
            // note here the difference between ClientGCMsgProtobuf and the ClientMsgProtobuf used when sending ClientGamesPlayed
            // this message is used for the GC, while the other is used for general steam messages
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);
    
            Debug.WriteLine("GC is welcoming us. Version: {0}", msg.Body.version);
    
            // at this point, the GC is now ready to accept messages from us
            // so now we'll request the details of the profile we're looking for

            var requestProfile = new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>((uint) EDOTAGCMsg.k_EMsgClientToGCGetProfileCard)
            {
                Body = {account_id = uint.Parse(_accountId.ToString()) }
            };

            _gameCoordinator.Send(requestProfile, Appid);
        }
    
        // this message arrives after we've requested the details for a profile
        private void OnProfileDetails(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgClientToGCGetProfileCard>(packetMsg);

            _profile = msg.Body;
            _gotProfile = true;
    
            // we've got everything we need, we can disconnect from steam now
            _client.Disconnect();
        }
    }

    // define our debuglog listener
    class MyListener : IDebugListener
    {
        public void WriteLine(string category, string msg)
        {
            // this function will be called when internal steamkit components write to the debuglog

            // for this example, we'll print the output to the console
            Debug.WriteLine("MyListener - {0}: {1}", category, msg);
        }
    }
}
