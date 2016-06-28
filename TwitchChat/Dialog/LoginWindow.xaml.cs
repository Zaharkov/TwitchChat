using VkApi;

namespace TwitchChat.Dialog
{
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public string TwitchAccessToken { get; set; }

        public string VkAccessToken { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
            //wbMain.Navigating += OnNavigatingTwitch;
            //wbMain.Navigate(TwitchApi.TwitchApiClient.AuthorizeUrl);

            wbMain.Navigating += OnNavigatingVk;
            wbMain.Navigate(VkApiClient.AuthorizeUrl);
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
                            TwitchAccessToken = values[1];
                            break;
                    }
                }
                wbMain.Navigating -= OnNavigatingTwitch;
                Close();
            }
        }

        void OnNavigatingVk(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.Query.StartsWith("?code"))
            {
                var fragments = e.Uri.Query.TrimStart('?').Split('=');
                VkAccessToken = fragments[1];

                var path = VkApiClient.GetTokenUrl + VkAccessToken;

                wbMain.Navigating -= OnNavigatingVk;
                Close();
            }
        }
    }
}
