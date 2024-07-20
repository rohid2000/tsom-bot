using DSharpPlus;
using DSharpPlus.Entities;
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

            List<DiscordMember> jediMasterPromoters = new List<DiscordMember>();
            List<DiscordMember> jediKnightPromoters = new List<DiscordMember>();
            List<DiscordMember> padawanPromoters = new List<DiscordMember>();
            List<DiscordMember> younglingPromoters = new List<DiscordMember>();

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
                        acolytePromoters.Add(dcMember);                    
                    }

                    if (roleName != null && role != null)
                    {
                        if (!await RoleHelper.hasRole(role ?? Role.Acolyte, dcMember))
                        {
                            await helper.GiveRole(client, role ?? Role.Acolyte, dcMember);

                            switch (role)
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

                    if (totalDays >= reader.rolePromotionDays.jediMaster)
                    {
                        roleName = "JediMaster";
                        role = Role.JediMaster;
                    }
                    else if (totalDays >= reader.rolePromotionDays.jediKnight)
                    {
                        roleName = "JediKnight";
                        role = Role.JediKnight;
                    }
                    else if (totalDays >= reader.rolePromotionDays.padawan)
                    {
                        roleName = "Padawan";
                        role = Role.Padawan;
                    }
                    else if (totalDays >= reader.rolePromotionDays.youngling)
                    {
                        roleName = "Youngling";
                        role = Role.Youngling;
                        younglingPromoters.Add(dcMember);
                    }

                        if (!await RoleHelper.hasRole(role ?? Role.Youngling, dcMember))
                        {
                            await helper.GiveRole(client, role ?? Role.Youngling, dcMember);

                            switch(role)
                            {
                                case Role.Youngling:
                                    younglingPromoters.Add(dcMember);
                                    break;
                                case Role.Padawan:
                                    padawanPromoters.Add(dcMember);
                                    break;
                                case Role.JediKnight:
                                    jediKnightPromoters.Add(dcMember);
                                    break;
                                case Role.JediMaster:
                                    jediMasterPromoters.Add(dcMember);
                                    break;
                            }
                        }
                    }
                }
            }

            DiscordGuild guild = client.Guilds[reader.server_id];

            string tsomMessage = "";
            DiscordRole acolyteRole = guild.GetRole(reader.roleIds.acolyte);
            DiscordRole apprenticeRole = guild.GetRole(reader.roleIds.apprentice);
            DiscordRole mandalorianRole = guild.GetRole(reader.roleIds.mandalorian);
            DiscordRole sithLordRole = guild.GetRole(reader.roleIds.sithlord);


            tsomMessage += "The Sith Will Become All powerful! \n\nCongrats";

            tsomMessage += GetRolePromotionsString(acolytePromoters, acolyteRole);
            tsomMessage += GetRolePromotionsString(apprenticePromoters, apprenticeRole);
            tsomMessage += GetRolePromotionsString(mandalorianPromoters, mandalorianRole);
            tsomMessage += GetRolePromotionsString(sithlordPromoters, sithLordRole);

            tsomMessage += "We are all the Sith!";

            await new DiscordMessageBuilder()
                .WithContent(tsomMessage)
                .SendAsync(chan);

            string tjomMessage = "";
            DiscordRole younglingRole = guild.GetRole(reader.roleIds.acolyte);
            DiscordRole padawanRole = guild.GetRole(reader.roleIds.apprentice);
            DiscordRole jediKnightRole = guild.GetRole(reader.roleIds.mandalorian);
            DiscordRole jediMasterRole = guild.GetRole(reader.roleIds.sithlord);


            tjomMessage += "We are keepers of the peace! \n\nCongrats";

            tjomMessage += GetRolePromotionsString(younglingPromoters, younglingRole);
            tjomMessage += GetRolePromotionsString(padawanPromoters, padawanRole);
            tjomMessage += GetRolePromotionsString(jediKnightPromoters, jediKnightRole);
            tjomMessage += GetRolePromotionsString(jediMasterPromoters, jediMasterRole);

            tjomMessage += "We are all the Jedi!";

            await new DiscordMessageBuilder()
                .WithContent(tjomMessage)
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
