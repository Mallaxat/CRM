using CRM.Servise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CRM
{

    public class MessageeServise : IntMassege
    {
        private readonly Window _window;
        public MessageeServise(Window window)
        {
            _window= window;
        }
        public void Show(string message)
        {
            MessageBox.Show(message);
        }

        public void Show(string message, string title)
        {
            MessageBox.Show(message);
        }
    }
}
