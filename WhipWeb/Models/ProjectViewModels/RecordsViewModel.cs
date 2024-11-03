using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using System;
using System.Collections.Generic;

namespace WhipStat.Models.ProjectViewModels
{
    public class RecordsViewModel
    {
        public string Area { get; set; }
        public string Chamber { get; set; }
        public short From { get; set; }
        public short To { get; set; }
        public List<SelectListItem> Areas { get; set; }
        public List<SelectListItem> Chambers { get; set; }
        public List<SelectListItem> OddYears { get; set; }
        public List<SelectListItem> EvenYears { get; set; }
    }
}
