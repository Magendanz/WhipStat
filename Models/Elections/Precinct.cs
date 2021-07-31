using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.Elections
{
    public class Precinct
    {
        [Key]
        public short PrecinctCode { get; set; }
        public string PrecinctName { get; set; }
        public byte LEG { get; set; }
        public byte CC { get; set; }
        public byte CG { get; set; }
    }
}
