
using DSharpPlus;
using DSharpPlus.Entities;
using tsom_bot.Commands.Helpers.promotions;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers
{
    public class TimerHelper
    {
        private readonly Timer _timer;
        private readonly int _interval;
        public TimerHelper(DiscordClient client, int intervalInSec)
        {
            _interval = intervalInSec * 1000;
            _timer = new Timer(async _ =>
            {
                ClientManager.time++;
                await SyncGuildSaveCommand(client, true);
                await SyncCheckPromotion(client, false);
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

        private bool IsInCycle(int cycleCooldown, int adjustedInterval, bool runOnLaunch)
        {
            if (ClientManager.time == adjustedInterval && runOnLaunch)
                return true;
            return (ClientManager.time - adjustedInterval) % cycleCooldown == 0;
        }

        private async Task SyncGuildSaveCommand(DiscordClient client, bool runOnLaunch)
        {
            ConfigReader configReader = new ConfigReader();
            DateTime strikeListSendTime = configReader.strikeListSendTime.sendTime;
            DateTime now = DateTime.Now;
            DateTime syncTime = new(now.Year, now.Month, now.Day, strikeListSendTime);

            int differenceInMin = (int)MathF.Floor((float)(syncTime - ClientManager.timerStartTime).TotalMinutes);

            if (ClientManager.time >= differenceInMin) 
            {
                await SendSaveGuildData(client, differenceInMin, runOnLaunch);
            }
        }

        private async Task SyncCheckPromotion(DiscordClient client, bool runOnLaunch)
        {
            DateTime now = DateTime.Now;
            DateTime syncTime = new(now.Year, now.Month, now.Day, 20, 0, 0);

            int differenceInMin = (int)MathF.Floor((float)(syncTime - ClientManager.timerStartTime).TotalMinutes);

            if (ClientManager.time >= differenceInMin)
            {
                await this.SendCheckPromotionCommand(client, differenceInMin, runOnLaunch);
            }
        }

        public async Task SendCheckPromotionCommand(DiscordClient client, int adjustedInterval, bool runOnLaunch)
        {
            int cycleCooldown = 24 * 60 * 30;
            if (IsInCycle(cycleCooldown, adjustedInterval, runOnLaunch))
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();
                var channelIdSith = reader.channelIds.sith.commands_private;
                var channelIdJedi = reader.channelIds.jedi.commands_private;
                var chanSith = await client.GetChannelAsync(channelIdSith);
                var chanJedi = await client.GetChannelAsync(channelIdJedi);

                GuildSwitch resetGuildSwitch = ClientManager.guildSwitch;

                if (chanSith != null) 
                {
                    ClientManager.guildSwitch = GuildSwitch.TSOM;
                    await TimedPromotionHelper.SyncPromotions(client, i18n.i18n.data.commands.promotion.sync.complete);
                }

                if(chanJedi != null)
                {
                    ClientManager.guildSwitch = GuildSwitch.TJOM;
                    await TimedPromotionHelper.SyncPromotions(client, i18n.i18n.data.commands.promotion.sync.complete);
                }

                ClientManager.guildSwitch = resetGuildSwitch;
            }
        }

        public async Task SendSaveGuildData(DiscordClient client, int adjustedInterval, bool runOnLaunch)
        {
            int commandCycleCooldown = 24 * 60; //24h cooldown if bot sends interval every 60s
            if (IsInCycle(commandCycleCooldown, adjustedInterval, runOnLaunch))
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

                        if(file != null)
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
}
