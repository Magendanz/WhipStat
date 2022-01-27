using System;

namespace WhipStat.Models.PDC
{
    public class Contribution : Transaction
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/contributions-candidates-and-political-committees
        public DateTime receipt_date { get; set; }
        public string cash_or_in_kind { get; set; }
        public string primary_general { get; set; }
        public string contributor_category { get; set; }
        public string contributor_name { get; set; }
        public string contributor_address { get; set; }
        public string contributor_city { get; set; }
        public string contributor_state { get; set; }
        public string contributor_zip { get; set; }
        public string contributor_occupation { get; set; }
        public string contributor_employer_name { get; set; }
        public string contributor_employer_city { get; set; }
        public string contributor_employer_state { get; set; }

        public override string Name => contributor_name;
        public override string Address => contributor_address;
        public override string City => contributor_city;
        public override string State => contributor_state;
        public override string Zip => contributor_zip;
    }
}
