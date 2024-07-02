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

    public List<IMemberTicketResult> GetNoReachedTicketMembers(int minimalTicketValue)
    {
        IMember[] members = this.member.Where(i => i.IsTicketGoalReached(minimalTicketValue) == ContributionReached.No).ToArray();
        List<IMemberTicketResult> memberResults = new List<IMemberTicketResult>();

        for(int i = 0; i < members.Length; i++)
        {
            IMember member = members[i];
            IMemberTicketResult memberResult = new IMemberTicketResult();
            memberResult.playerName = member.playerName;
            memberResult.ticketAmount = 1;
            memberResults.Add(memberResult);
        }

        return memberResults;
    }
}