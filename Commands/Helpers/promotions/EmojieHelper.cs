using DSharpPlus.Entities;

namespace tsom_bot.Commands.Helpers.promotions
{
    public class EmojieHelper
    {
        public static string GetEmojiesById(Emojies emojie, DiscordGuild guild)
        {
            switch (emojie)
            {
                case Emojies.sithEmoji:
                    return guild.Emojis[1299386369881149491].ToString();
                case Emojies.jediEmoji:
                    return guild.Emojis[1270371545201639444].ToString();
                case Emojies.republicEmoji:
                    return guild.Emojis[738812106270441512].ToString();
            }
            return "";
        }

        public enum Emojies
        {
            sithEmoji,
            jediEmoji,
            republicEmoji
        }
    }
}
