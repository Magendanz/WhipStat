using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhipStat.Models.LegTech
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public string[] Keywords { get; set; }

        public override string ToString() => Name;
        public override bool Equals(Object obj)
            => obj is Organization o && Id == o.Id;
        public override int GetHashCode()
            => Id;
    }
}