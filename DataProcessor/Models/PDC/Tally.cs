using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.PDC
{
    public class Tally
    {
        public int Donor { get; set; }
        public short Year { get; set; }
        public string Jurisdiction { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
        public decimal Bias { get; set; }

        public override string ToString()
        {
            return $"Score #{Donor} in {Year} {Jurisdiction}";
        }

        public override bool Equals(Object obj)
        {
            return (Donor == ((Tally)obj).Donor) && (Year == ((Tally)obj).Year) && (Jurisdiction == ((Tally)obj).Jurisdiction);
        }

        public override int GetHashCode()
        {
            return Donor;
        }
    }
}
