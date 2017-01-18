using System;
using System.Windows.Input;

namespace TournamentManager.Client.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> onExecute, Func<T, bool> canExecute)
        {
            this._execute = onExecute;
            this._canExecute = canExecute;
        }

        public RelayCommand(Action<T> onExecute) : this(onExecute, arg => true) { }
        
        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute((T) parameter);
        }

        public void Execute(object parameter)
        {
            this._execute((T) parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add    { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action onExecute, Func<bool> canExecute) : base(o => onExecute(), o => canExecute())
        {

        }

        public RelayCommand(Action onExecute) : base(o => onExecute(), o => true)
        {

        }
    }
}