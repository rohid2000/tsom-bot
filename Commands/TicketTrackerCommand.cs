using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using tsom_bot.Commands.Helpers;

namespace tsom_bot.Commands
{
    public class TicketTrackerCommand : BaseCommandModule
    {
        [Command("tickets")]
        public async Task templateCommand(CommandContext ctx, string param = "")
        {
            if(await RoleHelper.hasRole(Role.Acolyte, ctx.Member))
            {
                string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId);

                if (param == "excel")
                {
                    try
                    {
                        await new DiscordMessageBuilder()
                            .WithContent("this is your file")
                            .AddFile(helper.GetExcelFile())
                            .SendAsync(ctx.Channel);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        await new DiscordMessageBuilder()
                            .WithContent(helper.message)
                            .SendAsync(ctx.Channel);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
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
