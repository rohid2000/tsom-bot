using System.Collections.ObjectModel;
using ClosedXML.Excel;
using tsom_bot.Models;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerCommandHelper {
        public string message;
        private int minimalTicketValue;
        private ExcelHelper? excel;

        public TicketTrackerCommandHelper(IGuild guildData, int minimalTicketValue)
        {
            try {
                this.excel = new ExcelHelper(guildData, minimalTicketValue);
            } 
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            this.minimalTicketValue = minimalTicketValue;
            this.message = this.GetMemberTicketResultList(guildData);
        }

        async public static Task<TicketTrackerCommandHelper> BuildViewModelAsync(string guildId)  
        {       
            ObservableCollection<IGuild> guildData = new();
            guildData.Add(await GuildFetcher.GetGuildById(guildId, true, new()));
            
            return new TicketTrackerCommandHelper(guildData[0], 600);
        }   

        public FileStream? GetExcelFile()
        {
            return this.excel?.GetGeneratedFile();
        }

        private string GetMemberTicketResultList(IGuild GuildData)
        {
            string ResultListString = "";
            for(int i = 0; i < GuildData.member.Length; i++)
            {
                IMember member = GuildData.member[i];
                IMemberContribution? contribution = member.GetRaidTicketContribution();
                
                if(contribution != null)
                {
                    ContributionReached contributionReached = ExcelHelper.IsTicketGoalReached(int.Parse(contribution.currentValue), this.minimalTicketValue);
                    ResultListString += $"{ member.playerName } | { ExcelHelper.ConvertContributionReachedToString(contributionReached) } \n";
                }
            }

            if(ResultListString.Length >= 2000)
            {
                ResultListString = ResultListString.Substring(0, 2000);
            }

            return ResultListString;
        }
    }

    enum ContributionReached
    {
        Yes, No, NVT
    }

    internal class ExcelHelper {
        internal readonly string fileName = "excelguilddata.xlsx";

        public ExcelHelper(IGuild guildData, int minimalTicketValue)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sample Sheet");

                for(int i = 0; i < guildData.member.Length; i++)
                {
                    IMember member = guildData.member[i];
                    IMemberContribution? contribution = member.GetRaidTicketContribution();
                    ContributionReached memberTicketGoalReached = IsTicketGoalReached(int.Parse(contribution.currentValue), minimalTicketValue);
                    if(contribution != null && (memberTicketGoalReached != ContributionReached.Yes || memberTicketGoalReached != ContributionReached.NVT))
                    {
                        worksheet.Cell("A"+(i+2)).Value = member.playerName;
                        worksheet.Cell("B"+(i+2)).Value = contribution.currentValue;
                        worksheet.Cell("C"+(i+2)).Value = ConvertContributionReachedToString(memberTicketGoalReached);
                    }
                }

                workbook.SaveAs(fileName);
            }
        }

        internal FileStream GetGeneratedFile()
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        internal static ContributionReached IsTicketGoalReached(int contributionValue, int minimalTicketValue)
        {
            if(contributionValue >= minimalTicketValue)
            {
                return ContributionReached.Yes;
            } else {
                return ContributionReached.No;
            }
        }

        internal static string ConvertContributionReachedToString(ContributionReached contributionReached)
        {
            switch(contributionReached)
            {
                case ContributionReached.No:
                    return "Bad!";
                case ContributionReached.Yes:
                    return "Good!!";
                case ContributionReached.NVT:
                    return "This player doesnt take part in this event yet";
                default:
                return "";
            }
        }
    }
}