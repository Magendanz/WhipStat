using System;

namespace WhipStat.Models.LegTech
{
    public abstract class Record
    {
        public int Id { get; set; }
        public short BillNumber { get; set; }
        public string Biennium { get; set; }
        public short Sponsor { get; set; }
        public short Votes { get; set; }
        public short Support { get; set; }

        public override string ToString()
            => $"Member #{Id} voting record on Bill #{BillNumber} ({Biennium})";
        public override bool Equals(object obj)
            => obj is Record r && (Id, BillNumber, BillNumber).Equals((r.Id, r.BillNumber, r.BillNumber));
        public override int GetHashCode()
            => (Id, BillNumber, BillNumber).GetHashCode();
    }

    public class VotingRecord : Record { }
    public class AdvocacyRecord : Record { }
}