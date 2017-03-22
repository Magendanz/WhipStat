using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.VRDB
{
    public class District
    {
        [Key]
        public int Id { get; set; }
        public string CountyCode { get; set; }
        public string County { get; set; }
        public string DistrictType { get; set; }
        public int DistrictID { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public int PrecinctCode { get; set; }
        public string PrecinctName { get; set; }
        public int PrecinctPart { get; set; }
    }

    public class Precinct
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }
}
