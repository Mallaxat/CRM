using CRM.Servise;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CRM.Windows
{
    public class VM_NewClient : INotifyPropertyChanged
    {
        //Настройки
        private readonly Settings _settings;

        //Свойства
        private string firstName;
        public string FirstName 
        {
            get => firstName;
            set
            {
                if (value == null || value == firstName) return;
                firstName = value;
                OnPropertyChanged();
            }
        }
        
        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                if (value == null || value == lastName) return;
                lastName = value;
                OnPropertyChanged();
            }
        }
       
        private string middleName;
        public string MiddleName
        {
            get => middleName;
            set
            {
                if (value == null || value == middleName) return;
                middleName = value;
                OnPropertyChanged();
            }
        }
        
        private string phoneNumber;
        public string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                if (value == null || value == phoneNumber) return;
                phoneNumber = value;
                OnPropertyChanged();
            }
        }


        //Интерфейс
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
        //Команды
        ICommand AddClient;
        
        //Конструктор
        public VM_NewClient(Settings settings)
        {
            _settings = settings;
            AddClient = new RelayCommand(_ => 
            {
                ADD();
            });

        }
        //Методы
        private void ADD()
        {

        }
    }
}
