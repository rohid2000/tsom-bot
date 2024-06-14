using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsom_bot.Models;
public interface IMember
    {
        public string PlayerId { get; }
        public IMemberContribution MemberContribution { get; }
        public string PlayerName { get; }
        public int PlayerLevel { get; }
        public string LastActivityTime { get; }
        public string GalacticPower { get; }
    }