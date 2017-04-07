using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace WhipStat.Models.LegTech
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Member
    {
        [Key]
        [DataMember(IsRequired = true, Order = 0)]
        public int Id { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string Name { get; set; }
        [DataMember(Order = 2)]
        public string LongName { get; set; }
        [DataMember(Order = 3)]
        public string Agency { get; set; }
        [DataMember(Order = 4)]
        public string Acronym { get; set; }
        [DataMember(IsRequired = true, Order = 5)]
        public string Party { get; set; }
        [DataMember(Order = 6)]
        public short District { get; set; }
        [DataMember(Order = 7)]
        public string Phone { get; set; }
        [DataMember(Order = 8)]
        public string Email { get; set; }
        [DataMember(IsRequired = true, Order = 9)]
        public string FirstName { get; set; }
        [DataMember(IsRequired = true, Order = 10)]
        public string LastName { get; set; }

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
