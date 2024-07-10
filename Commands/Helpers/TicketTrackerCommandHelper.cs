using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using tsom_bot.Fetcher.database;
using tsom_bot.Models;
using tsom_bot.Models.Member;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerCommandHelper {
        public string message;
        private ExcelHelper? excel;

        async public static Task<TicketTrackerCommandHelper> BuildViewModelAsync(string guildId, int minimalTicketValue)  
        {       
            ObservableCollection<IGuild> guildData = new();
            guildData.Add(await GuildFetcher.GetGuildById(guildId, true, new()));
            TicketTrackerSaveCommandHelper saveHelper = new();
            await saveHelper.SaveTicketTrackerResultsInDatabase(guildData[0].GetTicketResults(minimalTicketValue));
            DataTable dataToday = await Database.SendSqlPull($"SELECT * FROM TicketResults WHERE date '{DateTime.Now.ToString("yyyy-MM-dd")}'");

            TicketTrackerCommandHelper helper =  new TicketTrackerCommandHelper();
            helper.excel = new ExcelHelper();
            await helper.excel.BuildExcel(dataToday);

            helper.message = await helper.GetMemberTicketResultList(guildData[0], minimalTicketValue, dataToday);
            return helper;
        }   

        public FileStream? GetExcelFile()
        {
<<<<<<< Updated upstream
            return this.excel?.GetGeneratedFile();
=======
            ExcelHelper excel = new();
            DataTable dataToday = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE date = '{DateTime.Now.ToString("yyyy-MM-dd")}';");
            await excel.BuildExcel(dataToday);

            return excel.GetGeneratedFile();
>>>>>>> Stashed changes
        }

        private async Task<string> GetMemberTicketResultList(IGuild GuildData, int minimalTicketValue, DataTable dataToday)
        {
            // pull data from database
            string resultString = "";
            int memberIndex = 0;

            for (int i = 0; i < dataToday.Rows.Count; i++)
            {
                IMemberTicketResult memberResult = new IMemberTicketResult()
                {
                    playerName = dataToday.Rows[i].Field<string>("playerName"),
                    missingTickets = dataToday.Rows[i].Field<sbyte>("missingTickets") == 1,
                    RaidAttempts = dataToday.Rows[i].Field<sbyte>("RaidAttempts") == 1,
                    TerritoryWar = dataToday.Rows[i].Field<sbyte>("TerritoryWar") == 1,
                    TerritoryBattle = dataToday.Rows[i].Field<sbyte>("TerritoryBattle") == 1,
                    date = dataToday.Rows[i].Field<DateTime>("date"),
                };

<<<<<<< Updated upstream
                if (memberResult.GetTotalStrikes() > 0)
=======
                DataTable isExcludedData = await Database.SendSqlPull($"SELECT * FROM excludefromtickets WHERE date > '{DateTime.Now.ToString("yyyy-MM-dd")}' AND playerName = '{memberResult.playerName}'");
                // if the player is found in this database it means they should not be included in the tickettracker
                if (isExcludedData.Rows.Count == 0)
>>>>>>> Stashed changes
                {
                    resultString += $"- {memberResult.playerName} +{memberResult.GetTotalStrikes()} ticket(s) today \n";

                    if(memberResult.missingTickets)
                    {
<<<<<<< Updated upstream
                        resultString += $"  - did not reach the minimal ticket requirement of {minimalTicketValue}";
=======
                        ConfigReader reader = new();
                        await reader.readConfig();
                        DiscordMember? dcMember = await DiscordUserHelper.GetDiscordUserFromIngameName(memberResult.playerName, client.Guilds[reader.server_id].Members);
                        if(dcMember != null)
                        {
                            resultString += $"- {dcMember.Mention} \n";
                        }
                        else
                        {
                            resultString += $"- {memberResult.playerName} *please sync your name with `/sync name (swgoh name)`*\n";
                        }
        
                        DateTime now = DateTime.Now;
                        DataTable memberResultDataThisMonth = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE date BETWEEN '{new DateTime(now.Year, now.Month, 1).ToString("yyyy-MM-dd")}' AND '{new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1).ToString("yyyy-MM-dd")}' AND playerName = '{memberResult.playerName}'");

                        int ticketAmount = 0;
                        if (memberResultDataThisMonth.Rows.Count >= 1)
                        {
                            for (int j = 0; j < memberResultDataThisMonth.Rows.Count; j++)
                            {
                                ticketAmount += new IMemberTicketResult()
                                {
                                    RaidAttempts = memberResultDataThisMonth.Rows[j].Field<sbyte>("RaidAttempts") == 1,
                                    TerritoryBattle = memberResultDataThisMonth.Rows[j].Field<sbyte>("TerritoryBattle") == 1,
                                    TerritoryWar = memberResultDataThisMonth.Rows[j].Field<sbyte>("TerritoryWar") == 1,
                                    missingTickets = memberResultDataThisMonth.Rows[j].Field<sbyte>("missingTickets") == 1,
                                }.GetTotalStrikes();
                            }
                        }

                        ticketAmount = ticketAmount % 3;

                        resultString += $" - +{memberResult.GetTotalStrikes()} strike(s) | Total {ticketAmount} \n";

                        if (memberResult.missingTickets)
                        {
                            resultString += $"   - ticket requirement not reached";
                        }
                        if (memberResult.RaidAttempts)
                        {
                            resultString += $"   - did not attempt the raid";
                        }
                        if (memberResult.TerritoryWar)
                        {
                            resultString += $"   - failed on TW";
                        }
                        if (memberResult.TerritoryBattle)
                        {
                            resultString += $"   - failed on TB";
                        }

                        resultString += "\n";
>>>>>>> Stashed changes
                    }
                    if(memberResult.RaidAttempts)
                    {
                        resultString += $"  - did not attempt the raid";
                    }
                    if(memberResult.TerritoryWar) 
                    {
                        resultString += $"  - failed on TW";
                    }
                    if (memberResult.TerritoryBattle)
                    {
                        resultString += $"  - failed on TB";
                    }

                    resultString += "\n\n";
                }
            }

            if(resultString.Length > 2000)
            {
                resultString = resultString.Substring(0, 2000);
            }

            return resultString;
        }
    }

    internal class ExcelHelper {
        internal readonly string fileName = "strike-list.xlsx";

            public async Task BuildExcel(DataTable dataToday)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Missed tickets only");
                    int heightIndex = 5;
                    worksheet.ColumnWidth = 16;

                    // fill excel headers
                    worksheet.Cell("A1").Value = "Strike reason";
                    worksheet.Cell("B1").Value = "Missing 400 tickets";
                    worksheet.Cell("C1").Value = "TB (0 TB Points in a Phase)";
                    worksheet.Cell("D1").Value = "TW (0 banners in Defense Phase)";
                    worksheet.Cell("E1").Value = "Raids (0 attempts)";
                    worksheet.Cell("F1").Value = "Total strikes";
                    worksheet.Cell("A3").Value = "Member name";
                    // pull data from database
                    int memberIndex = 0;
                for (int i = 0; i < dataToday.Rows.Count; i++)
                {
                    IMemberTicketResult memberResult = new IMemberTicketResult()
                    {
                        playerName = dataToday.Rows[i].Field<string>("playerName"),
                        missingTickets = dataToday.Rows[i].Field<sbyte>("missingTickets") == 1,
                        RaidAttempts = dataToday.Rows[i].Field<sbyte>("RaidAttempts") == 1,
                        TerritoryWar = dataToday.Rows[i].Field<sbyte>("TerritoryWar") == 1,
                        TerritoryBattle = dataToday.Rows[i].Field<sbyte>("TerritoryBattle") == 1,
                        date = dataToday.Rows[i].Field<DateTime>("date"),
                    };

                    DataTable isExcludedData = await Database.SendSqlPull($"SELECT * FROM excludefromtickets WHERE date > '{DateTime.Now.ToString("yyyy-MM-dd")}' AND playerName = '{memberResult.playerName}'");
                    // if the player is found in this database it means they should not be included in the tickettracker
                    if (isExcludedData.Rows.Count == 0)
                    {
                        // get tickets for player this month
                        DateTime now = DateTime.Now;
                        DataTable memberResultDataThisMonth = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE date BETWEEN '{new DateTime(now.Year, now.Month, 1).ToString("yyyy-MM-dd")}' AND '{new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1).ToString("yyyy-MM-dd")}' AND playerName = '{memberResult.playerName}'");

                        worksheet.Cell("A" + (memberIndex + heightIndex)).Value = memberResult.playerName;

                        if (memberResult.missingTickets)
                        {
                            var cell = worksheet.Cell("B" + (memberIndex + heightIndex));
                            cell.Style.Fill.BackgroundColor = XLColor.Red;
                            PaintBorders(cell);
                        }

                        if (memberResult.RaidAttempts)
                        {
                            var cell = worksheet.Cell("C" + (memberIndex + heightIndex));
                            cell.Style.Fill.BackgroundColor = XLColor.Red;
                            PaintBorders(cell);
                        }

                        if (memberResult.TerritoryWar)
                        {
                            var cell = worksheet.Cell("D" + (memberIndex + heightIndex));
                            cell.Style.Fill.BackgroundColor = XLColor.Red;
                            PaintBorders(cell);
                        }

                        if (memberResult.TerritoryBattle)
                        {
                            var cell = worksheet.Cell("E" + (memberIndex + heightIndex));
                            cell.Style.Fill.BackgroundColor = XLColor.Red;
                            PaintBorders(cell);
                        }

                        int ticketAmount = 0;
                        int ticketMaxReached = 0;
                        if (memberResultDataThisMonth.Rows.Count >= 1)
                        {
                            for (int j = 0; j < memberResultDataThisMonth.Rows.Count; j++)
                            {
                                ticketAmount += new IMemberTicketResult()
                                {
                                    RaidAttempts = memberResultDataThisMonth.Rows[j].Field<sbyte>("RaidAttempts") == 1,
                                    TerritoryBattle = memberResultDataThisMonth.Rows[j].Field<sbyte>("TerritoryBattle") == 1,
                                    TerritoryWar = memberResultDataThisMonth.Rows[j].Field<sbyte>("TerritoryWar") == 1,
                                    missingTickets = memberResultDataThisMonth.Rows[j].Field<sbyte>("missingTickets") == 1,
                                }.GetTotalStrikes();
                            }
                        }

                        ticketMaxReached = (int)MathF.Floor(ticketAmount / 3);
                        ticketAmount = ticketAmount % 3;
                        if (ticketAmount == 0)
                        {
                            ticketAmount = 3;
                            worksheet.Cell("G" + (memberIndex + heightIndex)).Value = "3rd ticket reached";
                            await Database.SendSqlSave($"INSERT INTO ExcludeFromTickets (playerName, date) VALUES ('{memberResult.playerName}', '{DateTime.Now.AddDays(2).ToString("yyyy-MM-dd")}')");
                        }

                        var strikesCell = worksheet.Cell("F" + (memberIndex + heightIndex));

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
                        PaintBorders(strikesCell);
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
        }

        internal void PaintBorders(IXLCell cell)
        {
           // Default color is black
           cell.Style
                .Border.SetTopBorder(XLBorderStyleValues.Medium)
                .Border.SetRightBorder(XLBorderStyleValues.Medium)
                .Border.SetBottomBorder(XLBorderStyleValues.Medium)
                .Border.SetLeftBorder(XLBorderStyleValues.Medium);
        }

        internal FileStream GetGeneratedFile()
        {
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}