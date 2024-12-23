using DSharpPlus.Entities;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers
{
    public static class SyncCommandHelper
    {
        public async static Task SyncAllNames(List<DiscordMember> dcMembers)
        {
            string guildId = await ClientManager.getGuildId();
            IGuild? guild = await GuildFetcher.GetGuildById(guildId, true, new());

            if (guild != null)
            {
                foreach(IMember member in guild.member)
                {
                    var memberResult = dcMembers.Where((m) => m.DisplayName.ToLower() == member.playerName.ToLower());

                    if (memberResult.Any())
                    {
                        DiscordMember dcMember = memberResult.First();

                        await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{dcMember.DisplayName.ToLower()}', {dcMember.Id})");
                    }
                }
            }
        }
    }
}
