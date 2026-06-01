using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM
{
    class Manager : Employer
    {
        private ObservableCollection<Employer> ListEmployer {  get; set; }=new ObservableCollection<Employer>();
        public Manager()
        {}

    }
}
