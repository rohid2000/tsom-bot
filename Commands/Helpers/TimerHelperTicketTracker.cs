using DSharpPlus;
using System.Data;
using tsom_bot.Commands.Helpers.EventQueue;
using tsom_bot.config;
using tsom_bot.Fetcher.database;

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
                await removeAllCheckCommands();
                await SetFirstTicketCheckCommandInQueue();
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
                    await QueueHelper.SendQueueCommand(row);
                }
            }
        }

        public async Task removeAllCheckCommands()
        {
            DataTable resultTicket = await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE eventId = 3");
            DataTable resultPromotion = await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE eventId = 4");

            foreach(DataRow row in resultTicket.Rows)
            {
                await QueueHelper.RemoveQueuedItem(row);
            }
            if(resultPromotion.Rows.Count > 0)
            {
                foreach (DataRow row in resultPromotion.Rows)
                {
                    DateTime sendTime = row.Field<DateTime>("sendDate");
                    if (sendTime < DateTime.Now)
                    {
                        await QueueHelper.RemoveQueuedItem(row);
                        await SetFirstPromotionCheckCommandInQueue();
                    }
                }
            }
            else
            {
                await SetFirstPromotionCheckCommandInQueue();
            }
        }

        public async Task SetFirstTicketCheckCommandInQueue()
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            DateTime now = DateTime.Now;
            DateTime queueTime = new DateTime(now.Year, now.Month, now.Day, reader.strikeListSendTime.hour, reader.strikeListSendTime.minute, 0);

            if(queueTime > now)
            {
                queueTime.AddDays(1);
            }

            ulong channelId;
            string guildId;
            if (ClientManager.launchTicketTrackerSwitchCommandJedi)
            {
                channelId = reader.channelIds.test;
                guildId = await ClientManager.getGuildId(GuildSwitch.TJOM);
            }
            else
            {
                channelId = reader.channelIds.test;
                guildId = await ClientManager.getGuildId(GuildSwitch.TSOM);
            }

            await QueueHelper.AddTicketCheckToQueue(channelId, guildId, queueTime);
        }

        public async Task SetFirstPromotionCheckCommandInQueue(bool runOnLaunch = false)
        {
            ConfigReader reader = new ConfigReader();
            await reader.readConfig();

            DateTime now = DateTime.Now;
            DateTime queueTime = new DateTime(now.Year, now.Month, now.Day, 20, 0, 0);

            if (!runOnLaunch)
            {
                await QueueHelper.AddPromotionCheckToQueue(queueTime.AddMonths(1));
            }
            else
            {
                await QueueHelper.AddPromotionCheckToQueue(queueTime);
            }
        }
    }
}
