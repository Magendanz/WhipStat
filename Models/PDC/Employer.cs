using System;
using System.Collections.Generic;
using System.Text;

namespace WhipStat.Models.PDC
{
    public class Employer
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/lobbyist-employment-registrations
        public string id { get; set; }
        public string report_number { get; set; }
        public string lobbyist_id { get; set; }
        public string lobbyist_name { get; set; }
        public string employer_id { get; set; } 
        public string employer_name { get; set; }
        public string contractor_id { get; set; }
        public string contractor_name { get; set; } 
        public short employment_year { get; set; }
    }
}
