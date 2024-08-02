using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Data;
using tsom_bot.Commands.Helpers;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands
{
    public class DiscordNameSync : ApplicationCommandModule
    {
        [SlashCommandGroup("sync", "sync your discord account with your Swgoh account")]
        public class DiscordNameSyncContainer
        {
            [SlashCommand("name", "give your ingame name and it gets linked to your discord account")]
            public async Task NameSyncCommand(InteractionContext ctx, [Option("name", "your ingame name")] string name, [Option("override", "overrides an already linked name")] bool overrideB = false)
            {
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.sync.name.loading);
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.sync.name.fail);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                await ctx.EditResponseAsync(loadingMessage);
                try
                {
                    string messageS = "";
                    if (overrideB)
                    {
                        await Database.SendSqlSave($"DELETE FROM sync WHERE discordId = {ctx.Member.Id}");
                        await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{name}', {ctx.Member.Id})");

                        messageS = i18n.i18n.Transform(i18n.i18n.data.commands.sync.name.complete, ctx.Member);
                    }
                    else
                    {

                        DataTable result = await DiscordUserHelper.GetLinkedAccounts(ctx.Member);
                        if (result.Rows.Count == 0)
                        {
                            await Database.SendSqlSave($"INSERT INTO sync (playerName, discordId) VALUES ('{name}', {ctx.Member.Id})");

                            messageS = i18n.i18n.Transform(i18n.i18n.data.commands.sync.name.complete, ctx.Member);
                        }
                        else
                        {
                            messageS = i18n.i18n.Transform(i18n.i18n.data.commands.sync.name.already_linked, ctx.Member);
                        }
                    }
                    DiscordWebhookBuilder message = new DiscordWebhookBuilder().WithContent(messageS);
                    await ctx.EditResponseAsync(message);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(failMessage);
                    Console.WriteLine(i18n.i18n.Transform(i18n.i18n.data.commands.sync.name.fail, ctx.Member) + "\n ERROR: " + ex.Message);
                }
            }

            [SlashCommand("nolist", "get a list of players who are not yet linked")]
            public async Task NoListCommand(InteractionContext ctx)
            {
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.sync.nolist.loading);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                await ctx.EditResponseAsync(loadingMessage);

                DataTable resultSync = await Database.SendSqlPull("SELECT * FROM sync");
                List<DiscordMember> dcMembers = (await ctx.Guild.GetAllMembersAsync()).ToList();
                List<DiscordMember> noSyncList = new();

                foreach (DiscordMember dcMember in dcMembers)
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(i18n.i18n.Transform(i18n.i18n.data.commands.sync.nolist.fail, ctx.Member) + "\n ERROR: " + ex.Message);
                    }
                }

                string messageS = "**Warning**: \n";
                foreach (DiscordMember dcMember in noSyncList)
                {
                    if (!dcMember.IsBot)
                    {
                        messageS += i18n.i18n.Transform(i18n.i18n.data.commands.sync.nolist.complete, dcMember);
                    }
                }

                DiscordWebhookBuilder message = new DiscordWebhookBuilder().WithContent(messageS);
                await ctx.EditResponseAsync(message);
            }

            [SlashCommand("test", "test if username is linked to discord account")]
            public async Task TestNameCommand(InteractionContext ctx, [Option("name", "ingame name")] string name)
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);
                DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.sync.test.loading);
                DiscordWebhookBuilder failMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.sync.test.fail);
                DataTable? result = null;
                string linkedMessageName = "";

                await ctx.EditResponseAsync(loadingMessage);

                try
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        result = await DiscordUserHelper.GetLinkedAccounts(ctx.Member);

                        foreach (DataRow row in result.Rows)
                        {
                            linkedMessageName += row.Field<string>("playerName");

                            if (result.Rows.Count > 1 && row != result.Rows[result.Rows.Count - 1])
                            {
                                linkedMessageName += " , ";
                            }
                        }
                    }
                    else
                    {
                        result = await Database.SendSqlPull($"SELECT * FROM sync WHERE playerName = '{name.ToLower()}'");
                        linkedMessageName = ctx.Guild.Members[(ulong)result.Rows[0].Field<Int64>("discordId")].Mention;
                    }
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(failMessage);
                    Console.WriteLine(i18n.i18n.Transform(i18n.i18n.data.commands.sync.test.fail, ctx.Member) + "\n ERROR: " + ex.Message);
                }

                if (result != null && result.Rows.Count >= 1)
                {
                    DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(i18n.i18n.data.commands.sync.test.complete + " " + linkedMessageName);
                    await ctx.EditResponseAsync(completeMessage);
                }
                else
                {
                    DiscordWebhookBuilder noAccountMessage = new DiscordWebhookBuilder().WithContent(name == "" ? i18n.i18n.data.commands.sync.test.no_link_self : i18n.i18n.data.commands.sync.test.no_link_name);
                    await ctx.EditResponseAsync(noAccountMessage);
                }
            }
        }
    }
}