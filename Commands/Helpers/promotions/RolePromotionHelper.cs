using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.promotions
{
    public class RolePromotionHelper
    {
        public async void GiveRole(DiscordClient client, Role role, DiscordMember dcMember)
        {
            ConfigReader reader = new();
            await reader.readConfig();;

            DiscordGuild guild = client.Guilds[reader.server_id];
            DiscordRole? dcRole;

            DiscordRole acolyteRole = guild.GetRole(reader.roleIds.acolyte);
            DiscordRole apprenticeRole = guild.GetRole(reader.roleIds.acolyte);
            DiscordRole mandalorianRole = guild.GetRole(reader.roleIds.acolyte);
            DiscordRole sithLordRole = guild.GetRole(reader.roleIds.acolyte);

            // removes all roles to make sure the top-permission role is only set on the player
            await dcMember.RevokeRoleAsync(acolyteRole);
            await dcMember.RevokeRoleAsync(apprenticeRole);
            await dcMember.RevokeRoleAsync(mandalorianRole);
            await dcMember.RevokeRoleAsync(sithLordRole);
            switch(role)
            {
                case Role.Acolyte:
                    dcRole = acolyteRole;
                    break;
                case Role.Apprentice:
                    dcRole = apprenticeRole;
                    break;
                case Role.Mandalorian:
                    dcRole = mandalorianRole;
                    break;
                case Role.SithLord:
                    dcRole = sithLordRole;
                    break;
                default:
                    dcRole = null;
                    break;
            }

            if (!dcMember.Roles.Contains(dcRole) && dcRole != null)
            {
                await dcMember.GrantRoleAsync(dcRole);
            }
        }
    }
}
