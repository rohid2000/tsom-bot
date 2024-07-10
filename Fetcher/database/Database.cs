using System.Data.SqlClient;
using System.Data;
using MySql.Data.MySqlClient;
using System.Runtime.CompilerServices;
using System.Data.Common;

namespace tsom_bot.Fetcher.database
{
    public static class Database
    {
        private static string connectString = "";
        public async static Task Init(string connectionString)
        {
            connectString = connectionString;
        }

        public async static Task<DataTable> SendSqlPull(string sql) 
        {
            DataSet ds = new();
            DataTable dt = new();
            string tableName = "ticketresults";
            MySqlConnection? conn = null;
            try
            {
                using (conn = new MySqlConnection(connectString))
                {
                    await conn.OpenAsync();
                    using (var command = new MySqlCommand(sql, conn))
                    {
                        using (var adapter = new MySqlDataAdapter(command))
                        {    
                            int results = await adapter.FillAsync(ds, tableName);
                            dt = ds.Tables[tableName];
                            Console.WriteLine(results);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await conn?.CloseAsync();

            return dt;
        }

        public async static Task SendSqlSave(string sql)
        {
            DataTable table = new DataTable();
            MySqlConnection? conn = null;
            try
            {
                using (conn = new MySqlConnection(connectString))
                {
                    await conn.OpenAsync();
                    using (var command = new MySqlCommand(sql, conn))
                    {
                        var result = await command.ExecuteScalarAsync();
                        Console.WriteLine("Command send success!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
            await conn?.CloseAsync();
        }
    }
}
