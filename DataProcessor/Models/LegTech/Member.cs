using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhipStat.Models.LegTech
{
    public class Member
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public short District { get; set; }
        public string Party { get; set; }
        public double? Score { get; set; }

        public override string ToString()
        {
            return $"{LastName}, {FirstName}";
        }

        public override bool Equals(Object obj)
        {
            return String.Equals(LastName, ((Member)obj).LastName) && String.Equals(FirstName, ((Member)obj).FirstName);
        }

        public override int GetHashCode()
        {
            return LastName.GetHashCode() ^ FirstName.GetHashCode();
        }
    }
}
