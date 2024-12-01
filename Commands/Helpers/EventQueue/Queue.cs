using System.Data;
using System.Linq;
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
            string param = row.Field<string>("parameters");
            int commandIndex = row.Field<int>("eventid");
            string[] parameters = param.Split(",");
            Dictionary<string, string> paramsDictionary = new Dictionary<string, string>();
            foreach (string text in parameters)
            {
                string[] textSplit = text.Split("=");
                paramsDictionary.Add(textSplit[0], textSplit[1]);
            }

            switch (commandIndex)
            {
                case 1:
                    await QueueCommands.sendMessage(paramsDictionary);
                    break;
                case 2:
                    await QueueCommands.defenseFallback(paramsDictionary);
                    break;
            }

            await RemoveQueuedItem(row);
        }
    }
}
