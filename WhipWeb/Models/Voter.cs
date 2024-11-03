using System;
using System.Collections.Generic;

namespace WhipStat.Models
{
    public class Voter
    {
        public string Id { get; set; }
        public int Build { get; set; }
        public string LName { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string Suffix { get; set; }
        public string Title { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountyCode { get; set; }
        public string PrecinctCode { get; set; }
        public string PrecinctPart { get; set; }
        public byte? LegislativeDistrict { get; set; }
        public byte? CongressionalDistrict { get; set; }
        public DateTime? Registered { get; set; }
        public DateTime? LastVoted { get; set; }
        public string StatusCode { get; set; }
        public short? Birthyear { get; set; }

        public override bool Equals(object obj)
            => obj is Voter v && (Id, LName, FName, Address).Equals((v.Id, v.LName, v.FName, v.Address));

        public override int GetHashCode()
            => (Id, LName, FName, Address).GetHashCode();
    }

    class VoterKeyComparer : IEqualityComparer<Voter>
    {
        public bool Equals(Voter x, Voter y)
            => (x.Id, x.Build).Equals((y.Id, y.Build));

        public int GetHashCode(Voter v)
            => (v.Id, v.Build).GetHashCode();
    }
}