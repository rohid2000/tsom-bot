namespace tsom_bot.Models;

public class IRecentRaidResult
{
    public IRaidMembers[]? RaidMembers { get; set; }
    public string? RaidId { get; set; }
    public string? Duration { get; set; }
    public string? EndTime { get; set; }
    public string? GuildRewardScore { get; set; }
}
