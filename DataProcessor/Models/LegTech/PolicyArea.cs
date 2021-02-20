using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class PolicyArea
    {
        [Key]
        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public string Committees { get; set; }

        public override string ToString()
            => Name;
        public override bool Equals(Object obj)
            => (Id == ((PolicyArea)obj).Id) && (Name == ((PolicyArea)obj).Name);
        public override int GetHashCode()
            => Id ^ Name.GetHashCode();
    }
}
