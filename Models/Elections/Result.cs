using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.Elections
{
    public class Result
    {
        [Key]
        public int ID { get; set; }
        public short? Year { get; set; }
        public string Election { get; set; }
        public string PrecinctName { get; set; }
        public short PrecinctCode { get; set; }
        public string Race{ get; set; }
        public byte? LEG { get; set; }
        public byte? CC { get; set; }
        public byte? CG { get; set; }
        public string Party { get; set; }
        public string CounterType { get; set; }
        public short Count { get; set; }
    }
}
