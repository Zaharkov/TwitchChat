namespace TwitchChat.Dialog
{
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public string Token { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
            wbMain.Navigating += OnNavigating;
            wbMain.Navigate(
                $"https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id={App.ClientId}&redirect_uri={App.Url}&scope={"chat_login user_read"}");
        }

        void OnNavigating(object sender, NavigatingCancelEventArgs e)
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
                            Token = values[1];
                            break;
                    }
                }
                wbMain.Navigating -= OnNavigating;
                Close();
            }
        }
    }
}
