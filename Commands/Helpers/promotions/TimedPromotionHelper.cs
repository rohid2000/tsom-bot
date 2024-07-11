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
            foreach (KeyValuePair<ulong, DiscordMember> member in client.Guilds[reader.server_id].Members)
            {
                DiscordMember dcMember = member.Value;
                if (!HasAdminRole(dcMember) && !dcMember.IsBot)
                {  
                    int totalDays = (int)MathF.Floor((float)(DateTime.Now - dcMember.JoinedAt).TotalDays);
                    RolePromotionHelper helper = new RolePromotionHelper();

                    string? roleName = null;
                    Role? role = null;
                    if (totalDays >= reader.rolePromotionDays.sithlord)
                    {
                        roleName = "Sithlord";
                        role = Role.SithLord;
                    }
                    else if (totalDays >= reader.rolePromotionDays.mandalorian)
                    {
                        roleName = "Mandalorian";
                        role = Role.Mandalorian;
                    }
                    else if (totalDays >= reader.rolePromotionDays.apprentice)
                    {
                        roleName = "Apprentice";
                        role = Role.Apprentice;
                    }
                    else if (totalDays >= reader.rolePromotionDays.acolyte)
                    {
                        roleName = "Acolyte";
                        role = Role.Acolyte;
                    }

                    if (roleName != null && role != null)
                    {
                        if(!await RoleHelper.hasRole(role ?? Role.Acolyte, dcMember))
                        {
                            await helper.GiveRole(client, role ?? Role.Acolyte, dcMember);

                            var channelId = reader.channelIds.promotions;
                            var chan = await client.GetChannelAsync(channelId);

                            await new DiscordMessageBuilder()
                            .WithContent($"{dcMember.Mention} has been promoted to **{roleName}**, Congrationaltions")
                            .SendAsync(chan);
                        }
                    }
                }
            }
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
