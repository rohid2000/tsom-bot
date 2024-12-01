using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Data;
using System.Globalization;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.config;
using tsom_bot.Fetcher.database;
using tsom_bot.i18n;

namespace tsom_bot.Commands
{
    public class TimeCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("queue", "Set a time for a command to be executed")]
        public class TimedMessagesContainer : ApplicationCommandModule
        {
            [SlashCommandGroup("event", "Configure Raid messages automatic")]
            public class GuildEventsContainer : ApplicationCommandModule
            {
                [SlashCommand("tw", "add pings for a tw")]
                public async Task addTwPings(InteractionContext ctx, [Option("startTime", "Format: yyyy-MM-dd HH:mm")] string twStartTime)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(twStartTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                        ConfigReader reader = new ConfigReader();
                        await reader.readConfig();

                        await QueueHelper.AddMessageToQueue(guildEvents.data.tw.signup, reader.channelIds.test, dateTime);

                        DateTime defenseTime = dateTime.AddHours(24);
                        await QueueHelper.AddTwDefenseToQueue(reader.channelIds.test, defenseTime);

                        DateTime attackTime = dateTime.AddHours(72);
                        await QueueHelper.AddMessageToQueue(guildEvents.data.tw.attack, reader.channelIds.test, dateTime);

                        DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent($"Messages added to the queue, Startdate is {dateTime.ToString()}, attack Startdate is {attackTime.ToString()}");
                        await ctx.EditResponseAsync(completeMessage);
                    }
                    catch
                    {
                        DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent($"could not convert startTime please use the format: yyyy-MM-dd HH:mm");
                        await ctx.EditResponseAsync(failMessage);
                    }
                }
                [SlashCommand("setDefense", "set the defense phase for a tw")]
                public async Task setdfPings(InteractionContext ctx,
                    [Choice("V1", 0)]
                    [Choice("V2", 1)]
                    [Option("version", "Version of the defense")] long defenseVersion = 0)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                    try
                    {
                        ConfigReader reader = new ConfigReader();
                        await reader.readConfig();

                        DataTable result = await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE eventid = 2");
                        DateTime defenseTime = result.Rows[0].Field<DateTime>("sendDate");
                        DateTime ybannerTime = defenseTime.AddHours(24);
                        DateTime fillerTime = defenseTime.AddHours(48);
                        if (defenseVersion == 0)
                        {
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev1.xbanner, reader.channelIds.test, defenseTime);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev1.ybanner, reader.channelIds.test, ybannerTime);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev1.filler, reader.channelIds.test, fillerTime);
                        }
                        else
                        {
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev2.xbanner, reader.channelIds.test, defenseTime);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev2.ybanner, reader.channelIds.test, defenseTime);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev2.filler, reader.channelIds.test, defenseTime);
                        }

                        DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent($"Added defense pings!");
                        await ctx.EditResponseAsync(completeMessage);
                    }
                    catch
                    {
                        DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent($"could not find tw in queue, Please add a tw event before setting the defense fase!");
                        await ctx.EditResponseAsync(failMessage);
                    }
                }
                [SlashCommand("raid", "Add all pings for a specific raid")]
                public async Task addRaidPings(InteractionContext ctx, [Option("startTime", "Format: yyyy-MM-dd HH:mm")] string raidStartTime)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(raidStartTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                        ConfigReader reader = new ConfigReader();
                        await reader.readConfig();

                        // First ping on start
                        await QueueHelper.AddMessageToQueue(guildEvents.data.raid.live, reader.channelIds.test, dateTime);

                        // Ping 24h before end
                        DateTime endTime = dateTime.AddHours(71 - 24);
                        await QueueHelper.AddMessageToQueue(guildEvents.data.raid.dayLeft, reader.channelIds.test, endTime);

                        DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent($"Messages added to the queue, Startdate is {dateTime.ToString()}");
                        await ctx.EditResponseAsync(completeMessage);
                    }
                    catch
                    {
                        DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent($"could not convert startTime please use the format: yyyy-MM-dd HH:mm");
                        await ctx.EditResponseAsync(failMessage);
                    }
                }
            }
        }
    }
}
