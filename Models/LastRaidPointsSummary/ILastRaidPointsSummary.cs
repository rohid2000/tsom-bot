namespace tsom_bot.Models;

public interface ILastRaidPointsSummary
{
    public ILastRaidPointsSummaryIndentifier Identifiers { get; }
    public string TotalPoints { get; }
}
