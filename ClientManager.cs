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
        public static GuildSwitch guildSwitch = GuildSwitch.TSOM;
        public static bool launchTicketTrackerSwitchCommandSith = true;
        public static bool launchTicketTrackerSwitchCommandJedi = true;

        public static async Task<string> getGuildId()
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            if(guildSwitch == GuildSwitch.TSOM) 
            {
                return reader.guild_ids.sith;
            }
            else
            {
                return reader.guild_ids.jedi;
            }
        }

        public static async Task<int> minimumTickets()
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            if (guildSwitch == GuildSwitch.TSOM)
            {
                return reader.minimumTicketAmount.ticketAmountSith;
            }
            else
            {
                return reader.minimumTicketAmount.ticketAmountJedi;
            }
        }
    }

    public enum GuildSwitch
    {
        TSOM,
        TJOM
    }
}
