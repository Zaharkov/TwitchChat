using System.Collections.Specialized;
using System.ComponentModel;
using Database;

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
                Current.Shutdown();
            };

            Current.DispatcherUnhandledException += (serder, ee) =>
            {
                SqLiteClient.LogException("Fatal error in application", ee.Exception);
                _vm.Logout();
            };

            mainWindow.Show();
        }

        private void Whispers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (WhisperWindowViewModel vm in e.NewItems)
                {
                    var whisperWindow = new WhisperWindow {DataContext = vm};
                    vm.OnRemove += (whisperSender, ee) =>
                    {
                        _vm.Whispers.Remove(whisperSender as WhisperWindowViewModel);
                        whisperWindow.Close();
                    };
                    whisperWindow.Closing += WhisperWindow_Closing;
                    whisperWindow.Show();
                }
            }
        }

        private void WhisperWindow_Closing(object sender, CancelEventArgs e)
        {
            var window = sender as WhisperWindow;

            if (window == null)
                return;

            //  Remove a whipser once the window is closed
            _vm.Whispers.Remove(window.DataContext as WhisperWindowViewModel);
        }
    }
}
