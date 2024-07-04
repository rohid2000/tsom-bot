
using DSharpPlus;
using DSharpPlus.Entities;

namespace tsom_bot.Commands.Helpers
{
    public class TimerHelper
    {
        private readonly Timer _timer;
        private readonly int _interval;
        public TimerHelper(DiscordClient client, int interval)
        {
            _interval = interval;
            _timer = new Timer(async _ =>
            {
                ClientManager.time++;
                SendTicktTrackerCommand(client);
            },
            null,
            interval,  // 4) Time that message should fire after the timer is created
            1000000); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat)
        }

        public void Stop() // 6) Example to make the timer stop running
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart() // 7) Example to restart the timer
        {
            _timer.Change(_interval, 1000000);
        }

        public async void SendTicktTrackerCommand(DiscordClient client)
        {
            if (ClientManager.time == 2)
            {
                var chan = await client.GetChannelAsync(1251847453947203625);
                await client.GetChannelAsync(1251847453947203625);

                if (chan != null)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId);

                    await new DiscordMessageBuilder()
                    .WithContent("this is your file")
                    .AddFile(helper.GetExcelFile())
                    .SendAsync(chan);
                }

            }
        }
    }
}
