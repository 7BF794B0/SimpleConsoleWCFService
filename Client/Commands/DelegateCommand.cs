using System;
using System.Windows.Input;

namespace Client.Commands
{
    class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<object> _execute;
        private bool _canExecute = true;

        public DelegateCommand(Action<object> execute, bool canExecute = true)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void SetCanExecute(bool val)
        {
            _canExecute = val;
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}
