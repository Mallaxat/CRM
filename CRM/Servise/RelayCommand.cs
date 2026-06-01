using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CRM.Servise
{
    public class RelayCommand : ICommand
    {
        //Это делегаты - указывают на методы
        //Action представляет некоторое действие, которое ничего не возвращает
        private readonly Action<object> _execute;

        //Func возвращает результат действия и может принимать параметры
        private readonly Func<object, bool> _canExecute;


        //Конструктор, принимает 2 параметра в качестве функции и последний по дефолту пуст
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        //Можно ли выполнять команду или нельзя
        public bool CanExecute(object parameter)
        {
            //_canExecute(parameter) вызвать функцию с указанным параметром, которая тут хранится
            return _canExecute == null || _canExecute(parameter);
        }
        //
        public void Execute(object parameter)
        {
            //Вызов функции с указанным параметром
            _execute(parameter);
        }
        //Событие, на него подпишется сам WPF когда будет делать привязку
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

    }
}
