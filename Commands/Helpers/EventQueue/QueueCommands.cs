using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Data;
using System.Text;
using tsom_bot.Commands.Helpers.Discord;
using tsom_bot.Commands.Helpers.promotions;
using tsom_bot.config;
using tsom_bot.Fetcher.database;
using tsom_bot.i18n;
using ZstdSharp.Unsafe;
using static System.Net.WebRequestMethods;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueCommands
    {
        public static async Task sendMessage(Dictionary<string, string> parameters)
        {
            string formattedMessage = parameters.GetValueOrDefault("message");
            string channelIdString = parameters.GetValueOrDefault("channelid");
            var channel = await ClientManager.client.GetChannelAsync(ulong.Parse(channelIdString));

            KeyValuePair<string, List<IMention>> convertedMessageResult = await DiscordMessageHelper.FormatMessage(formattedMessage);

            if (channel != null)
            {
                await new DiscordMessageBuilder()
                .WithContent(convertedMessageResult.Key)
                .WithAllowedMentions(convertedMessageResult.Value)
                .SendAsync(channel);
            }
        }

        public static async Task defenseReminder(Dictionary<string, string> parameters, InteractionContext ctx = null)
        {
            string state = parameters.GetValueOrDefault("state");
            ulong channelid = ulong.Parse(parameters.GetValueOrDefault("channelid"));

            if (state == "NORMAL")
            {
                ulong adminChannelId = 1207774070985199636;
                var channel = await ClientManager.client.GetChannelAsync(adminChannelId);
                if (channel != null)
                {
                    await new DiscordMessageBuilder()
                    .WithContent("@everyone Defense has begon and no version has been set, please set with /queue setdefense (version)")
                    .WithAllowedMention(new EveryoneMention())
                    .SendAsync(channel);
                }

                await QueueHelper.AddFinalTwDefenseToQueue(channelid, DateTime.Now.AddMinutes(15), "Final Defense Ping");
            }
            else if(state == "FINAL")
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();

                DataTable result = await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE eventid = 2");
                DateTime defenseTime = result.Rows[0].Field<DateTime>("sendDate").AddMinutes(-15);
                DateTime timestamp1Time = defenseTime.AddHours(6);
                DateTime timestamp2Time = defenseTime.AddHours(12);
                DateTime fillerTime = defenseTime.AddHours(18);

                string timestamp1Description = "TW Defense Ping 1";
                string timestamp2Description = "TW Defense Ping 2";
                string fillerDescription = "TW Defense Filler Ping";

                string version = parameters.GetValueOrDefault("version") ?? "0";
                if (version == "0")
                {
                    Dictionary<string, string> messageParameters = new Dictionary<string, string>
                {
                    { "message", guildEvents.data.tw.defenseUnder20zones.pingMessage },
                    { "channelid", channelid.ToString() }
                };
                    await sendMessage(messageParameters); // send ping message instant

                    await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20zones.timestamp1, channelid, timestamp1Time, timestamp1Description);
                    await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20zones.timestamp2, channelid, timestamp2Time, timestamp2Description);
                    await QueueHelper.AddMessageToQueue(guildEvents.data.tw.filler, channelid, fillerTime, fillerDescription);
                }
                else if(version == "1") 
                {
                    Dictionary<string, string> messageParameters = new Dictionary<string, string>
                {
                    { "message", guildEvents.data.tw.defenseOver20zones.pingMessage },
                    { "channelid", channelid.ToString() }
                };
                    await sendMessage(messageParameters); // send ping message instant

                    await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseOver20zones.timestamp1, channelid, timestamp1Time, timestamp1Description);
                    await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseOver20zones.timestamp2, channelid, timestamp2Time, timestamp2Description);
                    await QueueHelper.AddMessageToQueue(guildEvents.data.tw.filler, channelid, fillerTime, fillerDescription);
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("**Defense Ping configured!**");
                stringBuilder.AppendLine("- Added TimeStamp 1 ping for " + timestamp1Time.ToString("MM-dd hh:mm"));
                stringBuilder.AppendLine("- Added TimeStamp 2 ping for " + timestamp2Time.ToString("MM-dd hh:mm"));
                stringBuilder.AppendLine("- Added Filler ping for " + fillerTime.ToString("MM-dd hh:mm"));
                
                DiscordChannel channel; 
                if (ctx != null) 
                { 
                    channel = ctx.Channel; 
                } 
                else 
                { 
                    channel = await ClientManager.client.GetChannelAsync(channelid);
                }
                await new DiscordMessageBuilder()
                .WithContent(stringBuilder.ToString())
                .SendAsync(channel);
            }
        }

        public static async Task checkTickets(Dictionary<string, string> parameters)
        {
            string channelIdString = parameters.GetValueOrDefault("channelid");
            string guildId = parameters.GetValueOrDefault("guildid");

            // Add command for next day first for no delay
            await QueueHelper.AddTicketCheckToQueue(ulong.Parse(channelIdString), guildId, DateTime.Now.AddHours(24));

            // rest of the command
            int minimumTicketAmount = await ClientManager.minimumTickets();
            TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ClientManager.client);
            var channel = await ClientManager.client.GetChannelAsync(ulong.Parse(channelIdString));

            await helper.SaveGuildData();

            FileStream file = await helper.GetExcelFile();

            await new DiscordMessageBuilder()
            .WithContent(i18n.i18n.data.commands.tickettracker.get.excel.complete)
            .AddFile(file)
            .SendAsync(channel);

            file.Close();
            System.IO.File.Delete(file.Name);
        }

        public static async Task checkPromotions()
        {
             // Add command for next month first for no delay
            await QueueHelper.AddPromotionCheckToQueue(DateTime.Now.AddMonths(1));

            // rest of the command
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();
            var channelIdSith = reader.channelIds.sith.commands_private;
            var channelIdJedi = reader.channelIds.jedi.commands_private;
            var chanSith = await ClientManager.client.GetChannelAsync(channelIdSith);
            var chanJedi = await ClientManager.client.GetChannelAsync(channelIdJedi);

            if (chanSith != null)
            {
                await TimedPromotionHelper.SyncPromotions(i18n.i18n.data.commands.promotion.sync.complete, null, GuildSwitch.TSOM);
            }

            if (chanJedi != null)
            {
                await TimedPromotionHelper.SyncPromotions(i18n.i18n.data.commands.promotion.sync.complete, null, GuildSwitch.TJOM);
            }
        }
    }
}
