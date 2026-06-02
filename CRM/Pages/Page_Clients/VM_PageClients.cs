using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CRM
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
        Settings _setting {  get; set; }
        //Свойства
        public ObservableCollection<Client> ListClients {  get; set; }=new ObservableCollection<Client>();

        public VM_PageClients(Settings settings)
        {
            _setting = settings;
            //Тут юзер пустой почти
            //SqlService.Instance.ReadClients( ListClients, _setting.user);
            ListClients=SqlService.Instance.ReadDB<Client>(DBProcedure.READ_ONE_TABLE, DBNamesTable.Client);
        }

    }
    
}
