using System;

namespace WhipStat.Models.PDC
{
    public class Expenditure : Transaction
    {
        // Metadata: https://www.pdc.wa.gov/browse/open-data/expenditures-candidates-and-political-committees

        public DateTime expenditure_date { get; set; }
        public string itemized_or_non_itemized { get; set; }
        public string recipient_name { get; set; }
        public string recipient_address { get; set; }
        public string recipient_city { get; set; }
        public string recipient_state { get; set; }
        public string recipient_zip { get; set; }

        public override string Name => recipient_name;
        public override string Address => recipient_address;
        public override string City => recipient_city;
        public override string State => recipient_state;
        public override string Zip => recipient_zip;
    }
}
