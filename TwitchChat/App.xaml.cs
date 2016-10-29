using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Domain.Repositories;
using Domain.Utils;

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
            Task.Run(() =>
            {
                Backup.MakeBackUp();
            });

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
                LogRepository.Instance.LogException("Fatal error in application", ee.Exception);

                if(File.Exists("LastError.txt"))
                    File.Delete("LastError.txt");

                File.AppendAllText("LastError.txt", ee.Exception.ToString());

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
                    vm.OnRemove += (model) =>
                    {
                        _vm.Whispers.Remove(model);
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
