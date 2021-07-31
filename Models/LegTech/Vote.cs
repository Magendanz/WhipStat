using System;

namespace WhipStat.Models.LegTech
{
    public class Vote
    {
        public int RollCallId { get; set; }
        public int MemberId { get; set; }
        public string MemberVote { get; set; }

        public override string ToString()
            => $"Member #{MemberId} voted {MemberVote} for RollCall #{RollCallId}";
        public override bool Equals(object obj)
            => obj is Vote v && (RollCallId, MemberId).Equals((v.RollCallId, v.MemberId));
        public override int GetHashCode()
            => (RollCallId, MemberId).GetHashCode();
    }
}
