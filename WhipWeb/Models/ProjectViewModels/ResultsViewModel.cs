using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.ProjectViewModels
{
    public class ResultsViewModel
    {
        [Required]
        [Range(1, 49)]
        public int District { get; set; }
        [Required]
        [Range(2000,2099)]
        public int Year { get; set; }
        [Required]
        public string Election { get; set; }
        [Required]
        public string Race { get; set; }
        [Required]
        public string Entry { get; set; }
        public List<SelectListItem> Districts { get; set; }
        public List<SelectListItem> Years { get; set; }
        public List<SelectListItem> Elections { get; set; }
    }
}
