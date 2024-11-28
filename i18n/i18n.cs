using DSharpPlus.Entities;
using Newtonsoft.Json;
namespace tsom_bot.i18n
{
    public static class i18n
    {
        public static i18nStructure data;
        public static async void load()
        {
            using (StreamReader reader = new StreamReader("i18n.json"))
            {
                string json = await reader.ReadToEndAsync();
                i18nStructure data = JsonConvert.DeserializeObject<i18nStructure>(json);

                if(data != null)
                {
                    i18n.data = data;
                }
            }
        }

        public static string TransformDcUser(string tranformableString, DiscordUser dcuser)
        {
            return tranformableString.Replace("&&p", dcuser.Mention);
        }

        public static string TransformParams(string transformableString, Dictionary<string, string> parameters)
        {
            foreach (var param in parameters)
            {
                string test = $"({param.Key})";
                transformableString = transformableString.Replace(test, param.Value);
            }

            return transformableString;
        }
    }
    public class i18nStructure
    {
        public i18nStructureCommands commands { get; set; }
    }

    public class i18nStructureCommands
    {
        public i18nStructureTicketTrackerCommand tickettracker { get; set; }
        public i18nStructurePromotionCommand promotion { get; set; }
        public i18nStructureSyncCommand sync { get; set; }
    }

    public class i18nStructureSyncCommand
    {
        public i18nStructureSyncCommandName name { get; set; }
        public i18nStructureSyncCommandTest test {  get; set; }
        public i18nBasicMessages nolist { get; set; }
        public i18nBasicMessages all { get; set; }
        public i18nBasicMessages remove { get; set; }
    }

    public class i18nStructureSyncCommandName : i18nBasicMessages
    {
        public string already_linked { get; set; }
    }
    public class i18nStructureSyncCommandTest : i18nBasicMessages
    {
        public string no_link_self { get; set; }
        public string no_link_name { get; set; }
    }

    public class i18nStructurePromotionCommand
    {
        public i18nStructurePromotionCommandSync sync { get; set; }
        public i18nBasicMessages override_M { get; set; }
    }

    public class i18nStructurePromotionCommandSync: i18nOnlyFailMessages
    {
        public i18nStructurePromotionCommandSyncComplete complete { get; set; }
    }

    public class i18nStructurePromotionCommandSyncComplete
    {
        public i18nStructurePromotionCommandSyncCompleteHeaderAndFooter jedi { get; set; }
        public i18nStructurePromotionCommandSyncCompleteHeaderAndFooter sith { get; set; }
    }

    public class i18nStructurePromotionCommandSyncCompleteHeaderAndFooter
    {
        public string[] header { get; set; }
        public string[] footer { get; set; }

        public string GetRandomHeader()
        {
            Random rng = new();
            return header[rng.Next(0, header.Length - 1)];
        }

        public string GetRandomFooter()
        {
            Random rng = new();
            return footer[rng.Next(0, footer.Length - 1)];
        }
    }

    public class i18nStructureTicketTrackerCommand
    {
        public i18nStructureTicketTrackerCommandSync sync { get; set; }
        public i18nStructureTicketTrackerCommandGet get { get; set; }
        public i18nStructureTicketTrackerCommandNVT exclude { get; set; }
        public i18nBasicMessages remove { get; set; }
    }

    public class i18nStructureTicketTrackerCommandSync
    {
        public i18nBasicMessages go { get; set; }
        public i18nStructureCheckComplete check { get; set; }
        public i18nBasicMessages excel { get; set; }
        public i18nBasicMessages cleanup { get; set; }
    }

    public class i18nStructureTicketTrackerCommandGet
    {
        public i18nBasicMessages excel { get; set; }
        public i18nOnlyFailMessages message { get; set; }
    }

    public class i18nStructureTicketTrackerCommandNVT
    {
        public i18nBasicMessages add { get; set; }
        public i18nBasicMessages remove { get; set; }
        public i18nBasicMessages guildStrikeCount { get; set; }
    }

    public class i18nBasicMessages
    {
        public string complete { get; set; }
        public string fail { get; set; }
        public string loading {  get; set; }
    }

    public class i18nOnlyFailMessages
    {
        public string fail { get; set; }
        public string loading { get; set; }
    }

    public class i18nStructureCheckComplete
    {
        public i18nCompleteTrueFalseMessages complete { get; set; }
        public string loading { get; set; }
    }

    public class i18nCompleteTrueFalseMessages
    {
        public string true_message { get; set; }
        public string false_message { get; set; }

        public string getMessage(bool result)
        {
            return result ? this.true_message : this.false_message;
        }
    }
}
