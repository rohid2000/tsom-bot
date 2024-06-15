namespace tsom_bot.Models;

public class IRecentRaidResult
{
    public IRaidMembers[]? raidMember { get; set; }
    public string? raidId { get; set; }
    public string? duration { get; set; }
    public string? endTime { get; set; }
    public string? guildRewardScore { get; set; }
}
