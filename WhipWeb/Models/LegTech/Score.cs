using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class Score
    {
        public int Member_Id { get; set; }
        public short Year { get; set; }
        public short PolicyArea { get; set; }
        public double Total { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return $"Score #{Member_Id} for Policy Area #{PolicyArea} in {Year}";
        }

        public override bool Equals(Object obj)
        {
            return (Member_Id == ((Score)obj).Member_Id) && (Year == ((Score)obj).Year) && (PolicyArea == ((Score)obj).PolicyArea);
        }

        public override int GetHashCode()
        {
            return (Member_Id << 16) ^ (Year << 8) ^ PolicyArea;
        }
    }
}
