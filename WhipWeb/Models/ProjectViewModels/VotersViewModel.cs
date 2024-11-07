using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.ProjectViewModels
{
    public class VotersViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int District { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Precinct { get; set; }
        public List<SelectListItem> Districts { get; set; }
        public List<SelectListItem> Precincts { get; set; }

		[Required]
        public string Last { get; set; }
        public string First { get; set; }
        public string City { get; set; }

        public bool Inactive { get; set; }
    }
}