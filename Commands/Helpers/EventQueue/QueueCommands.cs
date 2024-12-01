using DSharpPlus.Entities;
using tsom_bot.config;
using tsom_bot.i18n;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueCommands
    {
        public static async Task sendMessage(Dictionary<string, string> parameters)
        {
            string messageString = parameters.GetValueOrDefault("message");
            string channelIdString = parameters.GetValueOrDefault("channelid");
            var channel = await ClientManager.client.GetChannelAsync(ulong.Parse(channelIdString));

            if(channel != null)
            {
                await new DiscordMessageBuilder()
                .WithContent(messageString)
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
                    .WithContent(guildEvents.data.tw.defensev1.xbanner)
                    .SendAsync(channel);
                await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev1.ybanner, reader.channelIds.test, ybannerTime);
                await QueueHelper.AddMessageToQueue(guildEvents.data.tw.defensev1.filler, reader.channelIds.test, fillerTime);
            }
        }
    }
}
