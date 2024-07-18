using DocumentFormat.OpenXml.Spreadsheet;
using DSharpPlus;
using DSharpPlus.Entities;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.config;

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
                if (!hasAdminRole && !dcMember.IsBot)
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
    }
}
