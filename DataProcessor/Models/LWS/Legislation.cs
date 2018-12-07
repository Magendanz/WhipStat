using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Legislation
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
        [DataMember(Order = 8)]
        public bool StateFiscalNote { get; set; }
        [DataMember(Order = 9)]
        public bool LocalFiscalNote { get; set; }
        [DataMember(Order = 10)]
        public bool Appropriations { get; set; }
        [DataMember(Order = 11)]
        public bool RequestedByGovernor { get; set; }
        [DataMember(Order = 12)]
        public bool RequestedByBudgetCommittee { get; set; }
        [DataMember(Order = 13)]
        public bool RequestedByDepartment { get; set; }
        [DataMember(Order = 14)]
        public bool RequestedByOther { get; set; }
        [DataMember(Order = 15)]
        public string ShortDescription { get; set; }
        [DataMember(Order = 16)]
        public string Request { get; set; }
        [DataMember(Order = 17)]
        public DateTime IntroducedDate { get; set; }
        [DataMember(Order = 18)]
        public BillStatus CurrentStatus { get; set; }
        [DataMember(Order = 19)]
        public string Sponsor { get; set; }
        [DataMember(Order = 20)]
        public int PrimeSponsorID { get; set; }
        [DataMember(Order = 21)]
        public string LongDescription { get; set; }
        [DataMember(Order = 22)]
        public string LegalTitle { get; set; }
        [DataMember(Order = 23)]
        public List<Companion> Companions { get; set; }

        public override string ToString()
            => $"{BillId} ({Biennium})";

        public override bool Equals(Object obj)
            => BillId == ((LegislationInfo)obj).BillId && Biennium == ((LegislationInfo)obj).Biennium;

        public override int GetHashCode()
            => ToString().GetHashCode();
    }

    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class LegislationType
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string ShortLegislationType { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string LongLegislationType { get; set; }

        public override string ToString() => LongLegislationType;
    }

    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class BillStatus
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string BillId { get; set; }
        [DataMember(Order = 1)]
        public string HistoryLine { get; set; }
        [DataMember(Order = 2)]
        public DateTime ActionDate { get; set; }
        [DataMember(Order = 3)]
        public bool AmendedByOppositeBody { get; set; }
        [DataMember(Order = 4)]
        public bool PartialVeto { get; set; }
        [DataMember(Order = 5)]
        public bool Veto { get; set; }
        [DataMember(Order = 6)]
        public bool AmendmentsExist { get; set; }
        [DataMember(Order = 7)]
        public string Status { get; set; }

        public override string ToString() => Status;
    }

    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Companion
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string Biennium { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string BillId { get; set; }
        [DataMember(Order = 2)]
        public string Status { get; set; }

        public override string ToString() => BillId;
    }
}
