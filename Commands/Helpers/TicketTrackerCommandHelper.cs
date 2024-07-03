using System.Collections.ObjectModel;
using System.Data;
using ClosedXML.Excel;
using tsom_bot.Fetcher.database;
using tsom_bot.Models;
using tsom_bot.Models.Member;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerCommandHelper {
        public string message;
        private int minimalTicketValue;
        private ExcelHelper? excel;

        public TicketTrackerCommandHelper(IGuild guildData, int minimalTicketValue)
        {
            TicketTrackerSaveCommandHelper saveHelper = new TicketTrackerSaveCommandHelper(guildData, minimalTicketValue);
            this.excel = new ExcelHelper(guildData, minimalTicketValue);
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
                int heightIndex = 5;

                worksheet.ColumnWidth = 16;

                worksheet.Cell("A1").Value = "Strike reason";
                worksheet.Cell("B1").Value = "Missing 400 tickets";
                worksheet.Cell("C1").Value = "Total strikes";

                worksheet.Cell("A3").Value = "Member name";

                int memberIndex = 0;
                DataTable dataToday = Database.SendSqlPull($"SELECT * FROM TicketResults WHERE date = {DateTime.Now.ToString("yyyy-MM-dd")}");
                DataTable dataYesterday = Database.SendSqlPull($"SELECT * FROM TicketResults WHERE date = {DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}"); ;

                foreach (IMemberTicketResult memberResult in dataToday.Rows)
                {
                    int ticketAmount = memberResult.ticketAmount + dataYesterday.Rows[memberIndex].Field<int>("ticketAmount");

                    worksheet.Cell("A" + (memberIndex + heightIndex)).Value = memberResult.playerName;
                    worksheet.Cell("B" + (memberIndex + heightIndex)).Value = ticketAmount >= 1 ? "X" : "";

                    if(memberResult.ticketAmount >= 1)
                    {
                        worksheet.Cell("B" + (memberIndex + heightIndex)).Style.Fill.BackgroundColor = XLColor.Red;
                    }

                    var strikesCell = worksheet.Cell("C"+(memberIndex + heightIndex));

                    strikesCell.Value = ticketAmount;

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
                    memberIndex++;
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