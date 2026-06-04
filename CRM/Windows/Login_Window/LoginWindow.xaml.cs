using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CRM.Servise;

namespace CRM.Windows
{
    /// <summary>
    /// Логика взаимодействия для Registration_Window.xaml
    /// </summary>
    public partial class Registration_Window : Window
    {
        VM_Login _vm ;
        Settings _settings ;

        
        public Registration_Window()
        {
            InitializeComponent();
            _settings = new Settings(new MessageeServise(this), new WindowService(),new User());

            _vm = new VM_Login(_settings);

            DataContext = _vm;

        }
        
        private void bt_Login_Click(object sender, RoutedEventArgs e)
        {
            _vm.Password = GetPassword(tb_Password.SecurePassword.Copy());
            tb_Password.Clear();
        }

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


    }
}
