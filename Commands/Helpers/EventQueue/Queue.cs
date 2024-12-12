using System.Data;
using System.Text;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueHelper
    {
        public async static Task AddMessageToQueue(string message, ulong channelid, DateTime sendDate, string description = null)
        {
            string formattedMessage = message.Replace(",", "|||");
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate, description) VALUES (1, 'message={formattedMessage},channelid={channelid}', '{sendDate.ToString("yyyy-MM-dd HH:mm")}', {FormatDescription(description)})";
            await Database.SendSqlSave(sql);
        }

        public async static Task AddTwDefenseToQueue(ulong channelid, DateTime sendDate, string description = null)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate, description) VALUES (2, 'channelid={channelid},state=NORMAL', '{sendDate.ToString("yyyy-MM-dd HH:mm")}', {FormatDescription(description)})";
            await Database.SendSqlSave(sql);
        }
        public async static Task AddFinalTwDefenseToQueue(ulong channelid, DateTime sendDate, string description = null)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate, description) VALUES (2, 'channelid={channelid},state=FINAL', '{sendDate.ToString("yyyy-MM-dd HH:mm")}', {FormatDescription(description)})";
            await Database.SendSqlSave(sql);
        }

        public async static Task AddTicketCheckToQueue(ulong channelid, string guildId, DateTime sendDate, string description = null)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate, description) VALUES (3, 'channelid={channelid},guildid={guildId}', '{sendDate.ToString("yyyy-MM-dd HH:mm")}', {FormatDescription(description)})";
            await Database.SendSqlSave(sql);
        }
        public async static Task AddPromotionCheckToQueue(DateTime sendDate, string description = null)
        {
            string sql = $"INSERT INTO queuedevents (eventid, parameters, sendDate, description) VALUES (4, '', '{sendDate.ToString("yyyy-MM-dd HH:mm")}', {FormatDescription(description)})";
            await Database.SendSqlSave(sql);
        }

        public async static Task<DataTable> GetQueueItemFromToTime(DateTime fromTime, DateTime toTime)
        {
            return await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE sendDate BETWEEN '{fromTime.ToString("yyyy-MM-dd HH:mm")}' AND '{toTime.ToString("yyyy-MM-dd HH:mm")}' ORDER BY sendDate DESC");
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
            DateTime time = row.Field<DateTime>("sendDate");

            string timeString = "";
            if (DateTime.Now.Day == time.Day && DateTime.Now.Month == time.Month)
            {
                double minutes = (time - DateTime.Now).TotalMinutes;
                int hours = (int)Math.Floor(minutes / 60);
                minutes -= hours * 60;
                timeString = $"*{hours}h <{Math.Ceiling(minutes)}m* left until send";
            }
            else
            {
                timeString = $"*{time.ToString("yyyy-MM-dd HH:mm")}*";
            }

            string? description = row.Field<string>("Description");
            if (description != null)
            {
                sb.AppendLine(description + " | " + timeString);
            }
            else
            {
                int eventId = row.Field<int>("eventid");
                sb.AppendLine(GetEventStringById(eventId) + " | " + timeString);
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
                    await QueueCommands.defenseReminder(GetParameters(row));
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

        public static Dictionary<string, string> GetParameters(DataRow row)
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
        private static string FormatDescription(string? description)
        {
            if (description != null)
            {
                return "'" + description?.Replace(",", "|||") + "'";
            }
            else
            {
                return "NULL";
            }
        }

        public static string GetEventStringById(int eventId)
        {
            if (eventId == 1)
            {
                return "Message";
            }
            else if (eventId == 2)
            {
                return "Defense Configure Ping";
            }
            else if (eventId == 3)
            {
                return "Ticket Check";
            }
            else if (eventId == 4)
            {
                return "Promotion Check";
            }
            return "NOT INMPLEMENTED";
        }
    }
}
