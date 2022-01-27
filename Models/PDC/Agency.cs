using System;
using System.Collections.Generic;
using System.Text;

namespace WhipStat.Models.PDC
{
    public class Agency
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/lobbyist-summary
        public string id { get; set; }
        public int year { get; set; }
        public string filer_id { get; set; }
        public string lobbyist_name { get; set; }
        public string lobbyist_email { get; set; }
        public string lobbyist_phone { get; set; }
        public string firm_address { get; set; }
        public string firm_city { get; set; }
        public string firm_state { get; set; }
        public string firm_zip { get; set; }
        public string firm_country { get; set; }
        public decimal contributions { get; set; }
        public decimal total_expenses { get; set; }
    }
}
