using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace TestsSQL
{
    internal class Program
    {
        class Employer
        {
            public string Name { get; set; }

            public string FName { get; set; }

            public string Patronymic { get; set; }

        }
        class User : Employer
        {
            public string Login { get; set; }
            public string Password { get; set; }
            public User()
            { }
        }
        static void Main(string[] args)
        {
            ObservableCollection<User> collection = new ObservableCollection<User>();
            string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;
            string command = "SELECT * FROM [User] LEFT JOIN Employer ON Employer.UserId = [User].UserId;";
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                SqlCommand cdm = new SqlCommand(command, con);
                SqlDataReader reader = cdm.ExecuteReader();

                while (reader.Read())
                {
                    var name = reader["FirstName"];
                    var secondName = reader["MiddleName"];
                    var thirdName = reader["LastName"];
                    var login = reader["Login"];
                    var password = reader["Password"];
                    collection.Add(new User
                    {
                        Name = name.ToString(),
                        FName = secondName.ToString(),
                        Patronymic = thirdName.ToString(),
                        Login = login.ToString(),
                        Password = password.ToString()
                    });
                }
            }
            foreach(var i in collection)
            {
                WriteLine(i.Login);
            }
        
        }
    }
}
