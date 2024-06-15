namespace tsom_bot.Models.Profile;

public interface IGuildEventTracker
{
    public string DefinitionId { get; }
    public string CompletedStars { get; }
    public string EndTime { get; }
}
