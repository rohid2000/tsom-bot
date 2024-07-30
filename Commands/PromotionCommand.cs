using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using tsom_bot.Commands.Helpers.promotions;

namespace tsom_bot.Commands
{
    public class PromotionCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("promotion", "Handles the promotions based on join date")]
        public class PromotionContainer : ApplicationCommandModule
        {
            [SlashCommand("sync", "synces the ranks of all players in guild")]
            public async Task promotionCommand(InteractionContext ctx)
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

            [SlashCommand("override", "override a players rank for this command")]
            public async Task promotionOverrrideCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("role", "the role that the player always has")] Role role)
            {
                try
                {
                    DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent($"excluded {dcMember.Mention} from promotion sync");
                    await TimedPromotionHelper.ExludePlayerFromPromotion(dcMember, role);
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
