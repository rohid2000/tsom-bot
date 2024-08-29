using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Net.Mail;
using tsom_bot.Commands.Helpers;
using tsom_bot.Commands.Helpers.Discord;

namespace tsom_bot.Commands
{
    public class TicketTrackerCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("tickets", "Command used to view strike data")]
        public class TicketTrackerContainer : ApplicationCommandModule
        {
            [SlashCommandGroup("sync", "used to sync strike data")]
            public class SyncContainer : ApplicationCommandModule
            {
                [SlashCommand("go", "Syncs the guilds data with the database")]
                public async Task SyncCommand(InteractionContext ctx)
                {
                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.go,
                        async () =>
                        {
                            string guildId = await ClientManager.getGuildId();
                            TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                            await helper.SaveGuildData();
                        });
                }

                [SlashCommand("check", "Checks if the data was synced today")]
                public async Task SyncCheckCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    await DiscordMessageHelper.BuildCheckMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.check, helper.IsDataSynced);
                }

                [SlashCommand("excel", "Syncs the data with provided excel")]
                public async Task SyncExcelCommand(InteractionContext ctx, [Option("file", "attach excel file")]DiscordAttachment file)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.excel, () => helper.SyncExcelFile(file));
                }

                [SlashCommand("cleanup", "will cleanup the database and only save total strikes lifetime")]
                public async Task SyncCleanupCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.sync.cleanup, helper.CleanupStrikes);
                }
            }

            [SlashCommandGroup("get", "used to get strike data")]
            public class GetContainer : ApplicationCommandModule
            {
                [SlashCommand("excel", "Returns an excel file with the synced strike data")]
                public async Task ExcelCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

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
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

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

            [SlashCommandGroup("nvt", "adds or removes a member from the count of the strikelist")]
            public class NVTContainer() : ApplicationCommandModule
            {
                [SlashCommand("add", "adds a member to the not count list")]
                public async Task NVTAddCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("dayAmount", "Amount of days this member should not be counted for the strikelist")] long dayAmount = 0)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    string sMessage = i18n.i18n.Transform(i18n.i18n.data.commands.tickettracker.nvt.add.complete, dcMember);
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
                        Console.WriteLine(i18n.i18n.data.commands.tickettracker.nvt.add.fail + "\n ERROR: " + ex.Message);
                    }
                }

                [SlashCommand("remove", "removes a member from the not count list")]
                public async Task NVTRemoveCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.tickettracker.nvt.remove, () => helper.RemoveMemberToNVT(dcMember));
                }

                [SlashCommand("switch", "turns off the ticket tracker for the guild")]
                public async Task TurnOffTicketTrackerCommand(InteractionContext ctx)
                {
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    await DiscordMessageHelper.BuildMessageWithExecute(ctx, i18n.i18n.data.commands.ticketTrackerSwitch, helper.SwitchLaunchTicketTrackCommand);
                }
            }
        }
    }
}
