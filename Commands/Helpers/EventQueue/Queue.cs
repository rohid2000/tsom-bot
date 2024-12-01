using System.Data;
using System.Text;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueHelper
    {
        public async static Task AddMessageToQueue(string message, ulong channelid, DateTime sendDate)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate) VALUES (1, 'message={message},channelid={channelid}', '{sendDate.ToString("yyyy-MM-dd HH:mm")}')";
            await Database.SendSqlSave(sql);
        }

        public async static Task AddTwDefenseToQueue(ulong channelid, DateTime sendDate)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate) VALUES (2, 'channelid={channelid}', '{sendDate.ToString("yyyy-MM-dd HH:mm")}')";
            await Database.SendSqlSave(sql);
        }

        public async static Task AddTicketCheckToQueue(ulong channelid, string guildId, DateTime sendDate)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate) VALUES (3, 'channelid={channelid},guildid={guildId}', '{sendDate.ToString("yyyy-MM-dd HH:mm")}')";
            await Database.SendSqlSave(sql);
        }
        public async static Task AddPromotionCheckToQueue(DateTime sendDate)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate) VALUES (4, '', '{sendDate.ToString("yyyy-MM-dd HH:mm")}')";
            await Database.SendSqlSave(sql);
        }

        public async static Task<DataTable> GetQueueItemToTime(DateTime time)
        {
            return await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE sendDate <= '{time.ToString("yyyy-MM-dd HH:mm")}'");
        }

        public async static Task<DataTable> GetQueueItemWithTime(DateTime time)
        {
            return await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE sendDate = '{ time.ToString("yyyy-MM-dd HH:mm") }'");
        }
        public async static Task<DataTable> RemoveQueuedItem(DataRow row)
        {
            return await Database.SendSqlPull($"DELETE FROM queuedevents WHERE sendDate = '{row.Field<DateTime>("sendDate").ToString("yyyy-MM-dd HH:mm")}' AND parameters = '{row.Field<string>("parameters")}'");
        }

        public static string QueueItemToString(DataRow row)
        {
            StringBuilder sb = new StringBuilder();
            int eventId = row.Field<int>("eventid");
            DateTime time = row.Field<DateTime>("sendDate");
            string timeString = "";
            if(DateTime.Now.Day == time.Day && DateTime.Now.Month == time.Month)
            {
                double minutes = (time - DateTime.Now).TotalMinutes;
                int hours = (int)Math.Floor(minutes / 60);
                minutes -= hours * 60;
                timeString = $" {hours}h <{Math.Ceiling(minutes)}m left until send";
            }
            else
            {
                timeString = $"for {time.ToString("yyyy-MM-dd HH:mm")}";
            }
            if(eventId == 1)
            {
                sb.AppendLine("Message queued" + timeString);
            }
            else if(eventId == 2)
            {
                sb.AppendLine($"Defense Configure Ping" + timeString);
            }
            else if(eventId == 3)
            {
                sb.AppendLine($"Ticket check queued" + timeString);
            }
            else if(eventId == 4)
            {
                sb.AppendLine($"Promotion check queued" + timeString);
            }

            return sb.ToString();
        }

        public async static Task SendQueueCommand(DataRow row)
        {
            int commandIndex = row.Field<int>("eventid");
            switch (commandIndex)
            {
                case 1:
                    await QueueCommands.sendMessage(GetParameters(row));
                    break;
                case 2:
                    await QueueCommands.defenseFallback(GetParameters(row));
                    break;
                case 3:
                    await QueueCommands.checkTickets(GetParameters(row));
                    break;
                case 4:
                    await QueueCommands.checkPromotions();
                    break;
            }

            await RemoveQueuedItem(row);
        }

        private static Dictionary<string, string> GetParameters(DataRow row)
        {
            string param = row.Field<string>("parameters");
            int commandIndex = row.Field<int>("eventid");
            string[] parameters = param.Split(",");
            Dictionary<string, string> paramsDictionary = new Dictionary<string, string>();
            foreach (string text in parameters)
            {
                string[] textSplit = text.Split("=");
                paramsDictionary.Add(textSplit[0], textSplit[1]);
            }

            return paramsDictionary;
        }
    }
}
