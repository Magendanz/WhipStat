using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.Fundraising
{
    public class Party
    {
        [Key]
        public int FParty_ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Inactive { get; set; }
    }
}
