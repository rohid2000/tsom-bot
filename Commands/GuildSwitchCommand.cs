using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.config;

namespace tsom_bot.Commands
{
    public class GuildSwitchCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("switch", "Switch the BOT between TSOM and TJOM")]
        public class GuildSwitchContainer : ApplicationCommandModule
        {
            [SlashCommand("guild", "Switches between Guilds")]
            public async Task switchCommand(InteractionContext ctx, [Option("guild", "The selected guild")] GuildSwitch guild)
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();
                await QueueHelper.AddMessageToQueue("test Message", reader.channelIds.test, DateTime.Now.AddMinutes(1));

                ClientManager.guildSwitch = guild;

                string messageString = "BOT switched to ";
                if(guild == GuildSwitch.TSOM) 
                {
                    messageString += "TSOM";
                }
                else
                {
                    messageString += "TJOM";
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
