using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhipStat.Models.LegTech
{
    public class Bill
    {
        public short BillNumber { get; set; }
        public short Year { get; set; }
        public string ShortBillId { get; set; }
        public string AbbrTitle { get; set; }
        public string Status { get; set; }
        public double? Score { get; set; }

        public override string ToString()
        {
            return $"{ShortBillId} [{Year}]";
        }
        public override bool Equals(Object obj)
        {
            return (BillNumber == ((Bill)obj).BillNumber) && (Year == ((Bill)obj).Year);
        }

        public override int GetHashCode()
        {
            return Year << 16 + BillNumber;
        }
    }
}
