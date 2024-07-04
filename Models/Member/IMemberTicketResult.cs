using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsom_bot.Models.Member
{
    public class IMemberTicketResult
    {
        public string playerName { get; set; }
        public bool missingTickets { get; set; }
        public bool TerritoryBattle { get; set; }
        public bool TerritoryWar { get; set; }
        public bool RaidAttempts { get; set; }
        public DateTime? date { get; set; }

        public int GetTotalStrikes()
        {
            int strikes = 0;

            if (this.missingTickets)
                strikes++;
            if(this.TerritoryBattle)
                strikes++;
            if(this.TerritoryWar) 
                strikes++;
            if(RaidAttempts) 
                strikes++;

            return strikes;
        }
    }
}
