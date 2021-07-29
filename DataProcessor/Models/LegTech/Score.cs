using System;

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
            => obj is Score s &&  (MemberId, Year, PolicyArea).Equals((s.MemberId, s.Year, s.PolicyArea));
        public override int GetHashCode()
            => (MemberId, Year, PolicyArea).GetHashCode();
    }
}
