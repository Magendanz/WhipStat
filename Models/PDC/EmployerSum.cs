using System;
using System.Collections.Generic;
using System.Text;

namespace WhipStat.Models.PDC
{
    public class EmployerSum
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/lobbyist-employers-summary
        public string id { get; set; }
        public short year { get; set; }
        public string Employer_Name { get; set; }
        public string Employer_Email { get; set; }
        public string Employer_Phone { get; set; } 
        public string Employer_Address { get; set; }
        public string Employer_City { get; set; }
        public string Employer_State { get; set; } 
        public string Employer_Zip { get; set; } 
        public string Employer_Country { get; set; }
        public decimal compensation { get; set; }
        public decimal expenditures { get; set; }
        public decimal agg_contrib { get; set; }
        public decimal ballot_prop { get; set; }
        public decimal entertain { get; set; }
        public decimal vendor { get; set; }
        public decimal expert_retain { get; set; }
        public decimal inform_material { get; set; }
        public decimal lobbying_comm { get; set; }
        public decimal ie_in_support { get; set; }
        public decimal itemized_exp { get; set; }
        public decimal other_l3_exp { get; set; }
        public decimal political { get; set; }
        public decimal corr_compensation { get; set; }
        public decimal corr_expend { get; set; }
        public decimal total_exp { get; set; }
        public string l3_nid { get; set; }
        public string employer_nid { get; set; }
    }
}
