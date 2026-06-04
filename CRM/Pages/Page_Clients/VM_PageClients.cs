using CRM.Servise;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CRM.Windows;

namespace CRM.Pages
{
    public class VM_PageClients : INotifyPropertyChanged
    {

        //Интерфейс
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        //Настройки
        private Settings _settings;
        public Settings _Settings
        {
            get => _settings;
            set
            {
                if (_settings == value || value == null) return;
                _settings = value;
                ListClients = SqlService.Instance.ReadDB<Client>(DBProcedure.READ_ONE_TABLE_CONDITION, DBNamesTable.Client, _Settings.user.Login);
                OnPropertyChanged();
            }
        }

        //Свойства
        public ObservableCollection<Client> ListClients { get; set; } = new ObservableCollection<Client>();

        //Команды
        public ICommand AddClient;
        public ICommand DeleteClient;

        public VM_PageClients(Settings settings)
        {
            _Settings = settings;
            
            AddClient = new RelayCommand(_ => 
            { 
                Add_Client();
            });

            DeleteClient = new RelayCommand(_ => 
            {
                Delete_Client();
            }
            );
        }
        //Методы
        private void Add_Client()
        {
            _settings.serviseWindow.WindowOpen<NewClientWindow>(new VM_NewClient(_settings));
        }
        private void Delete_Client()
        {

        }
    }
    
}
