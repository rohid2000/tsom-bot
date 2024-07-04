using tsom_bot.Models;
using tsom_bot.Models.Member;

public class IGuild
{
    public IGuild() {}
    public IMember[]? member { get; set; }
    public IRecentRaidResult[]? recentRaidResult { get; set; }
    public IRecentTerritoryWarResult[]? recentTerritoryWarResult { get; set; }
    public ILastRaidPointsSummary[]? lastRaidPointsSummary { get; set; }
    public IProfile? profile { get; set; }
    public string? nextChallengesRefresh { get; set; }

    public List<IMemberTicketResult> GetTicketResults(int minimalTicketValue)
    {
        List<IMemberTicketResult> memberResults = new List<IMemberTicketResult>();
        foreach (IMember singleMember in member)
        {
            if(
                singleMember.IsTicketGoalReached(minimalTicketValue) == ContributionReached.No ||
                singleMember.IsTerritoryBattleGoalReached() ||
                singleMember.IsTerritoryWarGoalReached() ||
                singleMember.IsRaidAttemptGoalReached()
            )
            {
                memberResults.Add(new()
                {
                    missingTickets = singleMember.IsTicketGoalReached(minimalTicketValue) == ContributionReached.No,
                    RaidAttempts = singleMember.IsRaidAttemptGoalReached(),
                    TerritoryBattle = singleMember.IsTerritoryBattleGoalReached(),
                    TerritoryWar = singleMember.IsTerritoryWarGoalReached(),
                    playerName = singleMember.playerName
                });
            }
        }
        return memberResults;
    }
}