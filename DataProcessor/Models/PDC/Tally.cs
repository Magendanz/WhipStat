using System;
using System.Collections.Generic;
using System.Text;

namespace WhipStat.Models.PDC
{
    public class Tally
    {
        public int DonorId { get; set; }
        public short Year { get; set; }
        public string Jurisdiction { get; set; }
        public int Contributions { get; set; }
        public int Campaigns { get; set; }
        public int Wins { get; set; }
        public int Unopposed { get; set; }
        public double Total { get; set; }
        public double Republican { get; set; }
        public double Democrat { get; set; }
    }
}
