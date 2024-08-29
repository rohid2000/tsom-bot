using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.config;

namespace tsom_bot
{
    public static class ClientManager
    {
        public static int time;
        public static DateTime timerStartTime;
        public static GuildSwitch guildSwitch = GuildSwitch.Sith;
        public static bool launchTicketTrackCommandSith = true;
        public static bool launchTicketTrackCommandJedi = true;

        public static async Task<string> getGuildId()
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            if(guildSwitch == GuildSwitch.Sith) 
            {
                return reader.guild_ids.sith;
            }
            else
            {
                return reader.guild_ids.jedi;
            }
        }
    }

    public enum GuildSwitch
    {
        Jedi, Sith
    }
}
