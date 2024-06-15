using tsom_bot.Models;
public interface IMember
{
    public string PlayerId { get; }
    public IMemberContribution[] MemberContribution { get; }
    public string PlayerName { get; }
    public int PlayerLevel { get; }
    public int MemberLevel { get; }
    public int GuildXp { get; }
    public string LastActivityTime { get; }
    public int SquadPower { get; }
    public string GuildJoinTime { get; }
    public string GalacticPower { get; }
    public string PlayerTitle { get; }
    public string PlayerPortrait { get; }
    public string LeagueId { get; }
    public string ShipGalacticPower { get; }
    public string CharacterGalacticPower { get; }
    public string NucleusId { get; }
}