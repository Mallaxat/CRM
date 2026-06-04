using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CRM.Pages;
using CRM.Servise;

namespace CRM.Windows
{
    class VM_Main: INotifyPropertyChanged
    {

        //Настройки
        private readonly Settings _settings;

        //Свойства
        private Page currentpage;
        public Page CurrentPage
        {
            get => currentpage;
            set
            {
                currentpage = value;
                OnPropertyChanged();
            }
        }

        //Интерфейс
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        //Конструктор
        public VM_Main(Settings settings)
        { 
            this._settings = settings;
            CurrentPage=_settings.serviseWindow.PageOpen<VM_PageClients,PageClients>(_settings);

        }


    }
}
