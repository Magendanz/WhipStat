using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Committee
    {
        [Key, DataMember(IsRequired = true, Order = 0)]
        public int Id { get; set; }
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

        public override string ToString() => LongName;
        public override bool Equals(Object obj) => Id == ((Committee)obj).Id;
        public override int GetHashCode() => Id;
    }
}