using DSharpPlus.Entities;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.Discord
{
    public static class DiscordChannelHelper
    {
        public static async Task<DiscordChannel?> GetChannelByName(string name)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            if(ClientManager.client.Guilds[reader.server_id].Channels.Where((channel) => channel.Value.Name.ToLower() == name.ToLower()).Any()) 
            {
                return ClientManager.client.Guilds[reader.server_id].Channels.Where((channel) => channel.Value.Name.ToLower() == name.ToLower()).First().Value;
            }

            return null;
        }
    }
}
