using tsom_bot.Models;

public interface IGuild
    {
        IMember[] Members { get; }
        IRecentRaidResult[] RaidMembers { get; }
    }
