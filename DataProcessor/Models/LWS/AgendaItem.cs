using System;
using System.Collections.Generic;

namespace WhipStat.Models.LWS
{
    public class AgendaItem
    {
        public int AgendaId { get; set; }
        public string BillId { get; set; }
        public string Description { get; set; }
        public string PrimeSponsorTestified { get; set; }
        public string PrimeSponsorName { get; set; }
        public List<Testimony> Testifiers { get; set; }

        public override string ToString() => Description;
        public override bool Equals(Object obj) => AgendaId == ((AgendaItem)obj).AgendaId;
        public override int GetHashCode() => AgendaId;
    }

    public class Testimony
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public Boolean IsSpeaking { get; set; }
        public Boolean OutOfTown { get; set; }
        public Boolean CalledUp { get; set; }
        public Boolean NoShow { get; set; }
        public DateTime TimeOfSignIn { get; set; }

        public override string ToString() => $"{LastName}, {FirstName}";
    }
}
