using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.config;
using tsom_bot.Fetcher.database;
using tsom_bot.i18n;

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
                    DataTable result = await QueueHelper.GetQueueItemFromToTime(DateTime.Now, toTime);

                    StringBuilder message = new StringBuilder();
                    message.AppendLine($"Total items: {result.Rows.Count}, in the queue");


                    for(int i = 0; i < result.Rows.Count; i++)
                    {
                        DataRow row = result.Rows[i];
                        message.AppendLine(QueueHelper.QueueItemToString(row));
                    }

                    DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(message.ToString());
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

                        DateTime attackTime = defenseTime.AddHours(23);
                        await QueueHelper.AddMessageToQueue(guildEvents.data.tw.attack, reader.channelIds.test, attackTime);


                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("**TW Pings configured**");
                        stringBuilder.AppendLine("- Signup Ping for " + "*" +  dateTime.ToString("MM-dd hh:mm") + "*");
                        stringBuilder.AppendLine("- Defense Ping for " + "*" + defenseTime.ToString("MM-dd hh:mm") + "*");
                        stringBuilder.AppendLine("- Attack Ping for " + "*" + attackTime.ToString("MM-dd hh:mm") + "*");

                        DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(stringBuilder.ToString());
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
                        if (result.Rows.Count > 0) 
                        {
                            string version = defenseVersion == 0 ? "defenseUnder20banners" : "defenseOver20banners";

                            Dictionary<string, string> parameters = new Dictionary<string, string>()
                            {
                                { "state", "FINAL" },
                                { "channelid", reader.channelIds.test.ToString() },
                                { "version", defenseVersion.ToString() }
                            };
                            await QueueCommands.defenseReminder(parameters, ctx);
                            await QueueHelper.RemoveQueuedItem(result.Rows[0]);

                            DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent($"Defense version set to **{version}** ping message send!");
                            await ctx.EditResponseAsync(completeMessage);
                        }
                        else
                        {
                            DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent($"**could not find tw in queue!**, Please add a tw event before setting the defense fase!");
                            await ctx.EditResponseAsync(failMessage);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent($"**ERRROR** please contact developer");
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

                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine($"*({raidType})* **Raid Pings configured**");
                        stringBuilder.AppendLine("- Live Ping for " + "*" + dateTime.ToString("MM-dd hh:mm") + "*");
                        stringBuilder.AppendLine("- 1 Day Left Ping for " + "*" + endTime.ToString("MM-dd hh:mm") + "*");

                        DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(stringBuilder.ToString());
                        await ctx.EditResponseAsync(completeMessage);
                    }
                    catch
                    {
                        DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent($"could not convert startTime please use the format: yyyy-MM-dd HH:mm");
                        await ctx.EditResponseAsync(failMessage);
                    }
                }

                [SlashCommand("tb", "Add all pings for a specific territory battle")]
                public async Task addTbPings(InteractionContext ctx, [Option("startTime", "Format: yyyy-MM-dd HH:mm")] string raidStartTime, [Option("raidType", "which raid is starting")] TBType tbType, [Option("guild", "The selected guild")] GuildSwitch guild)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                    ConfigReader reader = new ConfigReader();
                    await reader.readConfig();

                    try
                    {
                        DateTime phase1SendTime = DateTime.ParseExact(raidStartTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        DateTime phase2SendTime = phase1SendTime.AddHours(6);
                        DateTime phase3SendTime = phase2SendTime.AddHours(6);
                        DateTime phase4SendTime = phase3SendTime.AddHours(6);

                        string phase1PingMessage = guildEvents.data.tb.GetRandomHeader(1, tbType) + "\n\n";
                        string phase2PingMessage = guildEvents.data.tb.GetRandomHeader(2, tbType) + "\n\n";
                        string phase3PingMessage = guildEvents.data.tb.GetRandomHeader(3, tbType) + "\n\n";
                        string phase4PingMessage = guildEvents.data.tb.GetRandomHeader(4, tbType) + "\n\n";

                        if (tbType == TBType.RepublicOffense)
                        {
                            phase1PingMessage += guildEvents.data.tb.republicOffense.phase1pingMessage;
                            phase2PingMessage += guildEvents.data.tb.republicOffense.phase2pingMessage;
                            phase3PingMessage += guildEvents.data.tb.republicOffense.phase3pingMessage;
                            phase4PingMessage += guildEvents.data.tb.republicOffense.phase4pingMessage;

                            if (guildEvents.data.tb.republicOffense.specificFooter != null)
                            {
                                phase1PingMessage += "\n\n" + guildEvents.data.tb.republicOffense.specificFooter;
                                phase2PingMessage += "\n\n" + guildEvents.data.tb.republicOffense.specificFooter;
                                phase3PingMessage += "\n\n" + guildEvents.data.tb.republicOffense.specificFooter;
                                phase4PingMessage += "\n\n" + guildEvents.data.tb.GetRandomPhase4Footer(guild);
                            }
                            else
                            {
                                phase1PingMessage += "\n\n" + guildEvents.data.tb.GetRandomFooter(guild);
                                phase2PingMessage += "\n\n" + guildEvents.data.tb.GetRandomFooter(guild);
                                phase3PingMessage += "\n\n" + guildEvents.data.tb.GetRandomFooter(guild);
                                phase4PingMessage += "\n\n" + guildEvents.data.tb.GetRandomPhase4Footer(guild);
                            }

                            await QueueHelper.AddMessageToQueue(phase1PingMessage, reader.channelIds.test, phase1SendTime, "TB Phase 1 Ping (republicOffense)");
                            await QueueHelper.AddMessageToQueue(phase2PingMessage, reader.channelIds.test, phase2SendTime, "TB Phase 2 Ping (republicOffense)");
                            await QueueHelper.AddMessageToQueue(phase3PingMessage, reader.channelIds.test, phase3SendTime, "TB Phase 3 Ping (republicOffense)");
                            await QueueHelper.AddMessageToQueue(phase4PingMessage, reader.channelIds.test, phase4SendTime, "TB Phase 4 Ping (republicOffense)");
                        }
                        else if (tbType == TBType.SeparatistMight)
                        {
                            phase1PingMessage += guildEvents.data.tb.separatistMight.phase1pingMessage;
                            phase2PingMessage += guildEvents.data.tb.separatistMight.phase2pingMessage;
                            phase3PingMessage += guildEvents.data.tb.separatistMight.phase3pingMessage;
                            phase4PingMessage += guildEvents.data.tb.separatistMight.phase4pingMessage;

                            if (guildEvents.data.tb.separatistMight.specificFooter != null)
                            {
                                phase1PingMessage += "\n\n" + guildEvents.data.tb.separatistMight.specificFooter;
                                phase2PingMessage += "\n\n" + guildEvents.data.tb.separatistMight.specificFooter;
                                phase3PingMessage += "\n\n" + guildEvents.data.tb.separatistMight.specificFooter;
                                phase4PingMessage += "\n\n" + guildEvents.data.tb.GetRandomPhase4Footer(guild);
                            }
                            else
                            {
                                phase1PingMessage += "\n\n" + guildEvents.data.tb.GetRandomFooter(guild);
                                phase2PingMessage += "\n\n" + guildEvents.data.tb.GetRandomFooter(guild);
                                phase3PingMessage += "\n\n" + guildEvents.data.tb.GetRandomFooter(guild);
                                phase4PingMessage += "\n\n" + guildEvents.data.tb.GetRandomPhase4Footer(guild);
                            }

                            await QueueHelper.AddMessageToQueue(phase1PingMessage, reader.channelIds.test, phase1SendTime, "TB Phase 1 Ping (separatistMight)");
                            await QueueHelper.AddMessageToQueue(phase2PingMessage, reader.channelIds.test, phase2SendTime, "TB Phase 2 Ping (separatistMight)");
                            await QueueHelper.AddMessageToQueue(phase3PingMessage, reader.channelIds.test, phase3SendTime, "TB Phase 3 Ping (separatistMight)");
                            await QueueHelper.AddMessageToQueue(phase4PingMessage, reader.channelIds.test, phase4SendTime, "TB Phase 4 Ping (separatistMight)");
                        }

                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine($"*({tbType})* **TB Pings configured**");
                        stringBuilder.AppendLine("- Phase 1 Ping for " + "*" + phase1SendTime.ToString("MM-dd hh:mm") + "*");
                        stringBuilder.AppendLine("- Phase 2 Ping for " + "*" + phase2SendTime.ToString("MM-dd hh:mm") + "*");
                        stringBuilder.AppendLine("- Phase 3 Ping for " + "*" + phase3SendTime.ToString("MM-dd hh:mm") + "*");
                        stringBuilder.AppendLine("- Phase 4 Ping for " + "*" + phase4SendTime.ToString("MM-dd hh:mm") + "*");

                        DiscordWebhookBuilder successMessage = new DiscordWebhookBuilder().WithContent(stringBuilder.ToString());
                        await ctx.EditResponseAsync(successMessage);
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
