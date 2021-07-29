using System;

namespace WhipStat.Models.LegTech
{
    public class Testimony
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BillId { get; set; }
        public string Position { get; set; }
        public bool Testify { get; set; }
        public bool OutOfTown { get; set; }
        public bool CalledUp { get; set; }
        public bool NoShow { get; set; }
        public DateTime TimeSignedIn { get; set; }

        public override string ToString() => $"{LastName}, {FirstName}";
        public override bool Equals(Object obj)
            => obj is Testimony t && (LastName, FirstName, TimeSignedIn).Equals((t.LastName, t.FirstName, t.TimeSignedIn));
        public override int GetHashCode()
            => (LastName, FirstName, TimeSignedIn).GetHashCode();
    }
}
