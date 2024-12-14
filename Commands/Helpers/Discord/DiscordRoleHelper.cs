using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.Discord
{
    public static class DiscordRoleHelper
    {
        public static async Task<DiscordRole?> GetRoleByName(string name)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            string formatedName = Regex.Replace(name, @"[^a-zA-Z0-9]", "").ToLower();

            var roleExists = ClientManager.client.Guilds[reader.server_id].Roles.Where((role) =>
            {
                string formatedRoleName = Regex.Replace(role.Value.Name, @"[^a-zA-Z0-9]", "").ToLower();
                return formatedRoleName == formatedName;
            }).Any();

            if (roleExists)
            {
                var role = ClientManager.client.Guilds[reader.server_id].Roles.Where((role) =>
                {
                    string formatedRoleName = Regex.Replace(role.Value.Name, @"[^a-zA-Z0-9]", "").ToLower();
                    return formatedRoleName == formatedName;
                }).AsEnumerable();

                return role.First().Value;
            }

            return null;
        }
    }
}
