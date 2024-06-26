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
            try 
            {
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
            
            return new TicketTrackerCommandHelper(guildData[0], 400);
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
            }

            if(ResultListString.Length >= 2000)
            {
                ResultListString = ResultListString.Substring(0, 2000);
            }

            return ResultListString;
        }
    }

    internal class ExcelHelper {
        internal readonly string fileName = "strike-list.xlsx";

        public ExcelHelper(IGuild guildData, int minimalTicketValue)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Missed tickets only");
                var strikes = 0;
                int heightIndex = 5;

                worksheet.ColumnWidth = 16;

                worksheet.Cell("A1").Value = "Strike reason";
                worksheet.Cell("B1").Value = "Missing 400 tickets";
                worksheet.Cell("C1").Value = "Total strikes";

                worksheet.Cell("A3").Value = "Member name";

                IMember[] members = guildData.GetNoReachedTicketMembers(minimalTicketValue);

                for (int i = 0; i < members.Length; i++)
                {
                    IMember member = members[i];
                    IMemberContribution? contribution = member.GetRaidTicketContribution();
                    ContributionReached memberTicketGoalReached = member.IsTicketGoalReached(minimalTicketValue);
                    if(contribution != null && memberTicketGoalReached != ContributionReached.Yes && memberTicketGoalReached != ContributionReached.NVT)
                    {
                        worksheet.Cell("A"+(i+heightIndex)).Value = member.playerName;
                        worksheet.Cell("B"+(i+heightIndex)).Value = contribution.currentValue;

                        var strikesCell = worksheet.Cell("C"+(i+heightIndex));

                        strikesCell.Value = strikes + 1;

                        if (strikesCell.GetValue<int>() == 1)
                        {
                            strikesCell.Style.Fill.BackgroundColor = XLColor.Gray;
                        }
                        else if (strikesCell.GetValue<int>() == 2)
                        {
                            strikesCell.Style.Fill.BackgroundColor = XLColor.Orange;
                        }
                        else if (strikesCell.GetValue<int>() == 3)
                        {
                            strikesCell.Style.Fill.BackgroundColor = XLColor.Red;
                        }
                    }
                }
                try
                {
                    workbook.SaveAs(fileName);
                }
                catch (Exception ex)
                {
                  Console.WriteLine(ex.Message);
                }
            }
        }

        internal FileStream GetGeneratedFile()
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}