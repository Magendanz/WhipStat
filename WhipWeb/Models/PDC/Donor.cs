﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.PDC
{
    public class Donor
    {
        [Key]
        public int ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public double? Aggregate { get; set; }

        public override string ToString()
        {
            return $"Donor #{ID} ({Name})";
        }
    }
}
