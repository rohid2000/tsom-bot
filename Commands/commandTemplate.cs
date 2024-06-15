using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace tsom_bot.Commands
{
    public class commandTemplate : BaseCommandModule
    {
        [Command("template")]
        public async Task templateCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Template command message");
        }
    }
}
