namespace tsom_bot.Models;

public interface IRecentRaidResult
{
    public IRaidMembers[] RaidMembers { get; }
    public string RaidId { get; }
    public string Duration { get; }
    public string EndTime { get; }
    public string GuildRewardScore { get; }
}
