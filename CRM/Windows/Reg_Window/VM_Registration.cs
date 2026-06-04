using CRM.Servise;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CRM.Windows
{
    class VM_Registration : INotifyPropertyChanged
    {
        //Настройки
        private readonly Settings _settings;

        //Свойства
        public List<string> Posts { get; set; } = new List<string>();
        public string SelectPosts { get; set; }=String.Empty;

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                OnPropertyChanged();
            }
        }

        private string fname;
        public string FName
        {
            get => fname;
            set
            {
                if (fname == value) return;
                fname = value;
                OnPropertyChanged();
            }
        }

        private string patronymic;
        public string Patronymic
        {
            get => patronymic;
            set
            {
                if (patronymic == value) return;
                patronymic = value;
                OnPropertyChanged();
            }
        }
       
        private string login;
        public string Login
        {
            get => login;
            set
            {
                if (value == null || login == value) return;
                login = value;
                OnPropertyChanged();


            }
        }
        
        private SecureString password;
        public SecureString Password
        {
            get => password;
            set
            {
                if (value.Length == 0) return;
                password = value;
                OnPropertyChanged();
            }
        }
        //Команды
        public ICommand AddRegistration { get; }
        
        //Интерфейс
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
       
        //Конструктор
        public VM_Registration(Settings settings)
        {
            _settings = settings;

            Posts = SQLOLD.Instance.ReadDBColumn(DBProcedure.READ_COLUMN_TABLE, DBNamesTable.Post, "PostName");

             AddRegistration = new RelayCommand(  
                _ => Registration(),
                _ => CanRegistration()
            );

        }

        //Методы
        //ЭТО НЕ ПРАВИЛЬНО НАДО БУДЕТ ЧТО_ТО ПРИДУМАТЬ ЭТО КОСТЫЛЬ!
        private string GetPassword(SecureString password)
        {
            string pas = String.Empty;
            IntPtr intPassword = Marshal.SecureStringToBSTR(password);

            try
            {
                for (int i = 0; i < password.Length; i++)
                {
                    char firstChar = (char)Marshal.ReadInt16(intPassword, i * 2);
                    pas += firstChar.ToString();
                }
                return pas;
            }
            finally
            {
                if (intPassword != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(intPassword);
            }
        }
        public bool CanRegistration()
        {
            if ( !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(FName) && !String.IsNullOrEmpty(Patronymic)
                && !String.IsNullOrEmpty(Login)) return true;
            return false;
        }
        public void Registration()
        {
            string massege=LoginService.Instance.CheckPassword(Password);
            if(massege != null)
            {
                _settings.serviseMessege.Show(massege, "Не правильный пароль");
                return;
            }
            User user = new User
            {
                FirstName = this.Name,
                LastName = this.FName,
                MiddleName = this.Patronymic,
                Login = this.Login,
                Password = GetPassword(this.Password),
                PostName = CheckPost().ToString()
            };
            if (LoginService.Instance.IsLogin(user))
            {
                _settings.serviseMessege.Show("Пользователь уже существует", "Регистрация");
                return;
            }
            if (LoginService.Instance.Registration(user)) _settings.serviseMessege.Show("Регистрация прошла успешно!", "Регистрация");
            else _settings.serviseMessege.Show("Регистрация неудалась!", "Регистрация");
        }
        private int CheckPost()
        {
            for(int i=0;i<Posts.Count;i++)
            {
                if (SelectPosts == Posts[i]) return i + 1; 
            }
            return 0;

        }
    }
}
