using Database;
using Database.Entities;
using TwitchApi;
using VkApi;

namespace TwitchChat.Dialog
{
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            wbMain.Navigating += OnNavigatingTwitch;
            wbMain.Navigate(TwitchApiClient.AuthorizeUrl);
        }

        void OnNavigatingTwitch(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.Fragment.StartsWith("#access_token"))
            {
                var fragments = e.Uri.Fragment.TrimStart('#').Split('&');
                foreach (var fragment in fragments)
                {
                    var values = fragment.Split(new[] { '=' }, 2);
                    switch (values[0])
                    {
                        case "access_token":
                            TwitchApiClient.SetToken(values[1]);
                            SqLiteClient.AddToken(AccessTokenType.Twitch, values[1]);
                            break;
                    }
                }
                wbMain.Navigating -= OnNavigatingTwitch;
                wbMain.Navigating += OnNavigatingVk;
                wbMain.Navigate(VkApiClient.AuthorizeUrl);
            }
        }

        void OnNavigatingVk(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.Query.StartsWith("?code"))
            {
                var fragments = e.Uri.Query.TrimStart('?').Split('=');
                var code = fragments[1];

                var token = VkApiClient.GetToken(code);
                SqLiteClient.AddToken(AccessTokenType.Vk, token.AccessToken, token.Expire);

                wbMain.Navigating -= OnNavigatingVk;
                Close();
            }
        }
    }
}
