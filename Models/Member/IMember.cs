using tsom_bot.Models;
public class IMember
{
    public string? PlayerId { get; set; }
    public IMemberContribution[]? MemberContribution { get; set; }
    public string? PlayerName { get; set; }
    public int? PlayerLevel { get; set; }
    public int? MemberLevel { get; set; }
    public int? GuildXp { get; set; }
    public string? LastActivityTime { get; set; }
    public int? SquadPower { get; set; }
    public string? GuildJoinTime { get; set; }
    public string? GalacticPower { get; set; }
    public string? PlayerTitle { get; set; }
    public string? PlayerPortrait { get; set; }
    public string? LeagueId { get; set; }
    public string? ShipGalacticPower { get; set; }
    public string? CharacterGalacticPower { get; set; }
    public string? NucleusId { get; set; }
}