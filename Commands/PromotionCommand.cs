using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using tsom_bot.Commands.Helpers.Discord;
using tsom_bot.Commands.Helpers.promotions;

namespace tsom_bot.Commands
{
    public class PromotionCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("promotion", "Handles the promotions based on join date")]
        public class PromotionContainer : ApplicationCommandModule
        {
            [SlashCommand("sync", "Synces the Roles of all players in guild")]
            public async Task promotionCommand(InteractionContext ctx)
            {
                await DiscordMessageHelper.BuildPromotionMessageWithExecute(ctx, i18n.i18n.data.commands.promotion.sync, (completeMessage) => TimedPromotionHelper.SyncPromotions(completeMessage, ctx));
            }

            [SlashCommand("override", "Override a players Role for this command")]
            public async Task promotionOverrrideCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("role", "The role that the player always has")] Role role)
            {
                await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.promotion.override_M, () => TimedPromotionHelper.ExludePlayerFromPromotion(dcMember, role));
            }
        }
    }
}
