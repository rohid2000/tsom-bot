namespace tsom_bot.Models;

public interface IMemberContribution
{
    public int[] Type { get; }
    public string CurrentTicketValue { get; }
}
