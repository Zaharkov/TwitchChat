using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TwitchApi;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code
{
    public class TwitchIrcClient : Twitchie
    {
        private Thread _listenThread;
        public IrcState State { get; private set; }

        public string User { get; set; }

        public Dictionary<string, UserStateEventArgs> UserStateInfo = new Dictionary<string, UserStateEventArgs>();

        private readonly string _server;
        private readonly int _port;

        public TwitchIrcClient()
        {
            State = IrcState.Connecting;
            //  Get available servers and initialize IrcClient with the first one
            var server = TwitchApiClient.GetServers().ServersList.First();

            _server = server.Host;
            _port = server.Port;

            Connect(_server, _port);
            State = IrcState.Connected;
        }

        public void Reconnect()
        {
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
            OnUserState += OnGetUserState;
            Run();

            State = IrcState.Registered;
        }

        public override void Quit()
        {
            State = IrcState.Closing;
            base.Quit();
            _listenThread.Abort();
            _listenThread.Join();
            State = IrcState.Closed;
        }

        private void Run()
        {
            _listenThread = new Thread(() =>
            {
                try
                {
                    Listen();
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception ex)
                {
                    State = IrcState.Error;
                    Debug.WriteLine($"Unhandled Exception: {ex}");
                    throw;
                }
                finally
                {
                    Dispose();
                }

                State = IrcState.Disconnected;

            });

            _listenThread.Start();
        }
    }
}
