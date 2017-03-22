using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WhipStat.Data;

namespace WhipStat.Models.ProjectViewModels
{
    public class VotersViewModel
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int District { get; set; }
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Precinct { get; set; }
        public List<SelectListItem> Districts { get; set; }
        public List<SelectListItem> Precincts { get; set; }
    }
}
