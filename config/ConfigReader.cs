using Newtonsoft.Json;
using tsom_bot.Fetcher.azure;

namespace tsom_bot.config
{
    public class ConfigReader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public ulong server_id { get; set; }
        public string connectionString { get; set; }
        public ConfigStructureGuildIds guild_ids { get; set; }
        public ConfigStructureClanRoleIds clanrole_ids { get; set; }
        public ChannelIds channelIds { get; set; }
        public ulong[] adminRoleIds { get; set; }
        public ConfigStructureRoleId roleIds { get; set; }
        public ConfigStructureRolePromotionDays rolePromotionDays { get; set; }
        public ConfigureStructureMinimumTicketAmountSithOrJedi minimumTicketAmount { get; set; }
        public ConfigureStructureStrikeListSendTime strikeListSendTime { get; set; }

        public async Task readConfig()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                ConfigStructure data = JsonConvert.DeserializeObject<ConfigStructure>(json);

                if (data != null)
                {
                    this.token = await FetchSecretHelper.FetchSecret("tsom-bot-token");
                    this.prefix = data.prefix;
                    this.server_id = data.server_id;
                    this.connectionString = await FetchSecretHelper.FetchSecret("connectionString");
                    this.guild_ids = data.guild_ids;
                    this.clanrole_ids = data.clanrole_ids;
                    this.adminRoleIds = data.adminRoleIds;
                    this.roleIds = data.roleIds;
                    this.channelIds = data.channelIds;
                    this.rolePromotionDays = data.rolePromotionDays;
                    this.minimumTicketAmount = data.minimumTicketAmount;
                    this.strikeListSendTime = data.strikeListSendTime;
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
        public ConfigStructureGuildIds guild_ids { get; set; }
        public ConfigStructureClanRoleIds clanrole_ids { get; set; }
        public ChannelIds channelIds { get; set; }
        public ulong[] adminRoleIds { get; set; }
        public ConfigStructureRoleId roleIds { get; set; }
        public ConfigStructureRolePromotionDays rolePromotionDays { get; set; }
        public ConfigureStructureMinimumTicketAmountSithOrJedi minimumTicketAmount { get; set; }
        public ConfigureStructureStrikeListSendTime strikeListSendTime { get; set; }
    }

    public sealed class ChannelIds
    {
        public Channel jedi { get; set; }
        public Channel sith { get; set; }
        public ulong test { get; set; }
    }

    public class Channel
    {
        public ulong strikeList { get; set; }
        public ulong promotions { get; set; }
        public ulong commands_private { get; set; }
        public ulong commands_public { get; set; }
    }

    public class ConfigStructureRoleId
    {
        public ConfigStructureRolesJedi jedi { get; set; }
        public ConfigStructureRolesSith sith { get; set; }
    }

    public class ConfigStructureRolesSith
    {
        public ulong acolyte { get; set; }
        public ulong apprentice { get; set; }
        public ulong mandalorian { get; set; }
        public ulong sithlord { get; set; }
    }
    public class ConfigStructureRolesJedi
    {
        public ulong youngling { get; set; }
        public ulong padawan { get; set; }
        public ulong jediKnight { get; set; }
        public ulong jediMaster { get; set; }
    }

    public class ConfigStructureRolePromotionDays
    {
        public ConfigStructureRolePromotionDaysJedi jedi { get; set; }
        public ConfigStructureRolePromotionDaysSith sith { get; set; }
    }

    public class ConfigStructureRolePromotionDaysSith
    {
        public int acolyte { get; set; }
        public int apprentice { get; set; }
        public int mandalorian { get; set; }
        public int sithlord { get; set; }
    }

    public class ConfigStructureRolePromotionDaysJedi
    {
        public int youngling { get; set; }
        public int padawan { get; set; }
        public int jediKnight { get; set; }
        public int jediMaster { get; set; }
    }

    public class ConfigureStructureMinimumTicketAmountSithOrJedi
    {
        public int ticketAmountSith { get; set; }
        public int ticketAmountJedi { get; set; }
    }

    public class ConfigureStructureStrikeListSendTime
    {
        public int hour { get; set; }
        public int minute { get; set; }
        public int second { get; set; }
    }

    public class ConfigStructureGuildIds
    {
        public string jedi;
        public string sith;
    }

    public class ConfigStructureClanRoleIds
    {
        public ulong jedi;
        public ulong sith;
    }
}
