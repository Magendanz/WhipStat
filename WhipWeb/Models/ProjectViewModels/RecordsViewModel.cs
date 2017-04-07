using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WhipStat.Data;

namespace WhipStat.Models.ProjectViewModels
{
    public class RecordsViewModel
    {
        public string Area { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Chamber { get; set; }
        public List<SelectListItem> Areas { get; set; }
        public List<SelectListItem> OddYears { get; set; }
        public List<SelectListItem> EvenYears { get; set; }
        public List<SelectListItem> Chambers { get; set; }
    }
}
