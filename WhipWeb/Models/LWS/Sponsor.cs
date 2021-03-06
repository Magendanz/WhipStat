﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Sponsor
    {
        [DataMember(IsRequired = true, Order = 0)]
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
        public string Type { get; set; }
        [DataMember(IsRequired = true, Order = 6)]
        public short Order { get; set; }
        [DataMember(IsRequired = true, Order = 7)]
        public string Phone { get; set; }
        [DataMember(IsRequired = true, Order = 8)]
        public string Email { get; set; }
        [DataMember(IsRequired = true, Order = 9)]
        public string FirstName { get; set; }
        [DataMember(IsRequired = true, Order = 10)]
        public string LastName { get; set; }

        public override string ToString() => LongName;
        public override int GetHashCode() => Id;
    }
}
