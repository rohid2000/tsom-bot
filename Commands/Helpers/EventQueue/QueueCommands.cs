using DSharpPlus.Entities;
using tsom_bot.Commands.Helpers.promotions;
using tsom_bot.config;
using tsom_bot.i18n;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueCommands
    {
        public static async Task sendMessage(Dictionary<string, string> parameters)
        {
            string formattedMessage = parameters.GetValueOrDefault("message");
            string channelIdString = parameters.GetValueOrDefault("channelid");
            var channel = await ClientManager.client.GetChannelAsync(ulong.Parse(channelIdString));

            string convertedMessage = formattedMessage.Replace("||@@", ",");
            if (channel != null)
            {
                await new DiscordMessageBuilder()
                .WithContent(convertedMessage)
                .SendAsync(channel);
            }
        }

        public static async Task defenseFallback(Dictionary<string, string> parameters)
        {
            string channelIdString = parameters.GetValueOrDefault("channelid");
            var channel = await ClientManager.client.GetChannelAsync(ulong.Parse(channelIdString));

            if (channel != null)
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();

                DateTime defenseTime = DateTime.Now;
                DateTime ybannerTime = defenseTime.AddHours(24);
                DateTime fillerTime = defenseTime.AddHours(48);

                await new DiscordMessageBuilder()
                    .WithContent(guildEvents.data.tw.defenseUnder20banners.timestamp1)
                    .SendAsync(channel);
                await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20banners.timestamp2, reader.channelIds.test, ybannerTime);
                await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defenseUnder20banners.filler, reader.channelIds.test, fillerTime);
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
            File.Delete(file.Name);
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
