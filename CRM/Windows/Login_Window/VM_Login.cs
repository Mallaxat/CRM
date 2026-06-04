
using CRM.Servise;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CRM.Windows
{
    class VM_Login : INotifyPropertyChanged
    {
        //Интерфейс
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        //Настройки
        private readonly Settings _settings;
        //Свойства
        //public User user {  get; set; }=new User();
        private string login;
        public string Login
        { get => login;
            set
            {
                if (value == null || login==value) return;
                login= value;
                OnPropertyChanged();


            }
        }
        private string password;
        public string Password
        {
            get => password;
            set
            {
                if (value.Length==0) return;
                password = value;
                OnPropertyChanged();
            }
        }
        
        //Команды
        public ICommand cRegistration { get; }
        public ICommand cLogin { get; }

        //Конструкторы
        public VM_Login(Settings settings)
        {
            _settings = settings;
            cRegistration = new RelayCommand(_ =>
            {
                _settings.serviseWindow.WindowOpen<RegistrationWindow>(new VM_Registration(_settings));
            });
            cLogin = new RelayCommand(
                _=> LoginUser(),
                _ => CanLoginUser()
         );
        }

        //Методы
        private bool CanLoginUser()
        {
            return !string.IsNullOrEmpty(login);
        }

        private void LoginUser()
        {
            _settings.user.Login = Login;
            _settings.user.Password = Password;

            if (LoginService.Instance.Login(_settings.user))
            {
                _settings.serviseMessege.Show("Успешный вход","Вход");
                _settings.user = LoginService.Instance.GetUser(_settings.user);
                var VM_Main = new VM_Main(_settings);
                _settings.serviseWindow.WindowOpenAndClose<MainWindow>(VM_Main);
            }
            else _settings.serviseMessege.Show("Пользователя не существует", "Вход"); ;
        }

    }
}
