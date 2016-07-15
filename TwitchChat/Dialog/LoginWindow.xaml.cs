using System;
using System.Collections.Generic;
using System.Windows.Navigation;
using Database;
using Database.Entities;
using TwitchApi;
using VkApi;

namespace TwitchChat.Dialog
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        private static readonly Dictionary<LoginType, bool> IsTokenFromDatabase = new Dictionary<LoginType, bool>
        {
            { LoginType.Twitch, false },
            { LoginType.Vk, false }
        };

        private LoginWindow(LoginType type)
        {
            InitializeComponent();

            switch (type)
            {
                case LoginType.Twitch:
                {
                    var token = SqLiteClient.GetNotExpiredToken(AccessTokenType.Twitch);
                    if (string.IsNullOrEmpty(token))
                    {
                        WbMain.Navigating += OnNavigatingTwitch;
                        WbMain.Navigate(TwitchApiClient.AuthorizeUrl);
                    }
                    else
                    {
                        IsTokenFromDatabase[type] = true;
                        TwitchApiClient.SetToken(token);
                    }
                    break;
                }
                case LoginType.Vk:
                {
                    var token = SqLiteClient.GetNotExpiredToken(AccessTokenType.Vk, 3600 * 12);
                    if (string.IsNullOrEmpty(token))
                    {
                        WbMain.Navigating += OnNavigatingVk;
                        WbMain.Navigate(VkApiClient.AuthorizeUrl);
                    }
                    else
                    {
                        IsTokenFromDatabase[type] = true;
                        VkApiClient.SetToken(token);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            } 
        }

        public static void Login(LoginType type)
        {
            var login = new LoginWindow(type);

            if(!IsTokenFromDatabase[type])
                login.ShowDialog();
        }

        private void OnNavigatingTwitch(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.Fragment.StartsWith("#access_token"))
            {
                var fragments = e.Uri.Fragment.TrimStart('#').Split('&');
                foreach (var fragment in fragments)
                {
                    var values = fragment.Split(new[] {'='}, 2);
                    switch (values[0])
                    {
                        case "access_token":
                            TwitchApiClient.SetToken(values[1]);
                            SqLiteClient.AddToken(AccessTokenType.Twitch, values[1]);
                            break;
                    }
                }
                WbMain.Navigating -= OnNavigatingTwitch;
                Close();
            }
        }

        private void OnNavigatingVk(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.Query.StartsWith("?code"))
            {
                var fragments = e.Uri.Query.TrimStart('?').Split('=');
                var code = fragments[1];

                var token = VkApiClient.GetToken(code);
                SqLiteClient.AddToken(AccessTokenType.Vk, token.AccessToken, token.Expire);

                WbMain.Navigating -= OnNavigatingVk;
                Close();
            }
        }
    }
}
