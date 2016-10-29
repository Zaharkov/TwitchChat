using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchApi;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;
using Twitchiedll.IRC.Limits;

namespace TwitchChat.Code
{
    public class TwitchIrcClient : Twitchie
    {
        private Task _listenTask;
        private readonly string _server;
        private readonly int _port;

        public string User { get; set; }
        public Dictionary<string, UserStateEventArgs> UserStateInfo = new Dictionary<string, UserStateEventArgs>();

        public delegate void ErrorEventHandler(Exception e);
        public delegate void DisconnectEventHandler();

        public event DisconnectEventHandler OnDisconnect;
        public event ErrorEventHandler OnError;

        public TwitchIrcClient()
        {
            Logger = new Logger();

            //  Get available servers and initialize IrcClient with the first one
            var server = TwitchApiClient.GetServers().ServersList.First();

            _server = server.Host;
            _port = server.Port;

            Connect(_server, _port);
        }

        public void Reconnect()
        {
            Connect(_server, _port);
        }

        private void OnGetUserState(UserStateEventArgs state)
        {
            if(UserStateInfo.ContainsKey(state.Channel))
                UserStateInfo[state.Channel] = state;
            else
                UserStateInfo.Add(state.Channel, state);

            var allMod = true;
            foreach (var userState in UserStateInfo)
            {
                if (!userState.Value.UserType.HasFlag(UserType.Moderator))
                    allMod = false;
            }

            if (allMod)
                MessageHandler.GlobalMessageLimit = MessageLimit.Moderator;
        }

        /// <summary>
        /// Login to twitch irc client
        /// </summary>
        /// <param name="user">Username of account to login with</param>
        /// <param name="pass">Oath token obtained with chat_login scope</param>
        public override void Login(string user, string pass)
        {
            User = user;
            base.Login(user, pass);
            OnUserState += OnGetUserState;
            Run();
        }

        public void Quit()
        {
            Dispose();
            _listenTask?.Wait();
        }

        private void Run()
        {
            _listenTask = Task.Run(() =>
            {
                try
                {
                    Listen();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }

                OnDisconnect?.Invoke();
            });
        }
    }
}
