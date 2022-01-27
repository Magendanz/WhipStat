using System;
using System.Collections.Generic;
using System.Text;

namespace WhipStat.Models.PDC
{
    public class Agent
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/lobbyist-agents-0
        public string id { get; set; }
        public string filer_id { get; set; }
        public string lobbyist_firm_name { get; set; }
        public string lobbyist_phone { get; set; }
        public string lobbyist_email { get; set; }
        public string lobbyist_address { get; set; }
        public string employers { get; set; }
        public string agent_name { get; set; }
        public string agent_bio { get; set; }
        public string agent_pic_url { get; set; }
        public short employment_year { get; set; }
        //public string lobbyist_firm_url { get; set; }
    }
}
