using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.PDC
{
    public class Office
    {
        [Key]
        public int FOffice_ID { get; set; }
        public string OffCode { get; set; }
        public string OffTitle { get; set; }
        public bool On_Off_Code { get; set; }
        public string SName { get; set; }
    }
}
