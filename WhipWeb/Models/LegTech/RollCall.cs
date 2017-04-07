using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhipStat.Models.LegTech
{
    public class RollCall
    {
        [Key]
        public int RollCall_Id { get; set; }
        public short BillNumber { get; set; }
        public short Year { get; set; }
        public int Yea_total { get; set; }
        public int Nay_total { get; set; }
        public int Excused_total { get; set; }
        public int Absent_total { get; set; }
        public string Agency { get; set; }
        public string Motion_description { get; set; }
        public string Amendment_description { get; set; }
        public double? Score { get; set; }

        public override string ToString()
        {
            return $"Roll Call #{RollCall_Id}";
        }
        public override bool Equals(Object obj)
        {
            return RollCall_Id == ((RollCall)obj).RollCall_Id;
        }

        public override int GetHashCode()
        {
            return RollCall_Id;
        }
    }
}
