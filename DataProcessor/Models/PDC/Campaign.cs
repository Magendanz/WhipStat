using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.PDC
{
    public class Campaign
    {
        [Key]
        public int C1_ID { get; set; }
        public int RepNo { get; set; }
        public string Filer_ID { get; set; }
        public string Election_Year { get; set; }
        public string Name { get; set; }
        public string Pty { get; set; }
        public string LDis { get; set; }
        public string Jurisdiction { get; set; }
        public string Memo { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(Object obj)
        {
            return String.Equals(Name, ((Campaign)obj).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
