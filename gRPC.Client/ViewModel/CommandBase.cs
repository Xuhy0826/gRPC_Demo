using System;
using System.Windows.Input;

namespace gRPC.Client.ViewModel
{
    public class CommandBase : ICommand
    {
        private readonly Action<object> _commandpara;
        private readonly Action _command;
        private readonly Func<bool> _canExecute;

        public CommandBase(Action command, Func<bool> canExecute = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException();
            }
            _canExecute = canExecute;
            _command = command;
        }

        public CommandBase(Action<object> commandpara, Func<bool> canExecute = null)
        {
            if (commandpara == null)
            {
                throw new ArgumentNullException();
            }
            _canExecute = canExecute;
            _commandpara = commandpara;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                _commandpara(parameter);
            }
            else
            {
                if (_command != null)
                {
                    _command();
                }
                else if (_commandpara != null)
                {
                    _commandpara(null);
                }
            }
        }
    }
}
