using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using tsom_bot.Commands.Helpers;

namespace tsom_bot.Commands
{
    public class TicketTrackerCommand : BaseCommandModule
    {
        [Command("tickettrack")]
        public async Task templateCommand(CommandContext ctx)
        {
            string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
            TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId);

            try {
                await ctx.Channel.SendMessageAsync(helper.message);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
