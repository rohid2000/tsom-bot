using System.Collections.ObjectModel;
using tsom_bot.Models;

namespace tsom_bot.Commands.Helpers
{
    public class TicketTrackerCommandHelper {
        public string message;
        private int minimalTicketValue;

        public TicketTrackerCommandHelper(IGuild guildData, int minimalTicketValue)
        {
            this.minimalTicketValue = minimalTicketValue;
            this.message = this.GetMemberTicketResultList(guildData);
        }

        async public static Task<TicketTrackerCommandHelper> BuildViewModelAsync(string guildId)  
        {       
            ObservableCollection<IGuild> guildData = new();
            guildData.Add(await GuildFetcher.GetGuildById(guildId, true, new()));
            
            return new TicketTrackerCommandHelper(guildData[0], 600);
        }   

        private string GetMemberTicketResultList(IGuild GuildData)
        {
            string ResultListString = "";
            for(int i = 0; i < GuildData.member.Length; i++)
            {
                IMember member = GuildData.member[i];
                IMemberContribution? contribution = member?.memberContribution?[1];
                
                if(contribution != null)
                {
                    ContributionReached contributionReached = IsTicketGoalReached(int.Parse(contribution.currentValue));

                    /* ConvertContributionReachedToString(contributionReached) */
                    ResultListString += $"{ member.playerName } | { member?.memberContribution?[0].currentValue } | { member?.memberContribution?[1].currentValue } | { member?.memberContribution?[2].currentValue } \n";
                }
            }

            if(ResultListString.Length >= 2000)
            {
                ResultListString = ResultListString.Substring(0, 2000);
            }

            return ResultListString;
        }

        private ContributionReached IsTicketGoalReached(int contributionValue)
        {
            if(contributionValue >= this.minimalTicketValue)
            {
                return ContributionReached.Yes;
            } else {
                return ContributionReached.No;
            }
        }

        private string ConvertContributionReachedToString(ContributionReached contributionReached)
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

    enum ContributionReached
    {
        Yes, No, NVT
    }
}