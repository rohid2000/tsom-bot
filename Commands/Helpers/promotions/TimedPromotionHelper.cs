using DocumentFormat.OpenXml.Spreadsheet;
using DSharpPlus;
using DSharpPlus.Entities;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.config;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers.promotions
{
    public static class TimedPromotionHelper
    {
        public async static Task SyncPromotions(DiscordClient client)
        {
            ConfigReader reader = new();
            await reader.readConfig();

            var chan = await client.GetChannelAsync(reader.channelIds.promotions);
            List<DiscordMember> acolytePromoters = new List<DiscordMember>();
            List<DiscordMember> apprenticePromoters = new List<DiscordMember>();
            List<DiscordMember> mandalorianPromoters = new List<DiscordMember>();
            List<DiscordMember> sithlordPromoters = new List<DiscordMember>();
            foreach (KeyValuePair<ulong, DiscordMember> member in client.Guilds[reader.server_id].Members)
            {
                DiscordMember dcMember = member.Value;
                bool hasAdminRole = await HasAdminRole(dcMember);
                DataTable result = await Database.SendSqlPull($"SELECT * FROM excludefrompromotion WHERE dcid = {dcMember.Id}");
                if (!hasAdminRole && !dcMember.IsBot && result.Rows.Count == 0)
                {
                    int totalDays = (int)MathF.Floor((float)(DateTime.Now - dcMember.JoinedAt).TotalDays);
                    RolePromotionHelper helper = new RolePromotionHelper();

                    string? roleName = null;
                    Role? role = null;
                    if (
                        totalDays >= reader.rolePromotionDays.sithlord
                        )
                    {
                        roleName = "Sithlord";
                        role = Role.SithLord;
                    }
                    else if (
                        totalDays >= reader.rolePromotionDays.mandalorian &&
                        totalDays < reader.rolePromotionDays.sithlord
                        )
                    {
                        roleName = "Mandalorian";
                        role = Role.Mandalorian;
                    }
                    else if (
                        totalDays >= reader.rolePromotionDays.apprentice &&
                        totalDays < reader.rolePromotionDays.mandalorian
                        )
                    {
                        roleName = "Apprentice";
                        role = Role.Apprentice;
                    }
                    else if (
                        totalDays >= reader.rolePromotionDays.acolyte &&
                        totalDays < reader.rolePromotionDays.apprentice
                        )
                    {
                        roleName = "Acolyte";
                        role = Role.Acolyte;                    
                    }

                    if (roleName != null && role != null)
                    {
                        if(!await RoleHelper.hasRole(role ?? Role.Acolyte, dcMember))
                        {
                            await helper.GiveRole(client, role ?? Role.Acolyte, dcMember);

                            switch(role) 
                            {
                                case Role.Acolyte:
                                    acolytePromoters.Add(dcMember);
                                    break;
                                case Role.Apprentice:
                                    acolytePromoters.Add(dcMember);
                                    break;
                                case Role.Mandalorian:
                                    mandalorianPromoters.Add(dcMember);
                                    break;
                                case Role.SithLord:
                                    sithlordPromoters.Add(dcMember);
                                    break;
                            }
                        }
                    }
                }
                else if(result.Rows.Count > 1)
                {
                    RolePromotionHelper helper = new RolePromotionHelper();
                    Role role = convertStringToRole(result.Rows[0].Field<string>("role"));
                    if (!await RoleHelper.hasRole(role, dcMember))
                    {
                        await helper.GiveRole(client, role, dcMember);
                    }
                }
            }

            string message = "";
            DiscordGuild guild = client.Guilds[reader.server_id];
            DiscordRole acolyteRole = guild.GetRole(reader.roleIds.acolyte);
            DiscordRole apprenticeRole = guild.GetRole(reader.roleIds.apprentice);
            DiscordRole mandalorianRole = guild.GetRole(reader.roleIds.mandalorian);
            DiscordRole sithLordRole = guild.GetRole(reader.roleIds.sithlord);


            message += "The Sith Will Become All powerful! \n\nCongrats";

            message += GetRolePromotionsString(acolytePromoters, acolyteRole);
            message += GetRolePromotionsString(apprenticePromoters, apprenticeRole);
            message += GetRolePromotionsString(mandalorianPromoters, mandalorianRole);
            message += GetRolePromotionsString(sithlordPromoters, sithLordRole);

            message += "We are All the Sith!";

            await new DiscordMessageBuilder()
                .WithContent(message)
                .SendAsync(chan);
        }

        public async static Task ExludePlayerFromPromotion(DiscordUser dcMember, Role role)
        {
            string roleString;

            switch(role)
            {
                case Role.Acolyte:
                    roleString = "Acolyte";
                    break;
                case Role.Apprentice:
                    roleString = "Apprentice";
                    break;
                case Role.Mandalorian:
                    roleString = "Mandalorian";
                    break;
                case Role.SithLord:
                    roleString = "SithLord";
                    break;
                default:
                    roleString = "";
                    break;
            }

            await Database.SendSqlSave($"INSERT INTO excludefrompromotion (dcid, rank) VALUES ({dcMember.Id}, '{roleString}')");
        }

        private static string GetRolePromotionsString(List<DiscordMember> members, DiscordRole role)
        {
            string message = "";

            foreach (DiscordMember dcMember in members)
            {
                if (dcMember != members.Last())
                {
                    message += dcMember.Mention + ", ";
                }
                else
                {
                    message += dcMember.Mention + " ";
                }
            }

            if (members.Count > 1)
            {
                message += $"have been promoted to {role.Mention}";
            }
            else if (members.Count == 1)
            {
                message += $"has been promoted to {role.Mention}";
            }

            if(members.Count > 0)
            {
                message += "!\n\n";
            } 

            return message;
        }

        private async static Task<bool> HasAdminRole(DiscordMember dcMember)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();
            List<ulong> adminRoleIds = reader.adminRoleIds.ToList();
            foreach(ulong roleId in adminRoleIds) 
            {
                if(dcMember.Roles.Where(i => i.Id == roleId).Any())
                {
                    return true;
                }
            }
            return false;
        }

        private static Role convertStringToRole(string roleString)
        {
            switch (roleString)
            {
                case "Acolyte":
                    return Role.Acolyte;
                case "Apprentice":
                    return Role.Apprentice;
                case "Mandalorian":
                    return Role.Mandalorian;
                case "SithLord":
                    return Role.SithLord;
                default:
                    return Role.Acolyte;
            }
        }
    }
}
