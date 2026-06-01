using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CRM
{
    class SqlService
    {
        public static SqlService Instance {  get; set; } = new SqlService();
        private SqlService()
        {
            Instance = this;
        }

        private string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;

        public void ReadUser( ObservableCollection<User> collection)
        {
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
                        Patronymic = secondName.ToString(),
                        FName = thirdName.ToString(),
                        Login = login.ToString(),
                        Password = ToSecureString(password.ToString()),
                    });
                }
            }

        }
        public void ReadPost(List<string> collection)
        {
            string command = "SELECT * FROM Post ";
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                SqlCommand cdm = new SqlCommand(command, con);
                SqlDataReader reader = cdm.ExecuteReader();

                while (reader.Read())
                {
                    var post = reader["PostName"];
                    collection.Add(post.ToString());
                }
            }

        }

        public void ReadClients(ObservableCollection<Client> collection,User user)
        {
            string command = $"DECLARE @Login NVARCHAR(100) = N'{user.Login.ToString()}';" +
                "SELECT * FROM Client " +
                "WHERE EmployerId = ( SELECT Employer.EmployerId " +
                "FROM Employer INNER JOIN [User] ON Employer.UserId = [User].UserId " +
                "WHERE [User].Login = @Login" +
                ");";


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
                    var phone = reader["PhoneNumber"];

                    collection.Add(new Client
                    {
                        Name = name.ToString(),
                        Patronymic = secondName.ToString(),
                        FName = thirdName.ToString(),
                        Phone = phone.ToString(),
                    });
                }
            }

        }


        public void AddUser(User user)
        {
            //Важная заметка, тут нет должности пока что
            string command = @"
INSERT INTO [User] (Login, Password)
VALUES (@Login, @Password);
DECLARE @NewUserId INT = SCOPE_IDENTITY();
INSERT INTO Employer (UserId, FirstName, LastName, MiddleName, PostId)
VALUES (@NewUserId, @FirstName, @LastName, @MiddleName, @PostId);
";
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();

                using (SqlCommand cdm = new SqlCommand(command, con))
                {
                    cdm.Parameters.AddWithValue("@Login", user.Login);
                    cdm.Parameters.AddWithValue("@Password", GetPassword(user.Password));
                    cdm.Parameters.AddWithValue("@FirstName", user.Name);
                    cdm.Parameters.AddWithValue("@MiddleName", user.Patronymic);
                    cdm.Parameters.AddWithValue("@LastName", user.FName);
                    cdm.Parameters.AddWithValue("@PostId", user.Post);
                    cdm.ExecuteNonQuery();
                }

            }
        }
       
        public static SecureString ToSecureString(string value)
        {
            if (value == null)
                return null;

            var secure = new SecureString();

            foreach (char c in value)
                secure.AppendChar(c);

            secure.MakeReadOnly();
            return secure;
        }

        private string GetPassword(SecureString password)
        {
            string pas = String.Empty;
            IntPtr intPassword = Marshal.SecureStringToBSTR(password);

            try
            {
                for (int i = 0; i < password.Length; i++)
                {
                    char firstChar = (char)Marshal.ReadInt16(intPassword, i * 2);
                    pas += firstChar.ToString();
                }
                return pas;
            }
            finally
            {
                if (intPassword != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(intPassword);
            }
        }

    }
}
