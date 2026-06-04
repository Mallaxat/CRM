using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static Tests.Program;

namespace Tests
{

    internal class Program
    {
        static void Main(string[] args)
        {

            Client client = new Client
            {
                FirstName="Petr",
                MiddleName="Petrovic",
                LastName="Petrov",
                PhoneNumber="888888888"
            };



            SqlService.SQLClient.Add("Admin", client);
            ObservableCollection<Client> clients = SqlService.SQLClient.Read("Admin");

            foreach (var i in clients)
            {
                Console.WriteLine($"{i.FirstName,15}{i.LastName,15}{i.MiddleName,15}{i.PhoneNumber,15}");
            }

        }
    }
}
