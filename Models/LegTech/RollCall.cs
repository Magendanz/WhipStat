using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class RollCall
    {
        [Key]
        public int Id { get; set; }
        public int BillNumber { get; set; }
        public string BillId { get; set; }
        public string Biennium { get; set; }
        public string Agency { get; set; }
        public string Motion { get; set; }
        public short SequenceNumber { get; set; }
        public DateTime VoteDate { get; set; }
        public short YeaVotes { get; set; }
        public short NayVotes { get; set; }
        public short AbsentVotes { get; set; }
        public short ExcusedVotes { get; set; }
        public double? Score { get; set; }

        public override string ToString()
            => $"Roll Call #{Id}";
        public override bool Equals(Object obj)
            => Id == (obj as RollCall).Id;
        public override int GetHashCode()
            => Id;
    }
}
