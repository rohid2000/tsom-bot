using DSharpPlus.EventArgs;
using DSharpPlus;
using tsom_bot.config;
using DSharpPlus.Entities;
using tsom_bot.i18n;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers.Discord
{
    public static class DiscordJoinHelper
    {
        public static async Task JoinFunction(DiscordMember member)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            //Send welcome message in all channel
            DiscordChannel welcomeChannel = await ClientManager.client.GetChannelAsync(reader.channelIds.test);

            KeyValuePair<IMember, GuildSwitch>? lookupResult = await LookUpNameInGuilds(member.DisplayName);
            string welcomeMessage = "";
            if (lookupResult.HasValue)
            {
                await AsignServerRole(member, lookupResult.Value.Value);
                await SendServerMessage(member, lookupResult.Value.Value);
                await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{lookupResult.Value.Key.playerName}', {member.Id})");

                welcomeMessage = joinMessages.data.roleAssignMessages.welcome;

                welcomeMessage = welcomeMessage.Replace("(newMember)", member.Mention);
                KeyValuePair<string, List<IMention>> formatResult = await DiscordMessageHelper.FormatMessage(welcomeMessage);

                formatResult.Value.Add(new UserMention(member));

                await new DiscordMessageBuilder()
                    .WithContent(formatResult.Key)
                    .WithAllowedMentions(formatResult.Value)
                    .SendAsync(welcomeChannel);
            }
            else
            {
                welcomeMessage = joinMessages.data.roleAssignMessages.namesNotLinked;

                welcomeMessage = welcomeMessage.Replace("(newMember)", member.Mention);
                KeyValuePair<string, List<IMention>> formatResult = await DiscordMessageHelper.FormatMessage(welcomeMessage);

                formatResult.Value.Add(new UserMention(member));

                DiscordMessage message = await new DiscordMessageBuilder()
                    .WithContent(formatResult.Key)
                    .WithAllowedMentions(formatResult.Value)
                    .SendAsync(welcomeChannel);
                
                ClientManager.joinMessageIds.Add(message.Id);
            }
        }

        private static async Task<KeyValuePair<IMember, GuildSwitch>?> LookUpNameInGuilds(string name)
        {
            string guildIdTsom = await ClientManager.getGuildId(GuildSwitch.TSOM);
            string guildIdTjom = await ClientManager.getGuildId(GuildSwitch.TJOM);
            IGuild? guildTsom = await GuildFetcher.GetGuildById(guildIdTsom, true, new());
            IGuild? guildTjom = await GuildFetcher.GetGuildById(guildIdTjom, true, new());

            if (guildTsom != null)
            {
                if (guildTsom.member.Where((member) => member.playerName.ToLower() == name.ToLower()).Any())
                {
                    return new KeyValuePair<IMember, GuildSwitch>(guildTsom.member.Where((member) => member.playerName.ToLower() == name.ToLower()).First(), GuildSwitch.TSOM);
                }
            }

            if (guildTjom != null)
            {
                if (guildTjom.member.Where((member) => member.playerName.ToLower() == name.ToLower()).Any())
                {
                    return new KeyValuePair<IMember, GuildSwitch>(guildTjom.member.Where((member) => member.playerName.ToLower() == name.ToLower()).First(), GuildSwitch.TJOM);
                }
            }

            return null;
        }

        private static async Task AsignServerRole(DiscordMember member, GuildSwitch guild)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();
            ulong roleId = guild == GuildSwitch.TSOM ? reader.clanrole_ids.sith : reader.clanrole_ids.jedi;
            DiscordRole role = ClientManager.client.Guilds[reader.server_id].GetRole(roleId);

            await member.GrantRoleAsync(role);

            // remove member role
            DiscordRole memberRole = ClientManager.client.Guilds[reader.server_id].GetRole(1207772480362774558);
            await member.RevokeRoleAsync(memberRole);
        }

        private static async Task SendServerMessage(DiscordMember member, GuildSwitch guild)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            ulong channelId;
            StringBuilder message = new();
            if (guild == GuildSwitch.TSOM)
            {
                channelId = reader.channelIds.test;
                message.AppendLine(joinMessages.data.roleAssignMessages.welcomeMessageGeneralChat.sith.GetRandomHeader());
            }
            else
            {
                channelId = reader.channelIds.test;
                message.AppendLine(joinMessages.data.roleAssignMessages.welcomeMessageGeneralChat.jedi.GetRandomHeader());
            }
            message.AppendLine(joinMessages.data.roleAssignMessages.welcomeMessageGeneralChat.footer);
            message = message.Replace("(newMember)", member.Mention);

            KeyValuePair<string, List<IMention>> formatResult = await DiscordMessageHelper.FormatMessage(message.ToString());

            formatResult.Value.Add(new UserMention(member));

            DiscordChannel channel = await ClientManager.client.GetChannelAsync(channelId);

            await new DiscordMessageBuilder()
                .WithContent(formatResult.Key)
                .WithAllowedMentions(formatResult.Value)
                .SendAsync(channel);
        }
    }
}
