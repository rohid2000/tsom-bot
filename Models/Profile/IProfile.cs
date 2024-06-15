using tsom_bot.Models.Profile;

namespace tsom_bot.Models;

public class IProfile
{
    public IGuildEventTracker[]? GuildEventTracker { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ExternalMessageKey { get; set; }
    public string? LogoBackground { get; set; }
    public int? EnrollmentStatus { get; set; }
    public int? Trophy { get; set; }
    public int? MemberCount { get; set; }
    public int? MemberMax { get; set; }
    public int? Level { get; set; }
    public int? Rank { get; set; }
    public int? LevelRequirement { get; set; }
    public int? RaidWin { get; set; }
    public string? LeaderboardScore { get; set; }
    public string? BannerColorId { get; set; }
    public string? BannerLogoId { get; set; }
    public string? GuildGalacticPower { get; set; }
    public string? ChatChannelId { get; set; }
    public string? GuildType { get; set; }
    public string? GuildGalacticPowerForRequirement { get; set; }
}
