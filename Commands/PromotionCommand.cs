using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using tsom_bot.Commands.Helpers.promotions;

namespace tsom_bot.Commands
{
    public class PromotionCommand : ApplicationCommandModule
    {
        [SlashCommand("promotion", "Handles the promotions based on join date")]
        public async Task promotionCommand(InteractionContext ctx, [Choice("sync", 0)][Option("param", "parameter")] double param)
        {
            if (param == 0)
            {
                try
                {
                    DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent("synced promotions");
                    await TimedPromotionHelper.SyncPromotions(ctx.Client);
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
