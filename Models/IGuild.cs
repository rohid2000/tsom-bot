using tsom_bot.Models;

public class IGuild
    {
        public IGuild() {}
        public IMember[]? Members { get; set; }
        public IRecentRaidResult[]? RaidMembers { get; set; }
        public IRecentTerritoryWarResult[]? RecentTerritoryWarResult { get; set; }
        public ILastRaidPointsSummary[]? LastRaidPointsSummaries { get; set; }
        public IProfile? Profile { get; set; }
        public string? nextChallengesRefresh { get; set; }
    }