using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using System;
using System.Collections.Generic;

namespace WhipStat.Models.ProjectViewModels
{
    public class OrgDonorsViewModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Jurisdiction { get; set; }
        public List<SelectListItem> OddYears { get; set; }
        public List<SelectListItem> EvenYears { get; set; }
        public List<SelectListItem> Jurisdictions { get; set; }
    }
}
