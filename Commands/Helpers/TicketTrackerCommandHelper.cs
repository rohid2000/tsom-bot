using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Net;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DSharpPlus;
using DSharpPlus.Entities;
using tsom_bot.config;
using tsom_bot.Fetcher.database;
using tsom_bot.Models;
using tsom_bot.Models.Member;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerCommandHelper {
        private int minimalTicketValue;
        private string guildId;
        private DiscordClient client;
        async public static Task<TicketTrackerCommandHelper> BuildViewModelAsync(string guildId, int minimalTicketValue, DiscordClient client)  
        {       
            TicketTrackerCommandHelper helper =  new TicketTrackerCommandHelper();
            helper.guildId = guildId;
            helper.minimalTicketValue = minimalTicketValue;
            helper.client = client;
            return helper;
        }
        
        public async Task SaveGuildData()
        {
            ObservableCollection<IGuild> guildData = new();
            guildData.Add(await GuildFetcher.GetGuildById(this.guildId, true, new()));
            TicketTrackerSaveCommandHelper saveHelper = new();
            await saveHelper.SaveTicketTrackerResultsInDatabase(guildData[0].GetTicketResults(minimalTicketValue));
        }   

        public async Task<bool> IsDataSynced()
        {
            TicketTrackerSaveCommandHelper saveHelper = new();
            return await saveHelper.IsSyncedToday();
        }

        public async Task<FileStream?> GetExcelFile()
        {
            ExcelHelper excel = new();
            DataTable dataToday = await Database.SendSqlPull($"SELECT * FROM ticketresults WHERE date = '{DateTime.Now.ToString("yyyy-MM-dd")}';");
            await excel.BuildExcel(dataToday);

            return excel.GetGeneratedFile();
        }

        public async Task SyncExcelFile(DiscordAttachment file)
        {
            using(var webClient = new WebClient())
            {
                webClient.DownloadFile(new Uri(file.Url), file.FileName);
            }
            ExcelHelper excel = new();
            await excel.ReadExcel(file.FileName);
        }

        public async Task CleanupStrikes()
        {
            ObservableCollection<IGuild> guildData = new();
            guildData.Add(await GuildFetcher.GetGuildById(this.guildId, true, new()));

            foreach(IMember member in guildData[0].member)
            {
                await member.cleanupStrikeResults();
            }
        }

        public async Task<string> removeStrikes(DiscordUser member, int dayAmount = 1)
        {
            DataTable accounts = await DiscordUserHelper.GetLinkedAccounts(member);
            string message = "";
            foreach (DataRow accountRow in accounts.Rows)
            {
                string accountName = accountRow.Field<string>("playerName");

                DataTable result = await Database.SendSqlPull($"SELECT * FROM `ticketresults` WHERE playerName = '{accountName}' ORDER BY date DESC");

                if (result.Rows.Count < 1)
                {
                    message += $"No ticket data found for user:{accountName}" + "\n";
                    continue;
                }
                
                for (int i = 0; i < dayAmount; i++)
                {
                    DataRow row = result.Rows[i];
                    message += $"removed strike for {accountName} on {row.Field<DateTime>("date").ToString("dd-MM-yyyy")}" + "\n";
                    await Database.SendSqlSave($"DELETE FROM ticketresults WHERE playerName = '{accountName}' AND date = '{row.Field<DateTime>("date").ToString("yyyy-MM-dd")}'");
                }
            }

            return message;
        }

        public async Task<string> GetMessage()
        {
            // pull data from database
            string resultString = "";

            DataTable dataToday = await Database.SendSqlPull($"SELECT * FROM `ticketresults` WHERE date = '{DateTime.Now.ToString("yyyy-MM-dd")}'");

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
                    if(memberResult.missingTickets)
                    {
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
                    }
                }
            }

            if(resultString.Length > 2000)
            {
                resultString = resultString.Substring(0, 2000);
            }

            return resultString;
        }

        public async Task AddMemberToNVT(DiscordUser member, int dayAmount = 0)
        {
            DiscordMember dcMember = member as DiscordMember;
            await AddMemberToNVT(dcMember, dayAmount);
        }

        public async Task AddMemberToNVT(DiscordMember member, int dayAmount = 0)
        {
            DataTable result = await DiscordUserHelper.GetLinkedAccounts(member);
            string dateTime = DateTime.Now.AddDays(dayAmount).ToString("yyyy-MM-dd");

            foreach (DataRow row in result.Rows) 
            {
                await Database.SendSqlSave($"INSERT INTO excludefromtickets (playerName, date) VALUES ('{row.Field<string>("playerName")}', '{dateTime}')");
            }
        }

        public async Task RemoveMemberToNVT(DiscordUser member)
        {
            DiscordMember dcMember = member as DiscordMember;
            await RemoveMemberToNVT(dcMember);
        }

        public async Task RemoveMemberToNVT(DiscordMember member)
        {
            DataTable result = await DiscordUserHelper.GetLinkedAccounts(member);
            foreach (DataRow row in result.Rows)
            {
                await Database.SendSqlSave($"DELETE FROM excludefromtickets WHERE playerName = '{row.Field<string>("playerName")}'");
            }
        }
    }

    internal class ExcelHelper {
        internal string fileName;

        private string generateFileName()
        {
            string fileName = "strike-list-" + DateTime.Now.ToString("yyyy-MM-dd");
            string randomString = "";
            for(int i = 0; i < 5; i++)
            {
                randomString += new Random().Next(1, 9).ToString();
            }
            string extension = ".xlsx";
            return fileName + "-" + randomString + extension;
        }

        public async Task ReadExcel(string fileName)
        {
            string sqlFormattedDate = DateTime.Now.ToString("yyyy-MM-dd");
            await Database.SendSqlSave($"DELETE FROM ticketresults WHERE date = '{sqlFormattedDate}'");

            int heightIndex = 5;
            XLWorkbook wb = new XLWorkbook(fileName);
            var ws = wb.Worksheet("Missed tickets only");

            for(int i = 0; i < 50; i++) 
            {
                var row = ws.Row(i+heightIndex);
                if(!row.IsEmpty())
                {
                    string playerName = row.Cell(1).Value.ToString();
                    bool missingTickets = row.Cell(2).Style.Fill.BackgroundColor == XLColor.Red;
                    bool RaidAttempts = row.Cell(3).Style.Fill.BackgroundColor == XLColor.Red;
                    bool TerritoryWar = row.Cell(4).Style.Fill.BackgroundColor == XLColor.Red;
                    bool TerritoryBattle = row.Cell(5).Style.Fill.BackgroundColor == XLColor.Red;

                    IMemberTicketResult memberResult = new IMemberTicketResult()
                    {
                        playerName = playerName,
                        missingTickets = missingTickets,
                        RaidAttempts = RaidAttempts,
                        TerritoryWar = TerritoryWar,
                        TerritoryBattle = TerritoryBattle,
                        date = DateTime.Now,
                    };


                    DataTable isExcludedData = await Database.SendSqlPull($"SELECT * FROM excludefromtickets WHERE date > '{DateTime.Now.ToString("yyyy-MM-dd")}' AND playerName = '{memberResult.playerName}'");
                    // if the player is found in this database it means they should not be included in the tickettracker
                    if (isExcludedData.Rows.Count == 0)
                    {
                        await Database.SendSqlSave($"INSERT INTO ticketresults (playerName, missingTickets, TerritoryBattle, TerritoryWar, RaidAttempts, date) VALUES ('{memberResult.playerName}', {memberResult.missingTickets}, {memberResult.TerritoryBattle}, {memberResult.TerritoryWar}, {memberResult.RaidAttempts}, '{memberResult.date.Value.ToString("yyyy-MM-dd")}')");
                    }
                }
            }
        }

            public async Task BuildExcel(DataTable dataToday)
            {
                fileName = generateFileName();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Missed tickets only");
                    int heightIndex = 5;
                    worksheet.ColumnWidth = 22;

                    // fill excel headers
                    worksheet.Cell("A1").Value = "Strike reason";
                    worksheet.Cell("B1").Value = "Missing 400 Tickets";
                    worksheet.Cell("C1").Value = "TB (0 Points in Defense)";
                    worksheet.Cell("D1").Value = "TW (0 banners in Defense)";
                    worksheet.Cell("E1").Value = "Raids (0 attempts)";
                    worksheet.Cell("F1").Value = "Total Strikes";
                    worksheet.Cell("G1").Value = "LifeTime Strikes";
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


                        DataTable resultLifeTime = await Database.SendSqlPull($"SELECT * FROM lifetimetickets WHERE playerName = '{memberResult.playerName}'");
                        int lifeTimeTickets = 0;
                        if(resultLifeTime.Rows.Count > 0)
                        {
                            lifeTimeTickets += resultLifeTime.Rows[0].Field<int>("ticketamount");
                        }

                        lifeTimeTickets += ticketAmount;
       
                        if (ticketAmount % 3 == 0 && memberResult.GetTotalStrikes() > 0)
                        {
                            ticketAmount = 3;
                        }
                        else
                        {
                            ticketAmount = ticketAmount % 3;
                        }
 
                        var lifetimeStrikesCell = worksheet.Cell("G" + (memberIndex + heightIndex));
                        var strikesCell = worksheet.Cell("F" + (memberIndex + heightIndex));

                        // when whe add the feature that the database gets cleared every month add a table that saves the tickets that month before it is cleared.
                        lifetimeStrikesCell.Value = lifeTimeTickets;

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