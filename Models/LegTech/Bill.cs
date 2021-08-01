using System;

namespace WhipStat.Models.LegTech
{
    public class Bill
    {
        public short BillNumber { get; set; }
        public string Biennium { get; set; }
        public string BillId { get; set; }
        public string AbbrTitle { get; set; }
        public string Sponsors { get; set; }
        public string Committees { get; set; }
        public short? PolicyArea { get; set; }
        public double? Score { get; set; }

        public override string ToString()
            => $"{BillId} ({Biennium})";
        public override bool Equals(Object obj)
            => obj is Bill b && (BillNumber, Biennium).Equals((b.BillNumber, b.Biennium));
        public override int GetHashCode()
            => (BillNumber, Biennium).GetHashCode();
    }
}
