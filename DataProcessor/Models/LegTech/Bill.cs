﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
            => (BillId == ((Bill)obj).BillId) && (Biennium == ((Bill)obj).Biennium);

        public override int GetHashCode()
            => ToString().GetHashCode();
    }
}
