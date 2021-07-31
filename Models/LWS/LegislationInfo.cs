using System;
using System.Runtime.Serialization;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class LegislationInfo
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string Biennium { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string BillId { get; set; }
        [DataMember(IsRequired = true, Order = 2)]
        public short BillNumber { get; set; }
        [DataMember(Order = 3)]
        public short SubstituteVersion { get; set; }
        [DataMember(Order = 4)]
        public short EngrossedVersion { get; set; }
        [DataMember(Order = 5)]
        public LegislationType ShortLegislationType { get; set; }
        [DataMember(Order = 6)]
        public string OriginalAgency { get; set; }
        [DataMember(Order = 7)]
        public bool Active { get; set; }

        public override string ToString()
            => $"{BillId} ({Biennium})";
        public override bool Equals(Object obj)
            => obj is LegislationInfo l && (BillId, Biennium).Equals((l.BillId, l.Biennium));
        public override int GetHashCode()
            => (BillId, Biennium).GetHashCode();
    }
}
