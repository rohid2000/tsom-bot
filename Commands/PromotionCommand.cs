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
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.promotion.sync.loading);
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.promotion.sync.fail);

                await ctx.EditResponseAsync(loadingMessage);

                try
                {
                    await TimedPromotionHelper.SyncPromotions(ctx.Client, ctx);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(failMessage);
                    Console.WriteLine(ex.Message);
                }
            }

            [SlashCommand("override", "override a players rank for this command")]
            public async Task promotionOverrrideCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("role", "the role that the player always has")] Role role)
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.Transform(i18n.i18n.data.commands.promotion.override_M.loading, dcMember));
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.Transform(i18n.i18n.data.commands.promotion.override_M.fail, dcMember));
                DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.Transform(i18n.i18n.data.commands.promotion.override_M.complete, dcMember));

                await ctx.EditResponseAsync(loadingMessage);

                try
                {
                    await TimedPromotionHelper.ExludePlayerFromPromotion(dcMember, role);
                    await ctx.EditResponseAsync(completeMessage);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(failMessage);
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
