using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchApi;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code
{
    public class TwitchIrcClient : Twitchie
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public IrcState State { get; private set; }

        public string User { get; set; }

        public Dictionary<string, UserStateEventArgs> UserStateInfo = new Dictionary<string, UserStateEventArgs>();

        private string _server;
        private int _port;

        public TwitchIrcClient()
        {
            //  Get available servers and initialize IrcClient with the first one
            var server = TwitchApiClient.GetServers().ServersList.First();

            _server = server.Host;
            _port = server.Port;

            State = IrcState.Connecting;
            Connect(_server, _port);
            State = IrcState.Connected;
        }

        private void OnGetUserState(UserStateEventArgs state)
        {
            if(!UserStateInfo.ContainsKey(state.Channel))
                UserStateInfo.Add(state.Channel, state);
        }

        /// <summary>
        /// Login to twitch irc client
        /// </summary>
        /// <param name="user">Username of account to login with</param>
        /// <param name="pass">Oath token obtained with chat_login scope</param>
        public override void Login(string user, string pass)
        {
            State = IrcState.Registering;
            User = user;
            base.Login(user, pass);
            State = IrcState.Registered;

            OnUserState += OnGetUserState;

            Run();
        }

        public override void Quit()
        {
            State = IrcState.Closing;
            _tokenSource.Cancel();
            base.Quit();
            Dispose();
            State = IrcState.Closed;
        }

        private void Run()
        {
            Task.Run(() =>
            {
                try
                {
                    Listen(_tokenSource.Token);
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    State = IrcState.Error;
                    Debug.WriteLine("Unhandled Exception: {0}", ex.ToString());
                    throw;
                }
            }, _tokenSource.Token);
        }
    }
}
