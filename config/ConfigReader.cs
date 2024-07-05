using Newtonsoft.Json;

namespace tsom_bot.config
{
    public class ConfigReader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ChannelIds channelIds { get; set; }
        public ConfigStructureRoleId roleIds { get; set; }

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
                    this.roleIds = data.roleids;
                    this.channelIds = data.channelIds;  
                }
            }
        }
    }

    internal class ConfigStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ChannelIds channelIds { get; set; }
        public ConfigStructureRoleId roleIds { get; set; }
    }

    public sealed class ChannelIds
    {
        public ulong tsomBotTesting { get; set; }
        public ulong strikeSystem { get; set; }
        public ulong strikeList { get; set; }
        public ConfigStructureRoleId roleids { get; set; }
    }

    public class ConfigStructureRoleId
    {
        public ulong acolyte { get; set; }
        public ulong apprentice { get; set; }
        public ulong mandalorian { get; set; }
        public ulong sithlord { get; set; }
    }
}
