using tsom_bot.Models;
public class IMember
{
    public string? playerId { get; set; }
    public IMemberContribution[]? memberContribution { get; set; }
    public string? playerName { get; set; }
    public int? playerLevel { get; set; }
    public int? memberLevel { get; set; }
    public int? guildXp { get; set; }
    public string? lastActivityTime { get; set; }
    public int? squadPower { get; set; }
    public string? guildJoinTime { get; set; }
    public string? galacticPower { get; set; }
    public string? playerTitle { get; set; }
    public string? playerPortrait { get; set; }
    public string? leagueId { get; set; }
    public string? shipGalacticPower { get; set; }
    public string? characterGalacticPower { get; set; }
    public string? nucleusId { get; set; }

    public IMemberContribution? GetRaidTicketContribution()
    {
        return this.memberContribution?[2];
    }
}