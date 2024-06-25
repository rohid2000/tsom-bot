using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.Fetcher.database;
using tsom_bot.Models;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerSaveCommandHelper
    {
        private string sql = "SELECT * FROM TicketResults";
        public TicketTrackerSaveCommandHelper(IGuild guildData, int minimalTicketValue) 
        {
            SaveTicketTrackerResultsInDatabase(guildData.GetNoReachedTicketMembers(minimalTicketValue));
        }

        private void SaveTicketTrackerResultsInDatabase(IMember[] members)
        {;
            string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd");
            string sql = $"INSERT INTO TicketResults (playerName, ticketAmount, date) VALUES ('{members[0].playerName}', 1, '{sqlFormattedDate}')";
            Database.SendSqlSave(sql);
        }

        private DataTable PullTicketTrackerResultsFromDatabase()
        {
            DataTable table = Database.SendSqlPull(sql);
            return table;
        }
    }
}
