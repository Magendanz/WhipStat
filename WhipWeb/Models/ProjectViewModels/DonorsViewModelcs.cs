using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WhipStat.Data;

namespace WhipStat.Models.ProjectViewModels
{
    public class DonorsViewModel
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int District { get; set; }
        [Required]
        public string Party { get; set; }
        public List<SelectListItem> Districts { get; set; }
        public List<SelectListItem> Parties { get; set; }
    }
}
