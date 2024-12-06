using DSharpPlus.Entities;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.Discord
{
    public class EmojieHelper
    {
        public static async Task<string> GetEmojieByName(string name)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            DiscordGuild guild = ClientManager.client.Guilds[reader.server_id];
            switch (name)
            {
                case "sith":
                    return guild.Emojis[1299386369881149491].ToString();
                case "jedi":
                    return guild.Emojis[1270371545201639444].ToString();
                case "republic":
                    return guild.Emojis[738812106270441512].ToString();
            }
            return "";
        }
    }
}
