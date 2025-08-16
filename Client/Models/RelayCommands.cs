using System;
using System.Windows.Input;

namespace Client.Models
{
    public class RelayCommand : ICommand
    {
        // 매개변수 없는 메서드를 위한 필드와 생성자
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        // 매개변수 있는 메서드를 위한 필드와 생성자
        private readonly Action<object> _executeWithParameter;
        private readonly Func<object, bool> _canExecuteWithParameter;

        // 매개변수 없는 메서드를 위한 생성자
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 매개변수 있는 메서드를 위한 생성자
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _executeWithParameter = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteWithParameter = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute();
            if (_canExecuteWithParameter != null)
                return _canExecuteWithParameter(parameter);

            return true;
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute();
            else if (_executeWithParameter != null)
                _executeWithParameter(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}