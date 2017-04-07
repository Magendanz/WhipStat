using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class Vote
    {
        public int RollCall_Id { get; set; }
        public int Member_Id { get; set; }
        public string Member_vote { get; set; }

        public override string ToString()
        {
            return $"Member #{Member_Id} voted {Member_vote} for RollCall #{RollCall_Id}";
        }
    }
}
