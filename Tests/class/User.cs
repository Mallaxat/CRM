using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class User:Employer
    {
        public string Login { get; set; }
        public String Password { get; set; }
        public int PostId { get;  set; }
        public string PostName { get; set; }
        public User()
        {

        }
    }
}
