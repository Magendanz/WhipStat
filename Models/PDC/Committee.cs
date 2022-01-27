using System;
using System.Collections.Generic;
using System.Text;

namespace WhipStat.Models.PDC
{
    public class Committee
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/candidate-and-committee-registrations
        public string id { get; set; }
        public string report_number { get; set; }
        public string origin { get; set; }
        public string filer_id { get; set; }
        public string filer_type { get; set; }
        public DateTime receipt_date { get; set; }
        public short election_year { get; set; }
        public string candidate_committee_status { get; set; }
        public string filer_name { get; set; }
        public string first_name { get; set; }
        public string middle_initial { get; set; }
        public string last_name { get; set; }
        public string committee_acronym { get; set; }
        public string committee_address { get; set; }
        public string committee_city { get; set; }
        public string committee_county { get; set; }
        public string committee_state { get; set; }
        public string committee_zip { get; set; }
        public string committee_email { get; set; }
        public string candidate_email { get; set; }
        public string candidate_committee_phone { get; set; }
        public string committee_fax { get; set; }
        public string office { get; set; }
        public string office_code { get; set; }
        public string jurisdiction { get; set; }
        public string jurisdiction_code { get; set; }
        public string jurisdiction_county { get; set; }
        public string jurisdiction_type { get; set; }
        public string position { get; set; }
        public string party_code { get; set; }
        public string party { get; set; }
        public DateTime election_date { get; set; }
        public string reporting_option { get; set; }
        public string primary_election_status { get; set; }
        public string general_election_status { get; set; }
        public string election_status { get; set; }
        public string political_committee_type { get; set; }
        public string party_committee { get; set; }
        public string bonafide_type { get; set; }
        public string party_account_type { get; set; }
        public string initiative_type { get; set; }
        public string ballot_number { get; set; }
        public string for_or_against { get; set; }
        public string PAC_type { get; set; }
        public string treasurer_first_name { get; set; }
        public string treasurer_last_name { get; set; }
        public string treasurer_address { get; set; }
        public string treasurer_city { get; set; }
        public string treasurer_county { get; set; }
        public string treasurer_state { get; set; }
        public string treasurer_zip { get; set; }
        public string treasurer_phone { get; set; }
    }
}
