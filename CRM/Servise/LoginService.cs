using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CRM.Servise
{
    class LoginService
    {
        private char[] forbiddenSymbols = { '!', '@', '#', '$'};
        public static LoginService Instance { get; private set; }=new LoginService();

       private ObservableCollection<User> ListUsers { get; set; }=new ObservableCollection<User>();

       private const int PASLENGHT= 1;

       private LoginService()        
        {
            Instance = this;
            //Тут пока проблемки, нужно юзер и емплоер джоинить
            // SqlService.Instance.ReadUser(ListUsers);
            //ListUsers = SqlService.Instance.ReadDB<User, Employer>(DBProcedure.READ_TWO_TABLE, DBNamesTable.User, DBNamesTable.Employer);
            ListUsers = SQLOLD.Instance.ReadDB<User, Employer>(DBProcedure.READ_TWO_TABLE, DBNamesTable.User, DBNamesTable.Employer);
        }
    
        //Регистрация
       public bool Registration(User user)
        {
            if(user== null) return false;
            ListUsers.Add(user);
            SQLOLD.Instance.AddDB(DBProcedure.ADD_NEW_USER, user); 
            return ListUsers.Contains(user);
        }
        //Логирование 
       public bool Login(User user)
        {
            if(ListUsers==null)return false;
            foreach(var i in ListUsers)
            {
                if(i.Login==user.Login && i.Password== user.Password ) return true;
            }
            return false;
        }
        //Существует ли пользователь
        public bool IsLogin(User user)
        {
            if (ListUsers == null) return false;
            foreach (var i in ListUsers)
            {
                if (i.Login == user.Login) return true;
            }
            return false;
        }
        public User GetUser(User user)
        {
            foreach (var i in ListUsers)
            {
                if (i.Login == user.Login && i.Password == user.Password) return i;
            }
            return null;
        }
        //Метод,для проверки совпадения пароля его пока оставлю
        public bool SecureStringCheck(SecureString first, SecureString two)
        {
            if (first == null || two == null) return false;
            if (first.Length != two.Length) return false;
            IntPtr sfirst = Marshal.SecureStringToBSTR(first);
            IntPtr stwo = Marshal.SecureStringToBSTR(two);

            try
            {
                for (int i = 0; i < first.Length; i++)
                {
                    char firstChar = (char)Marshal.ReadInt16(sfirst, i * 2);
                    char secondChar = (char)Marshal.ReadInt16(stwo, i * 2);

                    if (firstChar != secondChar)
                        return false;
                }

                return true;

            }
            finally
            {
                if (sfirst != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(sfirst);

                if (stwo != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(stwo);
            }
        }

        //Метод для проверки корректности пароля
        public string CheckPassword(SecureString password)
        {
            return OptionsPassword(GetPassword(password));
        }


        //Вернет 0 если пароль соответствует правилам
        private string OptionsPassword(string password)
        {
            if (password == null) return $"Пароль не заполнен ";
            if (password.Length < PASLENGHT) return $"Пароль должен быть больше {PASLENGHT} ";
            foreach(var i in forbiddenSymbols)
            {
                if (password.Contains(i.ToString())) return $"Пароль содержит запрещенный символ {i}";
            }
            return null;

        }
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

    }
}
