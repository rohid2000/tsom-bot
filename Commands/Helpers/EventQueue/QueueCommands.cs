using DSharpPlus;
using DSharpPlus.Entities;
using MySqlX.XDevAPI;
using tsom_bot.config;

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
    }
}
