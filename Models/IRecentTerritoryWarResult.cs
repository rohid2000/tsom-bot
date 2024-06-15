namespace tsom_bot.Models;

public interface IRecentTerritoryWarResult
{
    public string TerritoryWarId { get; }
    public string Score { get; }
    public string Power { get; }
    public string OpponentScore { get; }
    public string EndTimeSeconds { get; }
}
