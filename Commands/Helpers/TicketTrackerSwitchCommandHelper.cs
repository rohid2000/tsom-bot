using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsom_bot.Commands.Helpers
{
    public static class TicketTrackerSwitchCommandHelper
    {
        public static void SwitchLaunchTicketTrackCommand()
        {
            if (ClientManager.guildSwitch == GuildSwitch.TJOM)
            {
                ClientManager.launchTicketTrackerSwitchCommandJedi = !ClientManager.launchTicketTrackerSwitchCommandJedi;
            }

            if (ClientManager.guildSwitch == GuildSwitch.TSOM)
            {
                ClientManager.launchTicketTrackerSwitchCommandSith = !ClientManager.launchTicketTrackerSwitchCommandSith;
            }
        }
    }
}
