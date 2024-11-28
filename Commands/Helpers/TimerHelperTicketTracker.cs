
using DSharpPlus;
using DSharpPlus.Entities;
using System.Data;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.Commands.Helpers.promotions;
using tsom_bot.config;

namespace tsom_bot.Commands.Helpers
{
    public class TimerHelper
    {
        private readonly Timer _timer;
        private readonly int _interval;
        private readonly DiscordClient _client;
        public TimerHelper(DiscordClient client, int intervalInSec)
        {
            _client = client;
            _interval = intervalInSec * 1000;
            _timer = new Timer(async _ =>
            {
                ClientManager.time++;
                await CheckForCommand();
            },
            null,
            0,
            _interval);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            _timer.Change(0, _interval);
        }

        private async Task CheckForCommand()
        {
            DataTable result = await QueueHelper.GetQueueItemWithTime(DateTime.Now);
            if(result.Rows.Count > 0)
            {
                foreach(DataRow row in result.Rows)
                {
                    await QueueHelper.SendQueueCommand(row.Field<int>("eventId"), row.Field<string>("parameters"));
                }
            }
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

        private async Task SyncGuildSaveCommand(bool runOnLaunch)
        {
            ConfigReader configReader = new ConfigReader();
            await configReader.readConfig();

            int strikeListSendTimeHour = configReader.strikeListSendTime.hour;
            int strikeListSendTimeMinute = configReader.strikeListSendTime.minute;
            int strikeListSendTimeSecond = configReader.strikeListSendTime.second;

            DateTime now = DateTime.Now;
            DateTime syncTime = new(now.Year, now.Month, now.Day, strikeListSendTimeHour, strikeListSendTimeMinute, strikeListSendTimeSecond);

            int differenceInMin = (int)MathF.Floor((float)(syncTime - ClientManager.timerStartTime).TotalMinutes);

            if (ClientManager.time >= differenceInMin) 
            {
                await AddPromotionCommandToQueue();
            }
        }

        private async Task SyncCheckPromotion(bool runOnLaunch)
        {
            DateTime now = DateTime.Now;
            DateTime syncTime = new(now.Year, now.Month, now.Day, 20, 0, 0);

            int differenceInMin = (int)MathF.Floor((float)(syncTime - ClientManager.timerStartTime).TotalMinutes);

            if (ClientManager.time >= differenceInMin)
            {
                await this.SendCheckPromotionCommand(differenceInMin, runOnLaunch);
            }
        }

        private async Task AddPromotionCommandToQueue()
        {
            ConfigReader configReader = new ConfigReader();
            await configReader.readConfig();
        }

        public async Task SendCheckPromotionCommand(int adjustedInterval, bool runOnLaunch)
        {
            int cycleCooldown = 24 * 60 * 30;
            if (IsInCycle(cycleCooldown, adjustedInterval, runOnLaunch))
            {
                ConfigReader reader = new ConfigReader();
                await reader.readConfig();
                var channelIdSith = reader.channelIds.sith.commands_private;
                var channelIdJedi = reader.channelIds.jedi.commands_private;
                var chanSith = await _client.GetChannelAsync(channelIdSith);
                var chanJedi = await _client.GetChannelAsync(channelIdJedi);

                GuildSwitch resetGuildSwitch = ClientManager.guildSwitch;

                if (chanSith != null) 
                {
                    ClientManager.guildSwitch = GuildSwitch.TSOM;
                    await TimedPromotionHelper.SyncPromotions(_client, i18n.i18n.data.commands.promotion.sync.complete);
                }

                if(chanJedi != null)
                {
                    ClientManager.guildSwitch = GuildSwitch.TJOM;
                    await TimedPromotionHelper.SyncPromotions(_client, i18n.i18n.data.commands.promotion.sync.complete);
                }

                ClientManager.guildSwitch = resetGuildSwitch;
            }
        }
    }
}
