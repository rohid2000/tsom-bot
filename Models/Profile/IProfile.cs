using tsom_bot.Models.Profile;

namespace tsom_bot.Models;

public class IProfile
{
    public IGuildEventTracker[]? guildEventTracker { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
    public string? externalMessageKey { get; set; }
    public string? logoBackground { get; set; }
    public int? enrollmentStatus { get; set; }
    public int? trophy { get; set; }
    public int? memberCount { get; set; }
    public int? memberMax { get; set; }
    public int? level { get; set; }
    public int? rank { get; set; }
    public int? levelRequirement { get; set; }
    public int? raidWin { get; set; }
    public string? leaderboardScore { get; set; }
    public string? bannerColorId { get; set; }
    public string? bannerLogoId { get; set; }
    public string? guildGalacticPower { get; set; }
    public string? chatChannelId { get; set; }
    public string? guildType { get; set; }
    public string? guildGalacticPowerForRequirement { get; set; }
}
