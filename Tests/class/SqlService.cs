using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static Tests.SqlService;

namespace Tests
{
    public  static class SqlService
    {
        public enum DBProcedure
        {
            READ_CLIENT,
            ADD_NEW_CLIENT,
            GET_EMPLOYER_ID,//сервис
            GET_TABLE_HEADERS//сервис
        }
        public enum DBNamesTable
        {
            Client,
            Post,
            Employer,
            User
        }

        //МЕТОДЫ СЕРВИСЫ

        private static string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;

        //Метод для полечения параметра процедуры
        private static List<string> GetPatametrs(DBProcedure procedureName)
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

        //Метод, для получения названия всех сущностей таблицы
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

        public static Dictionary<string, string> JoinParametrs<T>(T classObj, DBProcedure procedureName)
        {

            List<string> procedureParametrs = GetPatametrs(procedureName);
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            Type typeClass = typeof(T);
            string clearProcParametr = String.Empty;

            //перебираем свойства процедуры
            for (int i = 0; i < procedureParametrs.Count; i++)
            {
                clearProcParametr = procedureParametrs[i].Replace("@", "");

                //Перебираем свойства класса
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

        public static int GetIdEmployer(string login)
        {
            //Должны вернуть объект класса модели
            SqlConnection conn = new SqlConnection(connect);
            int result=-1;
            using(SqlCommand cmd = new SqlCommand(DBProcedure.GET_EMPLOYER_ID.ToString(), conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                List<string> listParametrs= GetPatametrs(DBProcedure.GET_EMPLOYER_ID);
                //ДОДЕЛАТЬ!

        


            }
            return result;

               
        }

        public static class SQLClient
        {

            //Чтение
            public static ObservableCollection<Client> Read( string login)
            {
                
                SqlConnection con = new SqlConnection(connect);
                try
                {
                    ObservableCollection<Client> result = new ObservableCollection<Client>();
                    using (SqlCommand cmd = new SqlCommand(DBProcedure.READ_CLIENT.ToString(), con))
                    {
                        #region
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;

                        Type _class = typeof(Client);
                        List<string> PrParam = GetPatametrs(DBProcedure.READ_CLIENT);
                        List<string> classProperty = new List<string>();

                        foreach (var item in _class.GetProperties()) 
                            classProperty.Add(item.Name.ToString());
                        

                        string[] classPropertyBasa = new string[classProperty.Count];
                        classProperty.CopyTo(classPropertyBasa);

                        cmd.Parameters.AddWithValue(PrParam[0].ToString(), login);
                        SqlDataReader reader = cmd.ExecuteReader();
                        #endregion
                        while (reader.Read())
                        {
                            Client addClient = new Client();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string tableheader = reader.GetName(i).ToString();

                                for (int j = 0; j < classProperty.Count; j++)
                                {
                                    if (!classProperty.Contains(tableheader)) break;
                                    if (classProperty[j] == reader.GetName(i).ToString())
                                    {
                                        var pr = _class.GetProperty(classProperty[j]);
                                        pr.SetValue(addClient, reader[i].ToString());
                                        classProperty.Remove(classProperty[j]);
                                        break;

                                    }

                                }

                            }

                            classProperty.Clear();
                            classProperty.AddRange(classPropertyBasa);
                            result.Add(addClient);

                        }
                        reader.Close();
                    }
                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if(con.State != ConnectionState.Open) con.Close();
                }
            }
            //Добавление
            public static void Add(string login, Client addClient)
            {
                SqlConnection con = new SqlConnection(connect);
                try
                {
                    Dictionary<string, string> ListProcedure_value = JoinParametrs(addClient, DBProcedure.ADD_NEW_CLIENT);
                    string commandProcedure = DBProcedure.ADD_NEW_CLIENT.ToString();

                    //Теперь в процедуру, нужно добавить все значения
                    using (SqlCommand cmd = new SqlCommand(commandProcedure, con))
                    {
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@login", login);
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
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (con.State != ConnectionState.Open) con.Close();
                }
            }

            //Удаление



        }




    }
}

