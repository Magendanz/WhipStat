using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace WhipStat.Models.ProjectViewModels
{
    public class FidelityViewModel
    {
        public string Chamber { get; set; }
        public short From { get; set; }
        public short To { get; set; }

        public List<SelectListItem> Chambers { get; set; }
        public List<SelectListItem> OddYears { get; set; }
        public List<SelectListItem> EvenYears { get; set; }
    }
}