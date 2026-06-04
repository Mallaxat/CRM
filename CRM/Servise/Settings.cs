using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Servise
{
  public class Settings
    {
        public readonly MessageeServise serviseMessege;
        public readonly WindowService serviseWindow;
        public readonly SqlService serviseSQL;
        public string Password {  get; set; }

        public User user;

        public Settings(MessageeServise serviseMessege, WindowService serviseWindow, User user)
        {
            this.serviseMessege = serviseMessege;
            this.serviseWindow = serviseWindow;
            this.serviseSQL = SqlService.Instance;
            this.user = user;
            if(user==null) user=new User();
        }
    }
}
