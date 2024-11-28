using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;
using tsom_bot.Fetcher.database;

namespace tsom_bot.Commands.Helpers.EventQueue
{
    public static class QueueHelper
    {
        public async static Task AddMessageToQueue(string message, ulong channelid, DateTime sendDate)
        {
            await Database.SendSqlSave($"INSERT INTO queuedevents (eventid, parameters, sendDate) VALUES (1, 'message={message},channelid={channelid}', '{sendDate.ToString("yyyy-MM-dd HH:mm:ss")}'");
        }

        public async static Task<DataTable> GetQueueItemWithTime(DateTime time)
        {
            return await Database.SendSqlPull($"SELECT * FROM queuedevents WHERE sendDate = '{ time.ToString("yyyy-MM-dd HH:mm:ss") }'");
        }

        public async static Task SendQueueCommand(int commandIndex, string param)
        {
            string[] parameters = param.Split(",");
            KeyValuePair<string, string>[] paramKVP = [];
            foreach (string text in parameters)
            {
                string[] textSplit = text.Split("=");
                paramKVP.Append(new KeyValuePair<string, string>(textSplit[0], textSplit[1]));
            }

            switch (commandIndex)
            {
                case 0:
                    await QueueCommands.SendSaveGuildData(ClientManager.client, paramKVP);
                    break;
            }
        }
    }
}
