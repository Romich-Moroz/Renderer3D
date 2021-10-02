using System;
using System.Windows.Input;

namespace Renderer3D.Viewmodels.Commands
{
    /// <summary>
    /// General purpose command
    /// </summary>
    /// <typeparam name="T">Type of the parameter for command</typeparam>
    internal class RelayCommand<T> : ICommand
    {

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action<T> _executeAction;

        private readonly Func<T, bool> _canExecuteAction;

        public RelayCommand(Action<T> executeAction, Func<T, bool> canExecuteAction)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter)
        {

            return _canExecuteAction == null || _canExecuteAction((T)parameter);
        }

        public void Execute(object parameter)
        {
            _executeAction((T)parameter);
        }
    }

    internal class RelayCommand : ICommand
    {

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action _executeAction;

        private readonly Func<bool> _canExecuteAction;

        public RelayCommand(Action executeAction, Func<bool> canExecuteAction)
        {
            _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            _canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter)
        {

            return _canExecuteAction == null || _canExecuteAction();
        }

        public void Execute(object parameter)
        {
            _executeAction();
        }
    }
}
