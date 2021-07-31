using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Hearing
    {
        [DataMember(Order = 0)]
        public string BillId { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string Biennium { get; set; }
        [DataMember(IsRequired = true, Order = 2)]
        public CommitteeMeeting CommitteeMeeting { get; set; }
        [DataMember(Order = 3)]
        public string HearingType { get; set; }
        [DataMember(Order = 4)]
        public string HearingTypeDescription { get; set; }

        public override string ToString() 
            => $"{CommitteeMeeting.Agency} Hearing #{CommitteeMeeting.AgendaId} ({Biennium})";
        public override bool Equals(object obj)
            => obj is Hearing h && (CommitteeMeeting, Biennium).Equals((h.CommitteeMeeting, Biennium));
        public override int GetHashCode()
            => (CommitteeMeeting, Biennium).GetHashCode();
    }

    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class CommitteeMeeting
    {
        [DataMember(IsRequired = true, Order = 0)]
        public int AgendaId { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string Agency { get; set; }
        [DataMember(IsRequired = true, Order = 2)]
        public List<Committee> Committees { get; set; }
        [DataMember(Order = 3)]
        public string Room { get; set; }
        [DataMember(Order = 4)]
        public string Building { get; set; }
        [DataMember(Order = 5)]
        public string Address { get; set; }
        [DataMember(Order = 6)]
        public string City { get; set; }
        [DataMember(Order = 7)]
        public string State { get; set; }
        [DataMember(Order = 8)]
        public string ZipCode { get; set; }
        [DataMember(Order = 9)]
        public DateTime Date { get; set; }
        [DataMember(Order = 10)]
        public bool Cancelled { get; set; }
        [DataMember(Order = 11)]
        public DateTime RevisedDate { get; set; }
        [DataMember(Order = 12)]
        public string ContactInformation { get; set; }
        [DataMember(Order = 13)]
        public string CommitteeType { get; set; }
        [DataMember(Order = 14)]
        public string Notes { get; set; }

        public override string ToString() 
		  => $"{Agency} Meeting #{AgendaId}";
        public override bool Equals(object obj) 
		  => obj is CommitteeMeeting c && (Agency, AgendaId).Equals((c.Agency, c.AgendaId));
        public override int GetHashCode() 
		  => (Agency, AgendaId).GetHashCode();
    }
}