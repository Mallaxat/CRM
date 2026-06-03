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
        static string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;
        public enum Procedure
        {
            ADD_NEW_USER,

        }
        public enum DBProcedure
        {
            ADD_NEW_USER,
            READ_ONE_TABLE,
            READ_TWO_TABLE,
            GET_TABLE_HEADERS
        }
        public enum DBNamesTable
        {
            Client,
            Post,
            Employer,
            User
        }
        //СТАТИКИ УБРАТЬ

        //Метод для полечения параметра процедуры
        public static List<string> GetPatametrs(DBProcedure procedureName)
        {
            string procedure = "GET_PROCEDURE_PARAMETRS";
            //Если процедура равна процедуре возврата параметров
            if (procedureName.ToString() == procedure) return null;

            List<string> result = new List<string>();
            using (SqlConnection conn = new SqlConnection(connect))
            {
                //Шапка созадния команды-процедуры
                conn.Open();
                SqlCommand cmd = new SqlCommand(procedure, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Добавляем процедуру-аргумент в качестве параметра
                cmd.Parameters.AddWithValue("@ProcedureName", procedureName.ToString());

                //Считываем значения
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(reader["ParameterName"].ToString());
                }
                if (result.Count > 0) return result;
                else return null;
            }

        }

        //Метод соединяющий параметр процедуры и значение для этой процедуры
        public static Dictionary<string,string> JoinParametrs <T>(T classObj,List<string> procParametr)
        {
            Dictionary<string, string> result= new Dictionary<string,string>();
            Type typeClass = typeof(T);
            string clearProcParametr=String.Empty;

            //перебираем свойства процедуры
            for(int i=0;i<procParametr.Count;i++)
            {
                clearProcParametr = procParametr[i].Replace("@","");

                //Перебираем свойства класса
                //j=propertyinfo
                foreach(var proInfo in typeClass.GetProperties())
                {
                    if(proInfo.Name==clearProcParametr)
                    {
                        object value = proInfo.GetValue(classObj);
                        result.Add($"@{clearProcParametr}", value.ToString());
                        break;
                    }
                }
            }
            return result;
        }


        private static List<string> GetTableHeaders(DBNamesTable tableName)
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

        public static ObservableCollection<T> ReadDB<T>(DBProcedure procedure, DBNamesTable tableName) where T : new()
        {
            //Тут база данных мне возвращает массив, с разными полями 
            //Процедура она принимает как значение название таблицы
            SqlConnection con=new SqlConnection(connect);
            ObservableCollection<T> reslt=new ObservableCollection<T>();

            using (SqlCommand cmd=new SqlCommand(procedure.ToString(), con))
            {          
                //Параметры, которые данная процедура принимает
                List<string> parametrs = GetPatametrs(procedure);

                //Заголовки этой таблицы
                List<string> tableHead = GetTableHeaders(tableName);


                Type typeClass=typeof(T);
                //Свойства класса
                List<string> classProperty=new List<string>();
                foreach (var i in typeClass.GetProperties())             
                {
                    classProperty.Add(i.Name);
                }



                con.Open();
                cmd.CommandType= CommandType.StoredProcedure;

                if (parametrs.Count != 1) return null;

                //Заполнение параметров
                cmd.Parameters.AddWithValue(parametrs[0].ToString(), tableName.ToString());

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    T classObj = new T();

                    for (int i = 0; i <= classProperty.Count-1; i++)
                    {
                        for(int j=0;j<= tableHead.Count-1;j++)
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


        public static ObservableCollection<T> ReadDB<T, K>(DBProcedure procedure, DBNamesTable tableName, DBNamesTable tableName2)
            where T : new()
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
                List<string> tableHead = new List<string>();


                Type typeClass = typeof(T);
                //Свойства класса
                List<string> classProperty = new List<string>();
                foreach (var i in typeClass.GetProperties())
                {
                    classProperty.Add(i.Name);
                }
                string [] classPropertyBasa = new string [classProperty.Count];
                classProperty.CopyTo(classPropertyBasa);


                con.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                if (parametrs.Count != 2) return null;

                List<string> tablNames = new List<string>();
                tablNames.Add(tableName.ToString());
                tablNames.Add(tableName2.ToString());

                for(int i=0;i<parametrs.Count;i++)
                {
                    //Заполнение параметров
                    cmd.Parameters.AddWithValue(parametrs[i].ToString(), tablNames[i].ToString());
                }


                SqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    T classObj = new T();
                   
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string tableheader = reader.GetName(i).ToString();
                        for(int j=0; j < classProperty.Count; j++)
                        {
                            if (!classProperty.Contains(tableheader)) break;
                            if (classProperty[j] == reader.GetName(i).ToString())
                            {
                                var pr = typeClass.GetProperty(classProperty[j]);
                                pr.SetValue(classObj, reader[i]);
                                classProperty.Remove(classProperty[j]);
                                break;
                            }
                        }

                       
                    }
                    classProperty.Clear();
                    classProperty.AddRange(classPropertyBasa);
                    reslt.Add(classObj);
                }

            }

            return reslt;
        }



        static void Main(string[] args)
        {

            //Тест метода возвращения параметров
            #region
            List<string> ProcedureParametrs=GetPatametrs(DBProcedure.ADD_NEW_USER);
            #endregion

            //Тест метода для передачи параметров из класса в процедуру
            
            User user = new User{ FirstName="Имя",
                                   LastName="Фамилия",
                                   MiddleName="Отчество",
                                   Login="Логин",
                                   Password="111"};


            /*
                        Dictionary<string, string> result = new Dictionary<string, string>();
                        result=JoinParametrs(user, ProcedureParametrs);

                        Console.WriteLine();
                        #endregion
            */

            #region
            List<string> tablehander = GetTableHeaders(DBNamesTable.User);
            Console.WriteLine();


            //ObservableCollection<Client> clients = new ObservableCollection<Client>();
            //clients= ReadDB<Client>(DBProcedure.READ_ONE_TABLE, DBNamesTable.Client);

            //ObservableCollection<Employer> users = new ObservableCollection<Employer>();
            //users = ReadDB<Employer>(DBProcedure.READ_ONE_TABLE, DBNamesTable.Employer);
            //foreach(var i in users)
            //{
            //    Console.WriteLine(i.FirstName.ToString());
            //}

            ObservableCollection<User> users=new ObservableCollection<User>();
            users= ReadDB<User,Employer>(DBProcedure.READ_TWO_TABLE, DBNamesTable.User,DBNamesTable.Employer);

            foreach(var i in users)
            {
                Console.WriteLine(i.FirstName);
            }
            #endregion
        }
    }
}
