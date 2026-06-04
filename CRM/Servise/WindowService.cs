using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace CRM.Servise
{
    public class WindowService
    {
        public void WindowOpen<T>() where T : Window,new()
        {
            T window=new T();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }
        public void WindowOpen<T>(object VM) where T : Window, new()
        {
            T window = new T()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext= VM
            };
            window.ShowDialog();
        }

        public void WindowOpenAndClose<T>() where T : Window, new()
        {
            T window = new T();
            //Найти текущее активное окно
            Window currentWindow = Application.Current.MainWindow;

            //Назначаем новое активное окно
            if(Application.Current.MainWindow==currentWindow)
            {
                Application.Current.MainWindow = window;
            }

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowState = WindowState.Maximized;
            window.Show();

            currentWindow.Close();
        }
        public void WindowOpenAndClose<T>(object VM) where T : Window, new()
        {
            T window = new T()
            {
                DataContext = VM,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            //Найти текущее активное окно
            Window currentWindow = Application.Current.MainWindow;

            //Назначаем новое активное окно
            if (Application.Current.MainWindow == currentWindow)
            {
                Application.Current.MainWindow = window;
            }

            window.Show();

            currentWindow.Close();
        }

        public Page PageOpen<VM,P>(Settings setting) where P : Page
        {
            //Создаст нужную мне VMку и её сразу как контекст даст в пейдж
            var VM_Page=(VM)Activator.CreateInstance(typeof(VM),setting);
            var page = (P)Activator.CreateInstance<P>();
            page.DataContext = VM_Page;
            return page;
        }
    }
}
