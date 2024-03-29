﻿using System;
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
            => obj is PolicyArea p && (Id, Name).Equals((p.Id, p.Name));
        public override int GetHashCode()
            => (Id, Name).GetHashCode();
    }
}
