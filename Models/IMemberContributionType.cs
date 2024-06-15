namespace tsom_bot.Models;

public interface IMemberContributionType
{
    public int Type { get; }
    public string CurrentValue { get; }
    public string LifetimeValue { get; }
}
