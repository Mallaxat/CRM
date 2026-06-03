using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CRM
{
    public class User:Employer
    {
        private List<string> posts {  get; set; }=new List<string>();
        public string Login { get; set; }
        public String Password { get; set; }
        public string PostName { get;  set; }


        public User()
        {}



    }
}
