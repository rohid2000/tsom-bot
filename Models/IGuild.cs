using tsom_bot.Models;

public interface IGuild
    {
        IMember[] members { get; }
        IRecentRaidResult[] RaidMembers { get; }
    }
