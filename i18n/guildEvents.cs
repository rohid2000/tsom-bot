using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsom_bot.i18n
{
    public static class guildEvents
    {
        public static guildEventsStructure data;
        public static async void load()
        {
            using (StreamReader reader = new StreamReader("guildEvents.json"))
            {
                string json = await reader.ReadToEndAsync();
                guildEventsStructure data = JsonConvert.DeserializeObject<guildEventsStructure>(json);

                if (data != null)
                {
                    guildEvents.data = data;
                }
            }
        }
    }

    public class guildEventsStructure
    {
        public guildEventsRaidStructure raid;
        public guildEventsTWStructure tw;
        public guildEventsTBStructure tb;
    }

    public class guildEventsRaidStructure
    {
        public string live;
        public string dayLeft;
    }

    public class guildEventsTWStructure
    {
        public string[] footer;
        public string signup;
        public guildEventsTWSDefensetructure defenseUnder20zones;
        public guildEventsTWSDefensetructure defenseOver20zones;
        public string filler;
        public string attack;
    }

    public class guildEventsTWSDefensetructure
    {
        public string pingMessage;
        public string timestamp1;
        public string timestamp2;
    }

    public class guildEventsTBStructure
    {
        public string[] header;
        public string[] phaseGenericFooter;
        public string[] phase4Footer;
        public guildEventsTBPhasePingStructure separatistMight;
        public guildEventsTBPhasePingStructure republicOffense;

        public string GetRandomFooter(GuildSwitch guild)
        {
            string footerString = phaseGenericFooter[phaseGenericFooter.Length - 1];
            footerString = footerString.Replace("(guild)", guild.ToString());
            return footerString;
        }

        public string GetRandomHeader(short phase, TBType type)
        {
            string headerString = header[header.Length - 1];
            headerString = headerString.Replace("(phase)", $"Phase {phase}");
            headerString = headerString.Replace("(type)", type.ToString());
            return headerString;
        }

        public string GetRandomPhase4Footer(GuildSwitch guild)
        {
            string footerString = phase4Footer[phase4Footer.Length - 1];
            footerString = footerString.Replace("(guild)", guild.ToString());
            return footerString;
        }
    }

    public class guildEventsTBPhasePingStructure
    {
        public string phase1pingMessage;
        public string phase2pingMessage;
        public string phase3pingMessage;
        public string phase4pingMessage;
        public string? specificFooter;
    }
}
