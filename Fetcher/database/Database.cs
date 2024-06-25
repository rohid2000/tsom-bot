using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace tsom_bot.Fetcher.database
{
    public static class Database
    {
        private static string connectString = "";
        private static SqlConnection conn;
        public static void Init(string connectionString)
        {
            connectString = connectionString;
        }

        public static DataTable SendSqlPull(string sql) 
        {
            DataTable table = new DataTable();
            using (conn = new SqlConnection(connectString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, conn))
            {
                try
                {
                    adapter.Fill(table);
                    Console.WriteLine("Data Filled");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                } 
            }

            return table;
        }

        public static void SendSqlSave(string sql)
        {
            using (conn = new SqlConnection(connectString))
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                try
                {
                    conn.Open();
                    command.ExecuteScalar();
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
