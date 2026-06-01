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
        public string Login { get; set; }
        public SecureString Password { get; set; }
        public int Post { get;  set; }

        public User()
        {

        }
    }
}
