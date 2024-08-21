using DSharpPlus;
using DSharpPlus.Entities;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.promotions
{
    public class RolePromotionHelper
    {
        public async Task GiveRole(DiscordClient client, Role role, DiscordMember dcMember)
        {
            ConfigReader reader = new();
            await reader.readConfig();;

            DiscordGuild guild = client.Guilds[reader.server_id];
            DiscordRole? dcRole = null;

            DiscordRole acolyteRole = guild.GetRole(reader.roleIds.sith.acolyte);
            DiscordRole apprenticeRole = guild.GetRole(reader.roleIds.sith.apprentice);
            DiscordRole mandalorianRole = guild.GetRole(reader.roleIds.sith.mandalorian);
            DiscordRole sithLordRole = guild.GetRole(reader.roleIds.sith.sithlord);

            DiscordRole younglingRole = guild.GetRole(reader.roleIds.jedi.youngling);
            DiscordRole padawanRole = guild.GetRole(reader.roleIds.jedi.padawan);
            DiscordRole jediKnightRole = guild.GetRole(reader.roleIds.jedi.jediKnight);
            DiscordRole jediMasterRole = guild.GetRole(reader.roleIds.jedi.jediMaster);

            // removes all roles to make sure the top-permission role is only set on the player

            if(!await RoleHelper.hasRole(role, dcMember))
            {
                switch (role)
                {
                    case Role.Acolyte:
                        dcRole = acolyteRole;
                        break;
                    case Role.Apprentice:
                        await dcMember.RevokeRoleAsync(acolyteRole);
                        dcRole = apprenticeRole;
                        break;
                    case Role.Mandalorian:
                        await dcMember.RevokeRoleAsync(apprenticeRole);
                        dcRole = mandalorianRole;
                        break;
                    case Role.SithLord:
                        await dcMember.RevokeRoleAsync(mandalorianRole);
                        dcRole = sithLordRole;
                        break;
                    default:
                        break;
                }
                // Als switch tussen tsom en tjom er is, moeten ze apart behandelt worden
                switch (role)
                {
                    case Role.Youngling:
                        dcRole = younglingRole;
                        break;
                    case Role.Padawan:
                        dcRole = padawanRole;
                        break;
                    case Role.JediKnight:
                        dcRole = jediKnightRole;
                        break;
                    case Role.JediMaster:
                        dcRole = jediMasterRole;
                        break;
                }
            }
            if(dcRole != null)
                await dcMember.GrantRoleAsync(dcRole);
        }
    }
}
