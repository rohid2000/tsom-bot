using DSharpPlus.Entities;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.Discord
{
    public static class DiscordRoleHelper
    {
        public static async Task<DiscordRole?> GetRoleByName(string name)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            if (ClientManager.client.Guilds[reader.server_id].Roles.Where((role) => role.Value.Name.ToLower() == name).Any())
            {
                return ClientManager.client.Guilds[reader.server_id].Roles.Where((role) => role.Value.Name.ToLower() == name).First().Value;
            }

            return null;
        }
    }
}
