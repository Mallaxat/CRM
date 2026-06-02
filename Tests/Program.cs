using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{

    internal class Program
    {
        static string connect = ConfigurationManager.ConnectionStrings["CRM"].ConnectionString;
        public enum Procedure
        {
            ADD_NEW_USER,

        }
        //СТАТИКИ УБРАТЬ

        public static bool SetPatametr<T>(T user, string procedureParametr,string value)
        {
            procedureParametr =  procedureParametr.Replace("@", "");

            Type type = typeof(T);

            //Получаем свойство класса
            var property= type.GetProperty(procedureParametr);
            if(property != null && property.CanWrite)
            {
                property.SetValue(user, value);
                return true;
            }

            return false;



        }
        public static string GetPatametr<T>(T user, string procedureParametr )
        {
            string option = "get_";
            procedureParametr = $"{option}{procedureParametr.Replace("@", "")}";

            Type type = typeof(T);
            //получить информацию о публичных названиях моего класса
            foreach (MemberInfo member in type.GetMembers())
            {

                if (member.Name.ToString().Contains(procedureParametr))
                    return member.Name.ToString();

            }
            return null;

        }

        public static List<string> GetPatametrs(Procedure procedureName)
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




        static void Main(string[] args)
        {

            //Тест метода возвращения параметров
            #region
            List<string> ProcedureParametrs=GetPatametrs(Procedure.ADD_NEW_USER);
            #endregion

            //Тест метода для передачи параметров из класса в процедуру
            #region
            User user = new User{ FirstName="Имя",
                                   LastName="Фамилия",
                                   MiddleName="Отчество",
                                   Login="Логин",
                                   Password="111"};

           

            Dictionary<string, string> result = new Dictionary<string, string>();
            result=JoinParametrs(user, ProcedureParametrs);

            Console.WriteLine();
            #endregion


        }
    }
}
