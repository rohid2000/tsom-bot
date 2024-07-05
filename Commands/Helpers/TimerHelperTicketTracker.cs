
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
                SendTicktTrackerCommand(client);
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

        public async void SendCheckPromotionCommand(DiscordClient client)
        {
            int commandCycleCooldown = 24 * 60;
            if (ClientManager.time == commandCycleCooldown)
            {
                string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                ConfigReader reader = new();
                await reader.readConfig();
                foreach (KeyValuePair<ulong, DiscordMember> member in client.Guilds[1247909896331198575].Members)
                {
                    DiscordMember dcMember = member.Value;
                    int totalDays = (int)MathF.Floor((float)(DateTime.Now - dcMember.JoinedAt).TotalDays);
                    RolePromotionHelper helper = new RolePromotionHelper();

                    if (totalDays >= reader.rolePromotionDays.sithlord)
                    {
                        helper.GiveRole(client, Role.SithLord, dcMember);
                    }
                    else if(totalDays >= reader.rolePromotionDays.mandalorian)
                    {
                        helper.GiveRole(client, Role.Mandalorian, dcMember);
                    }
                    else if (totalDays >= reader.rolePromotionDays.apprentice)
                    {
                        helper.GiveRole(client, Role.Apprentice, dcMember);
                    }
                    else if (totalDays >= reader.rolePromotionDays.acolyte)
                    {
                        helper.GiveRole(client, Role.Acolyte, dcMember);
                    }
                }
            }
        }

        public async void SendTicktTrackerCommand(DiscordClient client)
        {
            int commandCycleCooldown = 60 * 24; //24h cooldown if bot sends interval every 60s
            if (ClientManager.time == commandCycleCooldown)
            {
                var channelId = configReader.channelIds.tsomBotTesting;
                var chan = await client.GetChannelAsync(channelId);

                if (chan != null)
                {
                    string guildId = "l943tTO8QQ-_IwWHfwyJuQ";
                    TicketTrackerCommandHelper helper = await TicketTrackerCommandHelper.BuildViewModelAsync(guildId);
                    try
                    {
                        await new DiscordMessageBuilder()
                        .WithContent("this is your file")
                        .AddFile(helper.GetExcelFile())
                        .SendAsync(chan);
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
