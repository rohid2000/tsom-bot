using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Data;
using tsom_bot.config;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers.promotions
{
    public static class TimedPromotionHelper
    {
        public async static Task SyncPromotions(DiscordClient client, InteractionContext ctx)
        {
            ConfigReader reader = new();
            await reader.readConfig();

            var chan = await client.GetChannelAsync(reader.channelIds.sith.promotions);
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
                DataTable result = await Database.SendSqlPull($"SELECT * FROM excludefrompromotion WHERE dcid = {dcMember.Id}");
                if (!hasAdminRole && !dcMember.IsBot && result.Rows.Count == 0)
                {
                    int totalDays = (int)MathF.Floor((float)(DateTime.Now - dcMember.JoinedAt).TotalDays);
                    RolePromotionHelper helper = new RolePromotionHelper();

                    string? roleName = null;
                    Role? role = null;
                    if (
                        totalDays >= reader.rolePromotionDays.sith.sithlord
                        )
                    {
                        roleName = "Sithlord";
                        role = Role.SithLord;
                    }
                    else if (
                        totalDays >= reader.rolePromotionDays.sith.mandalorian &&
                        totalDays < reader.rolePromotionDays.sith.sithlord
                        )
                    {
                        roleName = "Mandalorian";
                        role = Role.Mandalorian;
                    }
                    else if (
                        totalDays >= reader.rolePromotionDays.sith.apprentice &&
                        totalDays < reader.rolePromotionDays.sith.mandalorian
                        )
                    {
                        roleName = "Apprentice";
                        role = Role.Apprentice;
                    }
                    else if (
                        totalDays >= reader.rolePromotionDays.sith.acolyte &&
                        totalDays < reader.rolePromotionDays.sith.apprentice
                        )
                    {
                        roleName = "Acolyte";
                        role = Role.Acolyte;                    
                    }

                    if (roleName != null && role != null)
                    {
                        if (!await RoleHelper.hasRole(role ?? Role.Acolyte, dcMember))
                        {
                            //await helper.GiveRole(client, role ?? Role.Acolyte, dcMember);

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

                    if (totalDays >= reader.rolePromotionDays.jedi.jediMaster)
                    {
                        roleName = "JediMaster";
                        role = Role.JediMaster;
                    }
                    else if (totalDays >= reader.rolePromotionDays.jedi.jediKnight)
                    {
                        roleName = "JediKnight";
                        role = Role.JediKnight;
                    }
                    else if (totalDays >= reader.rolePromotionDays.jedi.padawan)
                    {
                        roleName = "Padawan";
                        role = Role.Padawan;
                    }
                    else if (totalDays >= reader.rolePromotionDays.jedi.youngling)
                    {
                        roleName = "Youngling";
                        role = Role.Youngling;
                    }

                        if (!await RoleHelper.hasRole(role ?? Role.Youngling, dcMember))
                        {
                            // Function call commented so that Roles won'be given when being tested
                            //await helper.GiveRole(client, role ?? Role.Youngling, dcMember);

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
                else if(result.Rows.Count > 1)
                {
                    RolePromotionHelper helper = new RolePromotionHelper();
                    Role role = convertStringToRole(result.Rows[0].Field<string>("role"));
                    if (!await RoleHelper.hasRole(role, dcMember))
                    {
                        //await helper.GiveRole(client, role, dcMember);
                    }
                }
            }

            DiscordGuild guild = client.Guilds[reader.server_id];

            string tsomMessage = "";
            DiscordRole acolyteRole = guild.GetRole(reader.roleIds.sith.acolyte);
            DiscordRole apprenticeRole = guild.GetRole(reader.roleIds.sith.apprentice);
            DiscordRole mandalorianRole = guild.GetRole(reader.roleIds.sith.mandalorian);
            DiscordRole sithLordRole = guild.GetRole(reader.roleIds.sith.sithlord);


            tsomMessage += i18n.i18n.data.commands.promotion.sync.complete.header;

            tsomMessage += GetRolePromotionsString(acolytePromoters, acolyteRole);
            tsomMessage += GetRolePromotionsString(apprenticePromoters, apprenticeRole);
            tsomMessage += GetRolePromotionsString(mandalorianPromoters, mandalorianRole);
            tsomMessage += GetRolePromotionsString(sithlordPromoters, sithLordRole);

            tsomMessage += i18n.i18n.data.commands.promotion.sync.complete.footer;

            DiscordWebhookBuilder tsomMessageB = new DiscordWebhookBuilder().WithContent(tsomMessage);
            await ctx.EditResponseAsync(tsomMessageB);

            string tjomMessage = "";
            DiscordRole younglingRole = guild.GetRole(reader.roleIds.sith.acolyte);
            DiscordRole padawanRole = guild.GetRole(reader.roleIds.sith.apprentice);
            DiscordRole jediKnightRole = guild.GetRole(reader.roleIds.sith.mandalorian);
            DiscordRole jediMasterRole = guild.GetRole(reader.roleIds.sith.sithlord);


            tjomMessage += "We are keepers of the peace! \n\nCongrats";

            tjomMessage += GetRolePromotionsString(younglingPromoters, younglingRole);
            tjomMessage += GetRolePromotionsString(padawanPromoters, padawanRole);
            tjomMessage += GetRolePromotionsString(jediKnightPromoters, jediKnightRole);
            tjomMessage += GetRolePromotionsString(jediMasterPromoters, jediMasterRole);

            tjomMessage += "We are all the Jedi!";

            DiscordWebhookBuilder tjomMessageB = new DiscordWebhookBuilder().WithContent(tjomMessage);
            await ctx.EditResponseAsync(tjomMessageB);
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
