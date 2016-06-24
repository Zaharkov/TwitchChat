using System;
using System.Configuration;

namespace TwitchChat
{
    using System.Windows;
    using Dialog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        //  Create a client id for twitch application that redirects to 
        public static string ClientId = GetSetting("ClientId");
        public static string Url = GetSetting("Url");
        public const int Maxmessages = 150;

        private MainWindowViewModel _vm;

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            _vm = new MainWindowViewModel();
            _vm.Whispers.CollectionChanged += Whispers_CollectionChanged;

            mainWindow.DataContext = _vm;
            mainWindow.Closing += (sender, ee) =>
            {
                _vm.Logout();
            };
            mainWindow.Show();
        }

        private void Whispers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(WhisperWindowViewModel vm in e.NewItems)
            {
                var whisperWindow = new WhisperWindow {DataContext = vm};
                whisperWindow.Closing += WhisperWindow_Closing;
                whisperWindow.Show();
            }
        }

        private void WhisperWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //  Remove a whipser once the window is closed
            _vm.Whispers.Remove(sender as WhisperWindowViewModel);
        }

        private static string GetSetting(string name)
        {
            var value = ConfigurationManager.AppSettings.Get(name);

            if(string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(name), @"Not exists");

            return value;
        }
    }
}
