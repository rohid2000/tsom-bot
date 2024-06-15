using tsom_bot.Models.Profile;

namespace tsom_bot.Models;

public interface IProfile
{
    public IGuildEventTracker[] GuildEventTracker { get; }
    public string Id { get; }
    public string Name { get; }
    public string ExternalMessageKey { get; }
    public string LogoBackground { get; }
    public int EnrollmentStatus { get; }
    public int Trophy { get; }
    public int MemberCount { get; }
    public int MemberMax { get; }
    public int Level { get; }
    public int Rank { get; }
    public int LevelRequirement { get; }
    public int RaidWin { get; }
    public string LeaderboardScore { get; }
    public string BannerColorId { get; }
    public string BannerLogoId { get; }
    public string GuildGalacticPower { get; }
    public string ChatChannelId { get; }
    public string GuildType { get; }
    public string GuildGalacticPowerForRequirement { get; }
}
