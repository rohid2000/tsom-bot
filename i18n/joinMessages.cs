using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsom_bot.i18n
{
    public static class joinMessages
    {
        public static joinMessagesStructure data;

        public static async void load()
        {
            using (StreamReader reader = new StreamReader("joinMessages.json"))
            {
                string json = await reader.ReadToEndAsync();
                joinMessagesStructure data = JsonConvert.DeserializeObject<joinMessagesStructure>(json);

                if (data != null)
                {
                    joinMessages.data = data;
                }
            }
        }
    }

    public class joinMessagesStructure
    {
        public joinMessagesRoleAssignMessagesStructure roleAssignMessages;
    }

    public class joinMessagesRoleAssignMessagesStructure
    {
        public string welcome;
        public string namesNotLinked;
        public joinMessagesWelcomeMessageGeneralChat welcomeMessageGeneralChat;
    }

    public class joinMessagesWelcomeMessageGeneralChat
    {
        public joinMessagesWelcomeMessageGeneralChatHeaderAndFooter jedi { get; set; }
        public joinMessagesWelcomeMessageGeneralChatHeaderAndFooter sith { get; set; }
        public string footer;
    }

    public class joinMessagesWelcomeMessageGeneralChatHeaderAndFooter
    {
        public string[] header { get; set; }

        public string GetRandomHeader()
        {
            Random rng = new();
            return this.header[rng.Next(0, this.header.Length - 1)];
        }
    }
}
