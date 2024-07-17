using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using tsom_bot.Commands.Helpers;

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
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    try
                    {
                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent("sync data with latest");
                        await helper.SaveGuildData();
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                [SlashCommand("check", "Checks if the data was synced today")]
                public async Task SyncCheckCommand(InteractionContext ctx)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    try
                    {
                        string content;
                        if(await helper.IsDataSynced())
                        {
                            content = "data was already synced today";
                        }
                        else
                        {
                            content = "data was not synced today";
                        }
                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent(content);
                        
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                [SlashCommand("excel", "Syncs the data with provided excel")]
                public async Task SyncExcelCommand(InteractionContext ctx, [Option("file", "attach excel file")]DiscordAttachment file)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    try
                    {
                        await helper.SyncExcelFile(file);

                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent("Synced with excel data");

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            [SlashCommandGroup("get", "used to get strike data")]
            public class GetContainer : ApplicationCommandModule
            {
                [SlashCommand("excel", "Returns an excel file with the synced strike data")]
                public async Task ExcelCommand(InteractionContext ctx)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    try
                    {
                        FileStream file = await helper.GetExcelFile();
                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent("this is your file").AddFile(file);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);

                        file.Close();
                        File.Delete(file.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                [SlashCommand("message", "Returns a message that pings all the members with strikes")]
                public async Task MessageCommand(InteractionContext ctx)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    try
                    {
                        string message = await helper.GetMessage();
                        DiscordInteractionResponseBuilder dcMessage = new DiscordInteractionResponseBuilder().WithContent(message);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, dcMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            [SlashCommandGroup("nvt", "adds or removes a member from the count of the strikelist")]
            public class NVTContainer() : ApplicationCommandModule
            {
                [SlashCommand("add", "adds a member to the not count list")]
                public async Task NVTAddCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember, [Option("dayAmount", "Amount of days this member should not be counted for the strikelist")] long dayAmount = 0)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    string sMessage = $"Added {dcMember.Mention} to the not count list";
                    if(dayAmount > 0) 
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
                        Console.WriteLine(ex.Message);
                    }
                }

                [SlashCommand("remove", "removes a member from the not count list")]
                public async Task NVTRemoveCommand(InteractionContext ctx, [Option("user", "player")] DiscordUser dcMember)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, 400, ctx.Client);

                    try
                    {
                        await helper.RemoveMemberToNVT(dcMember);
                        DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder().WithContent($"Removed {dcMember.Mention} from the not count list");
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
