using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.VRDB
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public string StateVoterID { get; set; }
        public string CountyCode { get; set; }
        public DateTime Voted { get; set; }
    }
}
