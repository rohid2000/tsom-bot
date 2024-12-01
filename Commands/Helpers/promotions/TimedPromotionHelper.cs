using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Data;
using tsom_bot.config;
using tsom_bot.Fetcher.database;
using tsom_bot.i18n;

namespace tsom_bot.Commands.Helpers.promotions
{
    public static class TimedPromotionHelper
    {
        public async static Task SyncPromotions(i18nStructurePromotionCommandSyncComplete completeMessage, InteractionContext? ctx = null, GuildSwitch? guildSwitch = null)
        {
            ConfigReader reader = new();
            await reader.readConfig();

            List<DiscordMember> acolytePromoters = new List<DiscordMember>();
            List<DiscordMember> apprenticePromoters = new List<DiscordMember>();
            List<DiscordMember> mandalorianPromoters = new List<DiscordMember>();
            List<DiscordMember> sithlordPromoters = new List<DiscordMember>();

            List<DiscordMember> jediMasterPromoters = new List<DiscordMember>();
            List<DiscordMember> jediKnightPromoters = new List<DiscordMember>();
            List<DiscordMember> padawanPromoters = new List<DiscordMember>();
            List<DiscordMember> younglingPromoters = new List<DiscordMember>();

            GuildSwitch currentSwitch;
            if(guildSwitch.HasValue)
            {
                currentSwitch = (GuildSwitch)guildSwitch;
            }
            else
            {
                currentSwitch = ClientManager.guildSwitch;
            }

            IReadOnlyDictionary<ulong, DiscordMember> allMembers = ClientManager.client.Guilds[reader.server_id].Members;
            ulong roleId = currentSwitch == GuildSwitch.TSOM ? reader.clanrole_ids.sith : reader.clanrole_ids.jedi;

            List<KeyValuePair<ulong, DiscordMember>> members = allMembers.Where((member) => member.Value.Roles.Where((role) => role.Id == roleId).Count() > 0).ToList();

            foreach (KeyValuePair<ulong, DiscordMember> member in members)
            {
                DiscordMember dcMember = member.Value;
                bool hasAdminRole = await HasAdminRole(dcMember);
                DataTable result = await Database.SendSqlPull($"SELECT * FROM excludefrompromotion WHERE dcid = {dcMember.Id}");
                if (!hasAdminRole && !dcMember.IsBot && result.Rows.Count == 0)
                {
                    if(ctx != null) 
                    {
                        DiscordWebhookBuilder discordWebhookBuilder = new DiscordWebhookBuilder().WithContent($"Checking player {dcMember.DisplayName}");
                        await ctx.EditResponseAsync(discordWebhookBuilder);
                    }

                    int totalDays = (int)MathF.Floor((float)(DateTime.Now - dcMember.JoinedAt).TotalDays);
                    RolePromotionHelper helper = new RolePromotionHelper();

                    string? roleName = null;
                    Role? role = null;
                    if (currentSwitch == GuildSwitch.TSOM)
                    {
                        if (totalDays >= reader.rolePromotionDays.sith.sithlord)
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
                                await helper.GiveRole(role ?? Role.Acolyte, dcMember);
                                switch (role)
                                {
                                    case Role.Acolyte:
                                        acolytePromoters.Add(dcMember);
                                        break;
                                    case Role.Apprentice:
                                        apprenticePromoters.Add(dcMember);
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
                    else
                    {
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

                        if (roleName != null && role != null)
                        {
                            if (!await RoleHelper.hasRole(role ?? Role.Youngling, dcMember))
                            {
                                await helper.GiveRole(role ?? Role.Youngling, dcMember);

                                switch (role)
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
                else if (result.Rows.Count > 1)
                {
                    RolePromotionHelper helper = new RolePromotionHelper();
                    Role role = convertStringToRole(result.Rows[0].Field<string>("role"));
                    if (!await RoleHelper.hasRole(role, dcMember))
                    {
                        await helper.GiveRole(role, dcMember);
                    }
                }
            }

            DiscordGuild guild = ClientManager.client.Guilds[reader.server_id];

            if (currentSwitch == GuildSwitch.TSOM)
            {
                if(
                    HasPromotions(acolytePromoters) ||
                    HasPromotions(apprenticePromoters) ||
                    HasPromotions(mandalorianPromoters) ||
                    HasPromotions(sithlordPromoters))
                {
                    string tsomMessage = "";
                    DiscordRole acolyteRole = guild.GetRole(reader.roleIds.sith.acolyte);
                    DiscordRole apprenticeRole = guild.GetRole(reader.roleIds.sith.apprentice);
                    DiscordRole mandalorianRole = guild.GetRole(reader.roleIds.sith.mandalorian);
                    DiscordRole sithLordRole = guild.GetRole(reader.roleIds.sith.sithlord);


                    tsomMessage += completeMessage.sith.GetRandomHeader() + "\n\n";

                    tsomMessage += GetRolePromotionsString(acolytePromoters, acolyteRole);
                    tsomMessage += GetRolePromotionsString(apprenticePromoters, apprenticeRole);
                    tsomMessage += GetRolePromotionsString(mandalorianPromoters, mandalorianRole);
                    tsomMessage += GetRolePromotionsString(sithlordPromoters, sithLordRole);

                    tsomMessage += completeMessage.sith.GetRandomFooter();

                    if (ctx != null)
                    {
                        DiscordWebhookBuilder tsomMessageB = new DiscordWebhookBuilder().WithContent(tsomMessage);
                        await ctx.EditResponseAsync(tsomMessageB);
                    }
                    else
                    {
                        DiscordChannel channel = await ClientManager.client.GetChannelAsync(reader.channelIds.sith.promotions);
                        await new DiscordMessageBuilder()
                        .WithContent(tsomMessage)
                        .SendAsync(channel);
                    }
                }
                else
                {
                    if (ctx != null)
                    {
                        DiscordWebhookBuilder tsomMessage = new DiscordWebhookBuilder().WithContent("No Promotions this month");
                        await ctx.EditResponseAsync(tsomMessage);
                    }
                    else
                    {
                        DiscordChannel channel = await ClientManager.client.GetChannelAsync(reader.channelIds.sith.promotions);
                        await new DiscordMessageBuilder()
                        .WithContent("No Promotions this month")
                        .SendAsync(channel);
                    }
                }
            }
            else
            {
                if (
                    HasPromotions(younglingPromoters) ||
                    HasPromotions(padawanPromoters) ||
                    HasPromotions(jediKnightPromoters) ||
                    HasPromotions(jediMasterPromoters))
                {
                    string tjomMessage = "";
                    DiscordRole younglingRole = guild.GetRole(reader.roleIds.jedi.youngling);
                    DiscordRole padawanRole = guild.GetRole(reader.roleIds.jedi.padawan);
                    DiscordRole jediKnightRole = guild.GetRole(reader.roleIds.jedi.jediKnight);
                    DiscordRole jediMasterRole = guild.GetRole(reader.roleIds.jedi.jediMaster);


                    tjomMessage += completeMessage.jedi.GetRandomHeader() + "\n\n";

                    tjomMessage += GetRolePromotionsString(younglingPromoters, younglingRole);
                    tjomMessage += GetRolePromotionsString(padawanPromoters, padawanRole);
                    tjomMessage += GetRolePromotionsString(jediKnightPromoters, jediKnightRole);
                    tjomMessage += GetRolePromotionsString(jediMasterPromoters, jediMasterRole);

                    tjomMessage += completeMessage.jedi.GetRandomFooter();

                    if (ctx != null)
                    {
                        DiscordWebhookBuilder tjomMessageB = new DiscordWebhookBuilder().WithContent(tjomMessage);
                        await ctx.EditResponseAsync(tjomMessageB);
                    }
                    else
                    {
                        DiscordChannel channel = await ClientManager.client.GetChannelAsync(reader.channelIds.jedi.promotions);
                        await new DiscordMessageBuilder()
                        .WithContent(tjomMessage)
                        .SendAsync(channel);
                    }
                }
                else
                {
                    if (ctx != null)
                    {
                        DiscordWebhookBuilder tsomMessage = new DiscordWebhookBuilder().WithContent("No Promotions this month");
                        await ctx.EditResponseAsync(tsomMessage);
                    }
                    else
                    {
                        DiscordChannel channel = await ClientManager.client.GetChannelAsync(reader.channelIds.jedi.promotions);
                        await new DiscordMessageBuilder()
                        .WithContent("No Promotions this month")
                        .SendAsync(channel);
                    }
                }
            }
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
                case Role.Youngling:
                    roleString = "Youngling";
                    break;
                case Role.Padawan:
                    roleString = "Padawan";
                    break;
                case Role.JediKnight:
                    roleString = "JediKnight";
                    break;
                case Role.JediMaster:
                    roleString = "JediMaster";
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

        private static bool HasPromotions(List<DiscordMember> members) 
        {
            return members.Any();
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
                case "Youngling":
                    return Role.Youngling;
                case "Padawan":
                    return Role.Padawan;
                case "JediKnight":
                    return Role.JediKnight;
                case "JediMaster":
                    return Role.JediMaster;
                default:
                    return Role.Acolyte;
            }
        }
    }
}
