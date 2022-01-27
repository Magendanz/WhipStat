using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.Fundraising
{
    public class Donation
    {
        [Key]
        public int Ident { get; set; }
        public int RepNo { get; set; }
        public string Filer_ID { get; set; }
        public DateTime? Rpt_Date { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Prim_Gen { get; set; }
        public DateTime? Rcpt_Date { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Aggregate { get; set; }
        public string Employer { get; set; }
        public string Occup { get; set; }
        public string Empl_City { get; set; }
        public string Empl_State { get; set; }
        public string Memo { get; set; }
        public int? District { get; set; }
        public string ID { get; set; }
        public string Pty { get; set; }
    }
}
