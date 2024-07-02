using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.Fetcher.database;
using tsom_bot.Models;
using tsom_bot.Models.Member;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerSaveCommandHelper
    {
        public TicketTrackerSaveCommandHelper(IGuild guildData, int minimalTicketValue) 
        {
            SaveTicketTrackerResultsInDatabase(guildData.GetNoReachedTicketMembers(minimalTicketValue));
        }

        private void SaveTicketTrackerResultsInDatabase(List<IMemberTicketResult> members)
        {;
            string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd");

            foreach(IMemberTicketResult member in members)
            {
                string sql = $"INSERT INTO TicketResults (playerName, ticketAmount, date) VALUES ('{member.playerName}', {member.ticketAmount}, '{sqlFormattedDate}')";
                Database.SendSqlSave(sql);
            }
        }
    }
}
