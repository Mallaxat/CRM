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

namespace CRM.Servise
{
    public enum DBProcedure
    {
        ADD_NEW_USER,
        READ_ONE_TABLE,
        READ_TWO_TABLE,
        READ_COLUMN_TABLE,
        READ_ONE_TABLE_CONDITION,
        GET_TABLE_HEADERS
    }
    public enum DBNamesTable
    {
        Client,
        Post,
        Employer,
        User
    }

    public class SqlService
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

        //Метод для добавления новых данных в БД
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

        //Метод для считывания данных из БЖ
        public ObservableCollection<T> ReadDB<T>(DBProcedure procedure, DBNamesTable tableName) where T : new()
        {
            //Процедура она принимает как значение название таблицы
            SqlConnection con = new SqlConnection(connect);
            ObservableCollection<T> reslt = new ObservableCollection<T>();
            try { 
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
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                if(con.State==ConnectionState.Open) con.Close(); 
            }
            return reslt;
        }
        //Чтение по определенному юзеру
        public ObservableCollection<T> ReadDB<T>(DBProcedure procedure, DBNamesTable tableName,string condition) where T : new()
        {
            //Процедура она принимает как значение название таблицы
            SqlConnection con = new SqlConnection(connect);
            ObservableCollection<T> reslt = new ObservableCollection<T>();
            try
            {
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

                    if (parametrs.Count != 2) return null;


                    //Заполнение параметров
                    cmd.Parameters.AddWithValue(parametrs[0].ToString(), tableName.ToString());
                    cmd.Parameters.AddWithValue(parametrs[1].ToString(), condition);

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
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            return reslt;
        }
        public ObservableCollection<T> ReadDB<T, K>(DBProcedure procedure, DBNamesTable tableName, DBNamesTable tableName2)
       where T : new()
        {
            //Процедура она принимает как значение название таблицы
            SqlConnection con = new SqlConnection(connect);

            ObservableCollection<T> reslt = new ObservableCollection<T>();
            try
            {
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
                    string[] classPropertyBasa = new string[classProperty.Count];
                    classProperty.CopyTo(classPropertyBasa);


                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parametrs.Count != 2) return null;

                    List<string> tablNames = new List<string>();
                    tablNames.Add(tableName.ToString());
                    tablNames.Add(tableName2.ToString());

                    for (int i = 0; i < parametrs.Count; i++)
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
                            for (int j = 0; j < classProperty.Count; j++)
                            {
                                if (!classProperty.Contains(tableheader)) break;
                                if (classProperty[j] == reader.GetName(i).ToString())
                                {
                                    var pr = typeClass.GetProperty(classProperty[j]);
                                    pr.SetValue(classObj, reader[i].ToString());
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
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            return reslt;


        }

        //Метод для считывания 1 колонки таблицы
        public List<string> ReadDBColumn(DBProcedure procedure, DBNamesTable tableName, string header)
        {
            SqlConnection con = new SqlConnection(connect);
            List<string> result = new List<string>();
            try
            {
                using (SqlCommand cdm = new SqlCommand(procedure.ToString(), con))
                {
                    cdm.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    //Все заголовки этой таблицы
                    List<string> headerTable = GetTableHeaders(tableName);
                    if (!headerTable.Contains(header))
                    {
                        throw new Exception("ReadDBColumn SQL header is not find");
                    }

                    List<string> parametrs = GetPatametrs(procedure);
                    if (parametrs.Count > 1)
                    {
                        throw new Exception($"ReadDBColumn SQL procedure {procedure.ToString()} has {parametrs.Count} atrebut");
                    }

                    cdm.Parameters.AddWithValue(parametrs[0], tableName.ToString());

                    SqlDataReader reader = cdm.ExecuteReader();


                    while (reader.Read())
                    {
                        result.Add(reader[header].ToString());
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            return result;
        }

        //Вариант изменений колоса





    }
}
