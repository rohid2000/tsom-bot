using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace tsom_bot.Commands
{
    public class GuildSwitchCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("switch", "Switch the bot with jedi and sith guild")]
        public class GuildSwitchContainer : ApplicationCommandModule
        {
            [SlashCommand("set", "switches the bot")]
            public async Task switchCommand(InteractionContext ctx, [Option("guild", "the selected guild")] GuildSwitch guild)
            {
                ClientManager.guildSwitch = guild;

                string messageString = "bot switched to ";
                if(guild == GuildSwitch.Sith) 
                {
                    messageString += "sith guild";
                }
                else
                {
                    messageString += "jedi guild";
                }

                try
                {
                    DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent(messageString);
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
