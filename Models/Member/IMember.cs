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

    public IMemberTicketResult GetPreviousTicketResult()
    {
        string sql = $"SELECT * FROM TicketResults WHERE playerName = '{this.playerName}'";
        DataTable table = Database.SendSqlPull(sql);

        IMemberTicketResult memberResult = new IMemberTicketResult();
        memberResult.playerName = this.playerName;
        memberResult.ticketAmount = table.Rows[0].Field<int>("ticketAmount");
        return memberResult;
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
}

public enum ContributionReached
{
    Yes,
    No,
    NVT
}