﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class Vote
    {
        [Key]
        public int ID { get; set; }
        public int RollCall_Vote { get; set; }
        public int Yea_total { get; set; }
        public int Nay_total { get; set; }
        public int Excused_total { get; set; }
        public int Absent_total { get; set; }
        public string Motion_description { get; set; }
        public string Amendment_description { get; set; }
        public string ShortBillId { get; set; }
        public string LegNum { get; set; }
        public string AbbrTitle { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Member_vote { get; set; }
        public string Biennium { get; set; }
        public string Agency { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} voted {Member_vote} for {ShortBillId}";
        }
    }
}