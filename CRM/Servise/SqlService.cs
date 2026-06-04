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
using System.Runtime.InteropServices.ComTypes;
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

        private static string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;
        
        //Метод для полечения параметра процедуры
        private static List<string> GetPatametrs(DBProcedure procedureName)
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
        private List<string> GetTableHeaders(DBNamesTable tableName)
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

        public static class SQLClient
        {
            public static ObservableCollection<Client> Read(DBProcedure procedure, string login)
            {

                SqlConnection con = new SqlConnection(connect);


                ObservableCollection<Client> result = new ObservableCollection<Client>();

                using (SqlCommand cmd = new SqlCommand(procedure.ToString(), con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    Type _class = typeof(Client);
                    List<string> PrParam = GetPatametrs(procedure);
                    var ClassProperty = _class.GetProperties();


                    cmd.Parameters.AddWithValue(PrParam[0].ToString(),login);
                    SqlDataReader reader= cmd.ExecuteReader();
                    Client buf= new Client();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string tableheader = reader.GetName(i).ToString();

                            for (int j = 0; j < ClassProperty.Length; j++)
                            {
                                if(tableheader==ClassProperty[j].ToString())
                                {
                                    var property = _class.GetProperty(reader[i].ToString());
                                    property.SetValue(buf, reader[i].ToString());
                                }
     
                            }

                        }

                        result.Add(buf);
                        buf = new Client();
                    }

                }
                return result;
            }

        }

        //Метод для чтения одной таблицы типа клиент

        //Метод для обновления данных одной таблицы типа клиент
        //Метод для удаления одного значения из данных одной таблицы типа клиент
        //Метод для добавления нового объекта типа клиент




    }
}
