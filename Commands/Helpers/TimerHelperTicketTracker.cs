
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using tsom_bot.Commands.Helpers.promotions;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers
{
    public class TimerHelper
    {
        private readonly Timer _timer;
        private readonly int _interval;
        private ConfigReader configReader;
        public TimerHelper(DiscordClient client, int intervalInSec)
        {
            _interval = intervalInSec * 1000;
            _timer = new Timer(async _ =>
            {
                ClientManager.time++;
                SyncGuildSaveCommand(client);
                SendCheckPromotionCommand(client);
            },
            null,
            0,  // 4) Time that message should fire after the timer is created
            _interval); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat)
        }

        public void Stop() // 6) Example to make the timer stop running
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart() // 7) Example to restart the timer
        {
            _timer.Change(0, _interval);
        }

        private bool IsInCycle(int cycleCooldown)
        {
            return ClientManager.time % cycleCooldown == 0;
        }

        private bool IsInCycle(int cycleCooldown, int adjustedInterval)
        {
            if (ClientManager.time == adjustedInterval)
                return true;
            return (ClientManager.time - adjustedInterval) % cycleCooldown == 0;
        }

        private void SyncGuildSaveCommand(DiscordClient client)
        {
            DateTime now = DateTime.Now;
            DateTime syncTime = new(now.Year, now.Month, now.Day, 19, 28, 0); // 0, 0, 0, 19, 28, 0 preferred time

            int differenceInMin = (int)MathF.Floor((float)(syncTime - ClientManager.timerStartTime).TotalMinutes);

            if (ClientManager.time >= differenceInMin) 
            {
                SendSaveGuildData(client, differenceInMin);
            }
        }

        public async void SendCheckPromotionCommand(DiscordClient client)
        {
            int cycleCooldown = 24 * 60;
            if (IsInCycle(cycleCooldown))
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();
                var channelIdSith = reader.channelIds.sith.commands_private;
                var channelIdJedi = reader.channelIds.jedi.commands_private;
                var chanSith = await client.GetChannelAsync(channelIdSith);
                var chanJedi = await client.GetChannelAsync(channelIdJedi);

                if(chanSith != null) 
                {
                    ClientManager.guildSwitch = GuildSwitch.Sith;
                    string guildId = await ClientManager.getGuildId();
                    await TimedPromotionHelper.SyncPromotions(client, i18n.i18n.data.commands.promotion.sync.complete);

                    await new DiscordMessageBuilder()
                    .WithContent("TSOM promotion data has been synced")
                    .SendAsync(chanSith);
                }

                if(chanJedi != null)
                {
                    ClientManager.guildSwitch = GuildSwitch.Jedi;
                    string guildId = await ClientManager.getGuildId();
                    await TimedPromotionHelper.SyncPromotions(client, i18n.i18n.data.commands.promotion.sync.complete);

                    await new DiscordMessageBuilder()
                    .WithContent("TJOM promotion data has been synced")
                    .SendAsync(chanJedi);
                }
            }
        }

        public async void SendSaveGuildData(DiscordClient client, int adjustedInterval)
        {
            int commandCycleCooldown = 24 * 60; //24h cooldown if bot sends interval every 60s
            if (IsInCycle(commandCycleCooldown, adjustedInterval))
            {
                if (ClientManager.launchTicketTrackerSwitchCommandJedi || ClientManager.launchTicketTrackerSwitchCommandSith)
                {
                    ConfigReader reader = new ConfigReader();
                    await reader.readConfig();
                    var ticketsSith = reader.minimumRaidTicketAmount.ticketAmountSith;
                    var ticketsJedi = reader.minimumRaidTicketAmount.ticketAmountJedi;
                    var channelIdSith = reader.channelIds.sith.strikeList;
                    var channelIdJedi = reader.channelIds.jedi.strikeList;
                    var chanSith = await client.GetChannelAsync(channelIdSith);
                    var chanJedi = await client.GetChannelAsync(channelIdJedi);

                    if (chanSith != null && ClientManager.launchTicketTrackerSwitchCommandSith)
                    {
                        ClientManager.guildSwitch = GuildSwitch.Sith;
                        string guildId = await ClientManager.getGuildId();
                        TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, ticketsSith, client);
                        await helper.SaveGuildData();

                        FileStream file = await helper.GetExcelFile();

                        await new DiscordMessageBuilder()
                            .WithContent("Synced with latest data, here is the TSOM strike data")
                            .AddFile(file)
                            .SendAsync(chanSith);

                        file.Close();
                        File.Delete(file.Name);
                    }

                    if (chanJedi != null && ClientManager.launchTicketTrackerSwitchCommandJedi)
                    {
                        ClientManager.guildSwitch = GuildSwitch.Jedi;
                        string guildId = await ClientManager.getGuildId();
                        TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId, ticketsJedi, client);
                        await helper.SaveGuildData();

                        FileStream file = await helper.GetExcelFile();

                        await new DiscordMessageBuilder()
                            .WithContent("Synced with latest data, here is the TJOM strike data")
                            .AddFile(file)
                            .SendAsync(chanJedi);

                        file.Close();
                        File.Delete(file.Name);
                    }
                }
            }
        }
    }
}
