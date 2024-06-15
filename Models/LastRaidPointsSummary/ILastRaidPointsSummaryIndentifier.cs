namespace tsom_bot.Models;

public interface ILastRaidPointsSummaryIndentifier
{
    public string CampaignId { get; }
    public string CampaignMapId { get; }
    public string CampaignNodeId { get; }
    public string CampaignNodeDifficulty { get; }
    public string CampaignMissionId { get; }
}
