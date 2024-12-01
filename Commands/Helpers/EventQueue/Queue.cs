using System.Data;
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

        public async static Task<DataTable> GetQueueItemWithTime(DateTime time)
        {
            return await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE sendDate = '{ time.ToString("yyyy-MM-dd HH:mm") }'");
        }
        public async static Task<DataTable> RemoveQueuedItem(DataRow row)
        {
            return await Database.SendSqlPull($"DELETE FROM queuedevents WHERE sendDate = '{row.Field<DateTime>("sendDate").ToString("yyyy-MM-dd HH:mm")}' AND parameters = '{row.Field<string>("parameters")}'");
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
