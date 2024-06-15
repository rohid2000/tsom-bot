using tsom_bot.Models;

public interface IGuild
    {
        IMember[] Members { get; }
        IRecentRaidResult[] RaidMembers { get; }
        IRecentTerritoryWarResult[] RecentTerritoryWarResult { get; }
        ILastRaidPointsSummary[] LastRaidPointsSummaries { get; }
        public IProfile Profile { get; }
        public string NextChallengesRefresh { get; }
    }