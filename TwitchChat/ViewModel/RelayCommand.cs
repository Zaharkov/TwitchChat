namespace TwitchChat.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    /// <summary>
    /// Implementation found on http://stackoverflow.com/a/22286816/2258673
    /// </summary>
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(s => execute(), s => canExecute())
        {

        }

        public RelayCommand(Action execute)
            : this(s => execute(), null)
        {

        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

    }
}
