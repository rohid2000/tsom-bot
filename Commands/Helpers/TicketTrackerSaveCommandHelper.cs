﻿using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using tsom_bot.Fetcher.database;
using tsom_bot.Models.Member;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerSaveCommandHelper
    {
        public async Task SaveTicketTrackerResultsInDatabase(List<IMemberTicketResult> members)
        {
            string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd");
            await Database.SendSqlSave($"DELETE FROM ticketresults WHERE date = '{sqlFormattedDate}'");

            foreach(IMemberTicketResult member in members)
            {
                byte missingTickets = (byte) (member.missingTickets ? 1 : 0);
                byte TerritoryBattle = (byte) (member.TerritoryBattle ? 1 : 0);
                byte TerritoryWar = (byte) (member.TerritoryWar ? 1 : 0);
                byte RaidAttempts = (byte) (member.RaidAttempts ? 1 : 0);

                string sql = $"INSERT INTO ticketresults (playerName, missingTickets, TerritoryBattle, TerritoryWar, RaidAttempts, date) VALUES ('{member.playerName}', {missingTickets}, {TerritoryBattle}, {TerritoryWar}, {RaidAttempts}, '{sqlFormattedDate}')";
                await this.ExcludeMemberInDatabase(member.playerName);
                await Database.SendSqlSave(sql);
            }
        }

        public async Task ExcludeMemberInDatabase(string playerName)
        {
            DateTime now = DateTime.Now;
            DataTable memberResultDataThisMonth = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE date BETWEEN '{new DateTime(now.Year, now.Month, 1).ToString("yyyy-MM-dd")}' AND '{new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1).ToString("yyyy-MM-dd")}' AND playerName = '{playerName}'");
            if (memberResultDataThisMonth.Rows.Count > 0)
            {
                int ticketAmount = 0;
                for (int j = 0; j < memberResultDataThisMonth.Rows.Count; j++)
                {
                    ticketAmount += new IMemberTicketResult()
                    {
                        RaidAttempts = memberResultDataThisMonth.Rows[j].Field<sbyte>("RaidAttempts") == 1,
                        TerritoryBattle = memberResultDataThisMonth.Rows[j].Field<sbyte>("TerritoryBattle") == 1,
                        TerritoryWar = memberResultDataThisMonth.Rows[j].Field<sbyte>("TerritoryWar") == 1,
                        missingTickets = memberResultDataThisMonth.Rows[j].Field<sbyte>("missingTickets") == 1,
                    }.GetTotalStrikes();
                }
                ticketAmount = ticketAmount % 3;
                if (ticketAmount == 0)
                {
                    await Database.SendSqlSave($"INSERT INTO excludefromtickets (playerName, date) VALUES ('{playerName}', '{DateTime.Now.AddDays(2).ToString("yyyy-MM-dd")}')");
                }
            }
        }

        public async Task<bool> IsSyncedToday()
        {
            string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd");
            DataTable result = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE date = '{sqlFormattedDate}'");
            return result.Rows.Count > 0;
        }
    }
}
