using System.Data;
using tsom_bot.Commands.Helpers;
using tsom_bot.Fetcher.database;
using tsom_bot.Models;
using tsom_bot.Models.Member;
public class IMember
{
    public string? playerId { get; set; }
    public IMemberContribution[]? memberContribution { get; set; }
    public string? playerName { get; set; }
    public int? playerLevel { get; set; }
    public int? memberLevel { get; set; }
    public int? guildXp { get; set; }
    public string? lastActivityTime { get; set; }
    public int? squadPower { get; set; }
    public string? guildJoinTime { get; set; }
    public string? galacticPower { get; set; }
    public string? playerTitle { get; set; }
    public string? playerPortrait { get; set; }
    public string? leagueId { get; set; }
    public string? shipGalacticPower { get; set; }
    public string? characterGalacticPower { get; set; }
    public string? nucleusId { get; set; }

    public IMemberContribution? GetRaidTicketContribution()
    {
        return this.memberContribution?.Where(i => i.type == 2).ToArray()[0];
    }

    public ContributionReached IsTicketGoalReached(int minimalTicketValue)
    {
        if (int.Parse(GetRaidTicketContribution().currentValue) >= minimalTicketValue)
        {
            return ContributionReached.Yes;
        }
        else
        {
            return ContributionReached.No;
        }
    }

    public bool IsTerritoryWarGoalReached()
    {
        return false;
    }

    public bool IsTerritoryBattleGoalReached()
    {
        return false;
    }

    public bool IsRaidAttemptGoalReached()
    {
        return false;
    }

    public string ConvertContributionReachedToString(ContributionReached contributionReached)
    {
        switch (contributionReached)
        {
            case ContributionReached.No:
                return "Bad!";
            case ContributionReached.Yes:
                return "Good!!";
            case ContributionReached.NVT:
                return "This player doesnt take part in this event yet";
            default:
                return "";
        }
    }

    public async Task cleanupStrikeResults()
    {
        if (!await this.IsAlreadyCleaned())
        {
            DataTable result = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE playerName = '{this.playerName}'");

            if(result.Rows.Count > 0) 
            {
                DataTable resultLifeTime = await Database.SendSqlPull($"SELECT * FROM lifetimetickets WHERE playerName = '{this.playerName}'");

                int ticketamount = 0;
                if (resultLifeTime.Rows.Count > 0)
                {
                    await Database.SendSqlSave($"DELETE FROM lifetime WHERE playerName = '{this.playerName}'");
                    ticketamount += resultLifeTime.Rows[0].Field<int>("ticketamount");
                }
                foreach (DataRow row in result.Rows)
                {
                    ticketamount += new IMemberTicketResult()
                    {
                        playerName = row.Field<string>("playerName"),
                        missingTickets = row.Field<sbyte>("missingTickets") == 1,
                        RaidAttempts = row.Field<sbyte>("RaidAttempts") == 1,
                        TerritoryWar = row.Field<sbyte>("TerritoryWar") == 1,
                        TerritoryBattle = row.Field<sbyte>("TerritoryBattle") == 1,
                        date = row.Field<DateTime>("date"),
                    }.GetTotalStrikes();
                }
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
                await Database.SendSqlSave($"INSERT INTO lifetimetickets (playerName, ticketamount, date) VALUES ('{this.playerName}', {ticketamount}, '{dateTime}')");
                await Database.SendSqlSave($"DELETE FROM ticketresults WHERE playerName = '{this.playerName}'");
            }
        }
    }

    private async Task<bool> IsAlreadyCleaned()
    {
        string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
        DataTable result = await Database.SendSqlPull($"SELECT * FROM lifetimetickets WHERE playerName = '{this.playerName}' AND date = '{dateTime}'");

        return result.Rows.Count > 1;
    }
}

public enum ContributionReached
{
    Yes,
    No,
    NVT
}