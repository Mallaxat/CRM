using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CRM
{
    public enum DBProcedure
    {
        ADD_NEW_USER,
        READ_ONE_TABLE,
        GET_TABLE_HEADERS
    }
    public enum DBNamesTable
    {
        Client,
        Post,
        Employer,
        User
    }

    class SqlService
    {
        //Свойства
        public static SqlService Instance {  get; set; } = new SqlService();
        private List<string> procedureParametrs {  get; set; }=new List<string>();
        Dictionary<string, string> ListProcedure_value { get; set; } = new Dictionary<string, string>();
        
        //Конструктор
        private SqlService()
        {
            Instance = this;
        }
        //КОСТЫЛЬ
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


        //МЕТОДЫ СЕРВИСЫ

        private string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;
        
        //Метод для полечения параметра процедуры
        private List<string> GetPatametrs(DBProcedure procedureName)
        {
            string procedure = "GET_PROCEDURE_PARAMETRS";
            //Если процедура равна процедуре возврата параметров
            if (procedureName.ToString() == procedure) return null;

            List<string> result = new List<string>();
            using(SqlConnection conn = new SqlConnection(connect))
            {
                //Шапка созадния команды-процедуры
                conn.Open();
                SqlCommand cmd=new SqlCommand(procedure, conn);
                cmd.CommandType=CommandType.StoredProcedure;
                
                //Добавляем процедуру-аргумент в качестве параметра
                cmd.Parameters.AddWithValue("@ProcedureName",procedureName.ToString());

                //Считываем значения
                SqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    result.Add(reader["ParameterName"].ToString());
                }
                if(result.Count>0) return result;
                else return null;
            }
        }
        
        //Метод, для получения названия всех сущностей таблицы
        private  List<string> GetTableHeaders(DBNamesTable tableName)
        {
            SqlConnection con = new SqlConnection(connect);
            try
            {
                DBProcedure procedure = DBProcedure.GET_TABLE_HEADERS;

                List<string> patametrs = GetPatametrs(procedure);

                using (SqlCommand cmd = new SqlCommand(procedure.ToString(), con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var i in patametrs)
                    {
                        cmd.Parameters.AddWithValue(i.ToString(), tableName.ToString());
                    }
                    var listHeaderString = cmd.ExecuteScalar();
                    List<string> result = new List<string>();
                    result.AddRange(listHeaderString.ToString().Split(','));
                    return result;
                }
            }
            catch (Exception ex) { }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            return null;
        }


        //ПУБЛИЧНЫЕ МЕТОДЫ 

        //Метод соединяющий параметр процедуры и значение для этой процедуры
        public Dictionary<string, string> JoinParametrs<T>(T classObj, DBProcedure procedureName)
        {
            if(procedureParametrs.Count!=0)procedureParametrs.Clear();

            procedureParametrs = GetPatametrs(procedureName);

            Dictionary<string, string> result = new Dictionary<string, string>();
            Type typeClass = typeof(T);
            string clearProcParametr = String.Empty;

            //перебираем свойства процедуры
            for (int i = 0; i < procedureParametrs.Count; i++)
            {
                clearProcParametr = procedureParametrs[i].Replace("@", "");

                //Перебираем свойства класса
                //j=propertyinfo
                foreach (var proInfo in typeClass.GetProperties())
                {

                    if (proInfo.Name == clearProcParametr)
                    {
                        object value = proInfo.GetValue(classObj);
                        result.Add($"@{clearProcParametr}", value.ToString());
                        break;
                    }
                }
            }
            return result;
        }

        //Универсальный метод
        public void AddDB<T>(DBProcedure procedure,T classObj)
        {
            SqlConnection con = new SqlConnection(connect);
            try
            {
                if (ListProcedure_value.Count != 0) ListProcedure_value.Clear();
                ListProcedure_value = JoinParametrs(classObj, procedure);

                string commandProcedure = procedure.ToString();
                con.Open();
                //Теперь в процедуру, нужно добавить все значения
                using (SqlCommand cmd = new SqlCommand(commandProcedure, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //Теперь нужно передать все параметры в процедуру
                    foreach (KeyValuePair<string, string> pair in ListProcedure_value)
                    {
                        string key = pair.Key;
                        string value = pair.Value;

                        cmd.Parameters.AddWithValue(key, value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { }
       
            finally
            { 
                if(con.State==ConnectionState.Open) con.Close();
            }

        }


        public ObservableCollection<T> ReadDB<T>(DBProcedure procedure, DBNamesTable tableName) where T : new()
        {
            //Тут база данных мне возвращает массив, с разными полями 
            //Процедура она принимает как значение название таблицы
            SqlConnection con = new SqlConnection(connect);
            ObservableCollection<T> reslt = new ObservableCollection<T>();

            using (SqlCommand cmd = new SqlCommand(procedure.ToString(), con))
            {
                //Параметры, которые данная процедура принимает
                List<string> parametrs = GetPatametrs(procedure);

                //Заголовки этой таблицы
                List<string> tableHead = GetTableHeaders(tableName);


                Type typeClass = typeof(T);
                //Свойства класса
                List<string> classProperty = new List<string>();
                foreach (var i in typeClass.GetProperties())
                {
                    classProperty.Add(i.Name);
                }

                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                if (parametrs.Count != 1) return null;

                //Заполнение параметров
                cmd.Parameters.AddWithValue(parametrs[0].ToString(), tableName.ToString());

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    T classObj = new T();

                    for (int i = 0; i <= classProperty.Count - 1; i++)
                    {
                        for (int j = 0; j <= tableHead.Count - 1; j++)
                        {
                            if (classProperty[i] == tableHead[j])
                            {
                                var pr = typeClass.GetProperty(classProperty[i]);
                                pr.SetValue(classObj, reader[j]);
                                break;
                            }

                        }
                    }
                    reslt.Add(classObj);
                }

            }

            return reslt;
        }


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
                        FirstName = name.ToString(),
                        MiddleName = secondName.ToString(),
                        LastName = thirdName.ToString(),
                        Login = login.ToString(),
                        Password = password.ToString(),
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

/*        public void ReadClients(ObservableCollection<Client> collection,User user)
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
                        FirstName = name.ToString(),
                        MiddleName = secondName.ToString(),
                        LastName = thirdName.ToString(),
                        PhoneNumber = phone.ToString(),
                    });
                }
            }

        }*/



    }
}
