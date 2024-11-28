using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using tsom_bot.Commands.Helpers.Discord;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.Commands.Helpers.promotions;
using tsom_bot.config;

namespace tsom_bot.Commands
{
    public class QueueCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("queue", "Set a time for a command to be executed")]
        public class QueueContainer : ApplicationCommandModule
        {
            [SlashCommandGroup("raid", "Configure Raid messages automatic")]
            public class RaidContainer : ApplicationCommandModule
            {
                [SlashCommand("add", "Add all pings for a specific raid")]
                public async Task addRaidPings(InteractionContext ctx, [Option("start time", "the time when the raid is active")] TimeSpan raidStartTime)
                {
                    ConfigReader reader = new ConfigReader();
                    await reader.readConfig();
                    // First ping on start
                    await QueueHelper.AddMessageToQueue(i18n.i18n.data.raid.livePing, reader.channelIds.test, raidStartTime);


                    DateTime endTime = raidStartTime.AddHours(71);
                    // Ping 24h before end
                    await QueueHelper.AddMessageToQueue(i18n.i18n.data.raid.dayLeft, reader.channelIds.test, endTime.AddHours(-1));
                }
            }
        }
    }
}
