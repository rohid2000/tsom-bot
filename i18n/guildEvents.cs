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
    }

    public class guildEventsRaidStructure
    {
        public string live;
        public string dayLeft;
    }

    public class guildEventsTWStructure
    {
        public string signup;
        public guildEventsTWSDefensetructure defenseUnder20banners;
        public guildEventsTWSDefensetructure defenseOver20banners;
        public string filler;
        public string attack;
    }

    public class guildEventsTWSDefensetructure
    {
        public string timestamp1;
        public string timestamp2;
    }
}
