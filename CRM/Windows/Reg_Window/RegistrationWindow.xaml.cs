using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CRM.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        VM_Registration _vm;


        public RegistrationWindow()
        {
            InitializeComponent();

        }

        private void bt_enter_Click(object sender, RoutedEventArgs e)
        {
            _vm.Password = tb_Pass.SecurePassword.Copy();
            tb_Pass.Clear();
        }

        private void tb_Pass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Password = tb_Pass.SecurePassword.Copy();
        }
    }
}
