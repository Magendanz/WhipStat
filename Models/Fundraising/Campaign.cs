using System;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.Fundraising
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
            => $"{Name}";
        public override bool Equals(Object obj)
            => obj is Campaign c && Name.Equals(c.Name);
        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
