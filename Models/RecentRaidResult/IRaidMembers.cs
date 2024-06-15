namespace tsom_bot.Models;

public interface IRaidMembers
{
    public string PlayerId { get; }
    public string MemberProgress { get; }
    public int MemberRank { get; }
    public int MemberAttempts { get; }
}
