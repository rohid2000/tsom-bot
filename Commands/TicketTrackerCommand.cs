using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Mysqlx.Prepare;
using System.Net.Mail;
using tsom_bot.Commands.Helpers;
using tsom_bot.Commands.Helpers.Discord;

namespace tsom_bot.Commands
{
    public class TicketTrackerCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("tickets", "To return strike data")]
        public class TicketTrackerContainer : ApplicationCommandModule
        {
            [SlashCommandGroup("sync", "To sync strike data")]
            public class SyncContainer : ApplicationCommandModule
            {
                [SlashCommand("go", "Syncs the guilds data with the database")]
                public async Task SyncCommand(InteractionContext ctx)
                {
                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.go,
                        async () =>
                        {
                            string guildId = await ClientManager.getGuildId();
                            int minimumTicketAmount = await ClientManager.minimumTickets();
                            TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                            await helper.SaveGuildData();
                        });
                }

                [SlashCommand("check", "Checks if the data was synced today")]
                public async Task SyncCheckCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    await DiscordMessageHelper.BuildCheckMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.check, helper.IsDataSynced);
                }

                [SlashCommand("excel", "Syncs the data with provided excel")]
                public async Task SyncExcelCommand(InteractionContext ctx, [Option("file", "attach excel file")]DiscordAttachment file)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.excel, () => helper.SyncExcelFile(file));
                }

                [SlashCommand("cleanup", "Empties ticketresults table and saves lifetimetickets")]
                public async Task SyncCleanupCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.cleanup, helper.CleanupStrikes);
                }
            }

            [SlashCommandGroup("get", "To get strike data")]
            public class GetContainer : ApplicationCommandModule
            {
                [SlashCommand("excel", "Returns an excel file with the synced strike data")]
                public async Task ExcelCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    try
                    {
                        FileStream file = await helper.GetExcelFile();
                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent(i18n.i18n.data.commands.tickettracker.get.excel.complete).AddFile(file);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);

                        file.Close();
                        File.Delete(file.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(i18n.i18n.data.commands.tickettracker.get.excel.fail + "\n ERROR: " + ex.Message);
                    }
                }

                [SlashCommand("message", "Returns a message that pings all the members with strikes")]
                public async Task MessageCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    try
                    {
                        string message = await helper.GetMessage();
                        DiscordInteractionResponseBuilder dcMessage = new DiscordInteractionResponseBuilder().WithContent(message);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, dcMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(i18n.i18n.data.commands.tickettracker.get.message.fail + "\n ERROR: " + ex.Message);
                    }
                }
            }

            [SlashCommandGroup("remove", "To remove strikes")]
            public class RemoveContainer() : ApplicationCommandModule
            {
                [SlashCommand("user", "Removes strikes from a user")]
                public async Task RemoveUserStrikes(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("amount", "Amount of strikes")] long amount = 1)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    string complete = i18n.i18n.data.commands.tickettracker.remove.complete;
                    string fail = i18n.i18n.data.commands.tickettracker.remove.fail;
                    string loading = i18n.i18n.data.commands.tickettracker.remove.loading;

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource);

                    try
                    {
                        DiscordWebhookBuilder loadingMessage = new DiscordWebhookBuilder().WithContent(loading);
                        await ctx.EditResponseAsync(loadingMessage);
                        string addToComplete = await helper.removeStrikes(dcMember, (int)amount);
                        DiscordWebhookBuilder completeMessage = new DiscordWebhookBuilder().WithContent(complete + "\n\n" + addToComplete);
                        await ctx.EditResponseAsync(completeMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(fail + "\n ERROR: " + ex.Message);
                    }
                }
            }

            [SlashCommandGroup("exclude", "Adds or removes members from strike-list count")]
            public class NVTContainer() : ApplicationCommandModule
            {
                [SlashCommand("add", "Exludes a member from strike-list count")]
                public async Task NVTAddCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("dayAmount", "Amount of days this member should not be counted for the strikelist")] long dayAmount = 0)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    string sMessage = i18n.i18n.Transform(i18n.i18n.data.commands.tickettracker.exclude.add.complete, dcMember);
                    if (dayAmount > 0)
                    {
                        sMessage += $" for {dayAmount} days";
                    }

                    try
                    {
                        await helper.AddMemberToNVT(dcMember, (int)dayAmount);

                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent(sMessage);
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(i18n.i18n.data.commands.tickettracker.exclude.add.fail + "\n ERROR: " + ex.Message);
                    }
                }

                [SlashCommand("remove", "Includes a member to the strike-list count")]
                public async Task NVTRemoveCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember)
                {
                    string guildId = await ClientManager.getGuildId();
                    int minimumTicketAmount = await ClientManager.minimumTickets();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimumTicketAmount, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.exclude.remove, () => helper.RemoveMemberToNVT(dcMember));
                }

                [SlashCommand("guildstrikecount", "turns off strike-list count for selected guild")]
                public async Task TurnOffTicketTrackerCommand(InteractionContext ctx)
                {
                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.exclude.guildStrikeCount, TicketTrackerSwitchCommandHelper.SwitchLaunchTicketTrackCommand);
                }
            }
        }
    }
}
