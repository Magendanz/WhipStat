using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class Score
    {
        public int MemberId { get; set; }
        public short Year { get; set; }
        public short PolicyArea { get; set; }
        public double Total { get; set; }
        public int Count { get; set; }

        public override string ToString()
            => $"Score #{MemberId} for Policy Area #{PolicyArea} in {Year}";

        public override bool Equals(Object obj)
            => (MemberId == ((Score)obj).MemberId) && (Year == ((Score)obj).Year) && (PolicyArea == ((Score)obj).PolicyArea);

        public override int GetHashCode()
            => (MemberId << 16) ^ (Year << 8) ^ PolicyArea;
    }
}
