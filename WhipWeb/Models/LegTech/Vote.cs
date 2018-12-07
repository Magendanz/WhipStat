using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class Vote
    {
        public int RollCallId { get; set; }
        public int MemberId { get; set; }
        public string MemberVote { get; set; }

        public override string ToString()
            => $"Member #{MemberId} voted {MemberVote} for RollCall #{RollCallId}";
    }
}
