using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordManager.Core.MVVM
{
        public class AsyncRelayCommand : ICommand
        {
                private readonly Func<object, Task> _execute;
                private readonly Func<object, bool> _canExecute;
                private bool _isExecuting;

                public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
                {
                        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                        _canExecute = canExecute;
                }

                public event EventHandler CanExecuteChanged
                {
                        add => CommandManager.RequerySuggested += value;
                        remove => CommandManager.RequerySuggested -= value;
                }

                public bool CanExecute(object parameter)
                {
                        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
                }

                public async void Execute(object parameter)
                {
                        if (!CanExecute(parameter))
                                return;

                        try
                        {
                                _isExecuting = true;
                                CommandManager.InvalidateRequerySuggested();
                                await _execute(parameter);
                        }
                        finally
                        {
                                _isExecuting = false;
                                CommandManager.InvalidateRequerySuggested();
                        }
                }
        }
}
