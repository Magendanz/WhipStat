using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace WhipStat.Models.LegTech
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Committee
    {
        [Key]
        [DataMember(IsRequired = true, Order = 0)]
        public short Id { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string Name { get; set; }
        [DataMember(IsRequired = true, Order = 2)]
        public string LongName { get; set; }
        [DataMember(IsRequired = true, Order = 3)]
        public string Agency { get; set; }
        [DataMember(IsRequired = true, Order = 4)]
        public string Acronym { get; set; }
        [DataMember(Order = 5)]
        public string Phone { get; set; }

        public override string ToString()
        {
            return $"{LongName}";
        }
        public override bool Equals(Object obj)
        {
            return Id == ((Committee)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
