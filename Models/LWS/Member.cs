using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Member
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
        [DataMember(IsRequired = true, Order = 5)]
        public string Party { get; set; }
        [DataMember(IsRequired = true, Order = 6)]
        public string District { get; set; }
        [DataMember(Order = 7)]
        public string Phone { get; set; }
        [DataMember(Order = 8)]
        public string Email { get; set; }
        [DataMember(IsRequired = true, Order = 9)]
        public string FirstName { get; set; }
        [DataMember(IsRequired = true, Order = 10)]
        public string LastName { get; set; }

        public override string ToString() => LongName;
        public override bool Equals(Object obj) => Id == (obj as Member).Id;
        public override int GetHashCode() => Id;
    }
}
