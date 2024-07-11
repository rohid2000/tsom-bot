using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using tsom_bot.Commands.Helpers;
using tsom_bot.Commands.Helpers.promotions;

namespace tsom_bot.Commands
{
    public class PromotionCommand : BaseCommandModule
    {
        [Command("promotion")]
        public async Task templateCommand(CommandContext ctx, string param = "")
        {
            if (await RoleHelper.hasRole(Role.Acolyte, ctx.Member))
            {
                if (param == "sync")
                {
                    try
                    {
                        await TimedPromotionHelper.SyncPromotions(ctx.Client);
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                    await new DiscordMessageBuilder()
                    .WithContent("sync data with latest")
                    .SendAsync(ctx.Channel);
                }
            }
            else
            {
                try
                {
                    await new DiscordMessageBuilder()
                        .WithContent(RoleHelper.noRoleMessage)
                        .SendAsync(ctx.Channel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
