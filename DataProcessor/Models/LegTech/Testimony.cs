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
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string BillId { get; set; }
        public string Position { get; set; }
        public bool IsSpeaking { get; set; }
        public bool OutOfTown { get; set; }
        public bool CalledUp { get; set; }
        public bool NoShow { get; set; }
        public DateTime TimeOfSignIn { get; set; }

        public override string ToString() => $"{LastName}, {FirstName}";
        public override bool Equals(Object obj)
            => obj is Testimony t && (LastName, FirstName, TimeOfSignIn).Equals((t.LastName, t.FirstName, t.TimeOfSignIn));
        public override int GetHashCode()
            => (LastName, FirstName, TimeOfSignIn).GetHashCode();
    }
}
