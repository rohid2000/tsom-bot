using tsom_bot.Fetcher.database;
using tsom_bot.Models.Member;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerSaveCommandHelper
    {
        public TicketTrackerSaveCommandHelper(IGuild guildData, int minimalTicketValue) 
        {
            SaveTicketTrackerResultsInDatabase(guildData.GetTicketResults(minimalTicketValue));
        }

        private void SaveTicketTrackerResultsInDatabase(List<IMemberTicketResult> members)
        {;
            string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd");

            foreach(IMemberTicketResult member in members)
            {
                byte missingTickets = (byte) (member.missingTickets ? 1 : 0);
                byte TerritoryBattle = (byte) (member.TerritoryBattle ? 1 : 0);
                byte TerritoryWar = (byte) (member.TerritoryWar ? 1 : 0);
                byte RaidAttempts = (byte) (member.RaidAttempts ? 1 : 0);

                string sql = $"INSERT INTO TicketResults (playerName, missingTickets, TerritoryBattle, TerritoryWar, RaidAttempts, date) VALUES ('{member.playerName}', {missingTickets}, {TerritoryBattle}, {TerritoryWar}, {RaidAttempts}, '{sqlFormattedDate}')";
                Console.WriteLine(sql);
                Database.SendSqlSave(sql);
            }
        }
    }
}
