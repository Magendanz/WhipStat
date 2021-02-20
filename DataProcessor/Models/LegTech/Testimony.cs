using System;
using System.ComponentModel.DataAnnotations;

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
            => (LastName == ((Testimony)obj).LastName) && (FirstName == ((Testimony)obj).FirstName) && (TimeOfSignIn == ((Testimony)obj).TimeOfSignIn);
        public override int GetHashCode()
            => ToString().GetHashCode() ^ TimeOfSignIn.GetHashCode();
    }
}
