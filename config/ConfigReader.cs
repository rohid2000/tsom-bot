using Newtonsoft.Json;

namespace tsom_bot.config
{
    public class ConfigReader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong server_id { get; set; }
        public string connectionString { get; set; }
        public ChannelIds channelIds { get; set; }
        public ConfigStructureRoleId roleIds { get; set; }
        public ConfigStructureRolePromotionDays rolePromotionDays { get; set; }

        public async Task readConfig()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                ConfigStructure data = JsonConvert.DeserializeObject<ConfigStructure>(json);

                if (data != null)
                {
                    this.token = data.token;
                    this.prefix = data.prefix;
                    this.server_id = data.server_id;
                    this.connectionString = data.connectionString;
                    this.roleIds = data.roleIds;
                    this.channelIds = data.channelIds;  
                    this.rolePromotionDays = data.rolePromotionDays;
                }
            }
        }
    }

    internal class ConfigStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong server_id { get; set; }
        public string connectionString { get; set; }
        public ChannelIds channelIds { get; set; }
        public ConfigStructureRoleId roleIds { get; set; }
        public ConfigStructureRolePromotionDays rolePromotionDays { get; set; }
    }

    public sealed class ChannelIds
    {
        public ulong tsomBotTesting { get; set; }
        public ulong strikeSystem { get; set; }
        public ulong strikeList { get; set; }
        public ulong promotions { get; set; }
    }

    public class ConfigStructureRoleId
    {
        public ulong acolyte { get; set; }
        public ulong apprentice { get; set; }
        public ulong mandalorian { get; set; }
        public ulong sithlord { get; set; }
    }

    public class ConfigStructureRolePromotionDays
    {
        public int acolyte { get; set; }
        public int apprentice { get; set; }
        public int mandalorian { get; set; }
        public int sithlord { get; set; }
    }
}
