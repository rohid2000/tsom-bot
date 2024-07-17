using DocumentFormat.OpenXml.Spreadsheet;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Numerics;
using tsom_bot.Commands.Helpers;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands
{
    public class DiscordNameSync : BaseCommandModule
    {
        [Command("sync")]
        public async Task templateCommand(CommandContext ctx, string param = "", string param2 = "", string param3 = "")
        {
            string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
            ObservableCollection<IGuild> guildData = new();
            guildData.Add(await GuildFetcher.GetGuildById(guildId, true, new()));
            IGuild guild = guildData[0];
            List<DiscordMember> dcMembers = (await ctx.Guild.GetAllMembersAsync()).ToList();

            if (param == "nolist")
            {
                await this.SendNoSyncList(ctx, dcMembers);
            } 
            else if (param == "name")
            {
                if(param2 != "")
                {
                    if(param3 == "override")
                    {
                        await this.SyncName(ctx, param2, true);
                    }
                    else
                    {
                        await this.SyncName(ctx, param2);
                    }  
                } else
                {
                    await new DiscordMessageBuilder()
                    .WithContent("You didn't provide a name | Usage: `/sync name {name}`")
                    .SendAsync(ctx.Channel);
                }
            }
            else if (param == "add")
            {
                if(param2 == "")
                {
                    await new DiscordMessageBuilder()
                    .WithContent("You didn't provide a name | Usage: `/sync add name {name}`")
                    .SendAsync(ctx.Channel);
                }
                else
                {
                    await SyncNameAdd(ctx, param2);
                }
            }
            else if (param == "remove")
            {
                if (param2 == "")
                {
                    await new DiscordMessageBuilder()
                    .WithContent("You didn't provide a name | Usage: `/sync remove name {name}`")
                    .SendAsync(ctx.Channel);
                }
                else
                {
                    await SyncNameRemove(ctx, param2);
                }
            }
            else if (param == "test")
            {
                DataTable result;
                string linkedMessageName = "";
                if (param2 == "")
                {
                    result = await DiscordUserHelper.GetLinkedAccounts(ctx.Member);
                    foreach(DataRow row in result.Rows)
                    {
                        linkedMessageName += row.Field<string>("playerName");

                        if(result.Rows.Count > 1 && row != result.Rows[result.Rows.Count - 1])
                        {
                            linkedMessageName += " , ";
                        }
                    }
                }
                else
                {
                    result = await Database.SendSqlPull($"SELECT * FROM sync WHERE playerName = '{param2.ToLower()}'");
                    linkedMessageName = ctx.Guild.Members[(ulong)result.Rows[0].Field<Int64>("discordId")].Mention;
                }
                if(result.Rows.Count >= 1)
                {
                    await new DiscordMessageBuilder()
                    .WithContent("You're account is linked to " + linkedMessageName)
                    .SendAsync(ctx.Channel);
                }
                else
                {
                    string message = param2 == "" ? "You dont have a account linked to you" : "this name has no account linked to it";
                    await new DiscordMessageBuilder()
                    .WithContent(message)
                    .SendAsync(ctx.Channel);
                }
            }
            else
            {
                await this.SyncDatabase(ctx, guild, dcMembers);
            }
        }

        private async Task SendNoSyncList(CommandContext ctx, List<DiscordMember> dcMembers)
        {
            DataTable resultSync = await Database.SendSqlPull("SELECT * FROM sync");
            List<DiscordMember> noSyncList = new();

            foreach(DiscordMember dcMember in dcMembers) 
            {
                try
                {
                    List<DataRow> results = resultSync.AsEnumerable().Where(i => (ulong)i.Field<Int64>("discordId") == dcMember.Id).ToList();
                    if (results.Count() == 0)
                    {
                        noSyncList.Add(dcMember);
                    }
                    else
                    {
                        if (results[0].Field<string>("playerName").ToLower() != dcMember.DisplayName.ToLower())
                        {
                            noSyncList.Add(dcMember);
                        }
                    }
                } catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }

            }

            string message = "**Warning**: \n";
            foreach(DiscordMember dcMember in noSyncList)
            {
                if(!dcMember.IsBot) 
                {
                    message += $"{dcMember.Mention}, Discord name and ingame name differ. \n";
                }
            }

            await new DiscordMessageBuilder()
                .WithContent(message)
                .SendAsync(ctx.Channel);
        }

        private async Task SyncName(CommandContext ctx, string name, bool overide = false)
        {
            if(overide)
            {
                await Database.SendSqlSave($"DELETE FROM sync WHERE discordId = {ctx.Member.Id}");
                await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{name}', {ctx.Member.Id})");

                await new DiscordMessageBuilder()
                .WithContent("Synced your name with override")
                .SendAsync(ctx.Channel);
            }
            else
            {

                DataTable result = await DiscordUserHelper.GetLinkedAccounts(ctx.Member);
                if (result.Rows.Count == 0)
                {
                    await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{name}', {ctx.Member.Id})");

                    await new DiscordMessageBuilder()
                    .WithContent("Synced your name")
                    .SendAsync(ctx.Channel);
                }
                else
                {
                    await new DiscordMessageBuilder()
                    .WithContent("This discord user is already linked | Use `/sync name {name} overide` to override this value")
                    .SendAsync(ctx.Channel);
                }
            }
        }

        private async Task SyncNameAdd(CommandContext ctx, string name)
        {
            await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{name}', {ctx.Member.Id})");
            DataTable result = await DiscordUserHelper.GetLinkedAccounts(ctx.Member);

            await new DiscordMessageBuilder()
            .WithContent("Synced your name: there are " + result.Rows.Count + " name*(s)* synced")
            .SendAsync(ctx.Channel);
        }
        private async Task SyncNameRemove(CommandContext ctx, string name)
        {
            await Database.SendSqlSave($"DELETE FROM sync WHERE playerName = '{name}'");
            DataTable result = await DiscordUserHelper.GetLinkedAccounts(ctx.Member);

            await new DiscordMessageBuilder()
            .WithContent("Removed your name: there are " + result.Rows.Count + "name*(s)* synced")
            .SendAsync(ctx.Channel);
        }

        private async Task SyncDatabase(CommandContext ctx, IGuild guildData, List<DiscordMember> dcMembers)
        {
            Database.SendSqlSave("DELETE FROM sync"); // clear table
            foreach (IMember member in guildData.member)
            {
                List<DiscordMember> resultList = dcMembers.Where(i => i.DisplayName.ToLower() == member.playerName.ToLower()).ToList();
                if (resultList.Count > 0)
                {
                    DiscordMember dcMember = resultList[0];
                    if(!dcMember.IsBot) 
                    {
                        Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{member.playerName}', {dcMember.Id})");
                    }   
                }
            }
            await new DiscordMessageBuilder()
            .WithContent("Names synced")
            .SendAsync(ctx.Channel);
        }
    }
}
