using DSharpPlus;
using DSharpPlus.Entities;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueCommands
    {
        public static async Task SendSaveGuildData(DiscordClient client, KeyValuePair<string, string>[] parameters)
        {
            if (ClientManager.launchTicketTrackerSwitchCommandJedi || ClientManager.launchTicketTrackerSwitchCommandSith)
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();

                var channelIds = reader.channelIds;
                var minimunTicketAmounts = reader.minimumTicketAmount;

                var chanSith = await client.GetChannelAsync(channelIds.sith.strikeList);
                var chanJedi = await client.GetChannelAsync(channelIds.jedi.strikeList);

                GuildSwitch resetGuildSwitch = ClientManager.guildSwitch;

                if (chanSith != null && ClientManager.launchTicketTrackerSwitchCommandSith)
                {
                    ClientManager.guildSwitch = GuildSwitch.TSOM;
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimunTicketAmounts.ticketAmountSith, client);
                    await helper.SaveGuildData();

                    FileStream? file = await helper.GetExcelFile();

                    if (file != null)
                    {
                        await new DiscordMessageBuilder()
                            .WithContent("Synced with latest data, here is the TSOM strike data")
                            .AddFile(file)
                            .SendAsync(chanSith);

                        file.Close();
                        File.Delete(file.Name);
                    }
                }

                if (chanJedi != null && ClientManager.launchTicketTrackerSwitchCommandJedi)
                {
                    ClientManager.guildSwitch = GuildSwitch.TJOM;
                    string guildId = await ClientManager.getGuildId();
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, minimunTicketAmounts.ticketAmountJedi, client);
                    await helper.SaveGuildData();

                    FileStream? file = await helper.GetExcelFile();

                    if (file != null)
                    {
                        await new DiscordMessageBuilder()
                            .WithContent("Synced with latest data, here is the TJOM strike data")
                            .AddFile(file)
                            .SendAsync(chanJedi);

                        file.Close();
                        File.Delete(file.Name);
                    }
                }

                ClientManager.guildSwitch = resetGuildSwitch;
            }
        }
    }
}
