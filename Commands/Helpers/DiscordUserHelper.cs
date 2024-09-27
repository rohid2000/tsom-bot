using DSharpPlus.Entities;
using MySqlX.XDevAPI.Common;
using System.Data;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers
{
    public static class DiscordUserHelper
    {
        public async static Task<DiscordMember?> GetDiscordUserFromIngameName(string playerName, IReadOnlyDictionary<ulong, DiscordMember> dcMembers)
        {
            DataTable result = await Database.SendSqlPull($"SELECT (discordId) FROM sync WHERE playerName = '{playerName}'");

            if(result.Rows.Count == 0) { return null; }
            ulong discordId = (ulong)result.Rows[0].Field<Int64>("discordId");
            return dcMembers.Where(i => i.Value.Id == discordId).ToArray()[0].Value;
        }

        public async static Task<DataTable> GetLinkedAccounts(DiscordMember member)
        {
            return await Database.SendSqlPull($"SELECT * FROM `sync` WHERE discordId = '{member.Id}'");
        }

        public async static Task<DataTable> GetLinkedAccounts(DiscordUser member)
        {
            return await Database.SendSqlPull($"SELECT * FROM `sync` WHERE discordId = '{member.Id}'");
        }
    }
}
