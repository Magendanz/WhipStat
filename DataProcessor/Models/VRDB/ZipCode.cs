using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.VRDB
{
    public class ZipCode
    {
        [Key]
        public int LegislativeDistrict { get; set; }
        public string ZipCodes { get; set; }
    }
}
