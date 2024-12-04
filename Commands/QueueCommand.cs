using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.config;
using tsom_bot.Fetcher.database;
using tsom_bot.i18n;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace tsom_bot.Commands
{
    public class TimeCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("queue", "Set a time for a command to be executed")]
        public class QueueContainer : ApplicationCommandModule
        {
            [SlashCommandGroup("list", "Configure Raid messages automatic")]
            public class QueueListContainer : ApplicationCommandModule
            {
                [SlashCommand("show", "shows a list of all items in queue")]
                public async Task showQueueItems(InteractionContext ctx, 
                    [Choice("1 week", 0)]
                    [Choice("1 month", 1)]
                    [Option("time", "the max time to search to")] long time = 0)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                    DateTime toTime = DateTime.Now;
                    if(time == 0)
                    {
                        toTime = toTime.AddDays(7);
                    }
                    else
                    {
                        toTime = toTime.AddMonths(1);
                    }
                    DataTable result = await QueueHelper.GetQueueItemToTime(toTime);
                    string message = $"Total off {result.Rows.Count} items in queue until {toTime.ToString("yyyy-MM-dd")}\n";

                    for(int i = 0; i < result.Rows.Count; i++)
                    {
                        DataRow row = result.Rows[i];
                        message += QueueHelper.QueueItemToString(row) + "\n";
                    }

                    DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(message);
                    await ctx.EditResponseAsync(completeMessage);
                }
            }
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

                [SlashCommand("setdefense", "set the defense phase for a tw")]
                public async Task setdfPings(InteractionContext ctx,
                    [Choice("defenseUnder20banners", 0)]
                    [Choice("defenseOver20banners", 1)]
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

                        string timestamp1Description = "TW Defense Ping 1";
                        string timestamp2Description = "TW Defense Ping 2";
                        string fillerDescription = "TW Defense Filler Ping";

                        if (defenseVersion == 0)
                        {
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20zones.timestamp1, reader.channelIds.test, defenseTime, timestamp1Description);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20zones.timestamp2, reader.channelIds.test, ybannerTime, timestamp2Description);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20zones.filler, reader.channelIds.test, fillerTime, fillerDescription);
                        }
                        else
                        {
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseOver20zones.timestamp1, reader.channelIds.test, defenseTime, timestamp1Description);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseOver20zones.timestamp2, reader.channelIds.test, defenseTime, timestamp2Description);
                            await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseOver20zones.filler, reader.channelIds.test, defenseTime, fillerDescription);
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
                public async Task addRaidPings(InteractionContext ctx, [Option("startTime", "Format: yyyy-MM-dd HH:mm")] string raidStartTime, [Option("raidType", "which raid is starting")] RaidType raidType)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(raidStartTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                        ConfigReader reader = new ConfigReader();
                        await reader.readConfig();

                        // First ping on start
                        string liveDescription = $"{raidType} Raid - Live ping";
                        await QueueHelper.AddMessageToQueue(guildEvents.data.raid.live, reader.channelIds.test, dateTime, liveDescription);

                        // Ping 24h before end
                        DateTime endTime = dateTime.AddHours(71 - 24);
                        string dayLeftDescription = $"{raidType} Raid - Day left ping";
                        await QueueHelper.AddMessageToQueue(guildEvents.data.raid.dayLeft, reader.channelIds.test, endTime, dayLeftDescription);

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
