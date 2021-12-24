using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WhipStat.Data;

namespace WhipStat.Models.ProjectViewModels
{
    public class RatingsViewModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public int Organization { get; set; }

        public List<SelectListItem> OddYears { get; set; }
        public List<SelectListItem> EvenYears { get; set; }
        public List<SelectListItem> Organizations { get; set; }
        public List<Rating> Leaderboard { get; set; }
    }

    public class Rating
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }
}