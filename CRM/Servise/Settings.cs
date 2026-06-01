using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM
{
  public class Settings
    {
        public readonly MessageeServise serviseMessege;
        public readonly WindowService serviseWindow;
        public User user;

        public Settings(MessageeServise serviseMessege, WindowService serviseWindow, User user)
        {
            this.serviseMessege = serviseMessege;
            this.serviseWindow = serviseWindow;
            this.user = user;
            if(user==null) user=new User();
        }
    }
}
