using tsom_bot.Models;

public class IGuild
    {
        public IGuild() {}
        public IMember[]? member { get; set; }
        public IRecentRaidResult[]? recentRaidResult { get; set; }
        public IRecentTerritoryWarResult[]? recentTerritoryWarResult { get; set; }
        public ILastRaidPointsSummary[]? lastRaidPointsSummary { get; set; }
        public IProfile? profile { get; set; }
        public string? nextChallengesRefresh { get; set; }
    }