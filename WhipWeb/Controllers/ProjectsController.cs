using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhipStat.Data;
using WhipStat.Models.ProjectViewModels;

namespace WhipStat.Controllers
{
    public class ProjectsController : Controller
    {
        VoterDbContext VoterDb = new VoterDbContext();
        DonorDbContext DonorDb = new DonorDbContext();
        ResultDbContext ResultDb = new ResultDbContext();

        SelectListItem SelectPrompt = new SelectListItem { Value = "0", Text = "Select...", Selected = true, Disabled = true };

    public IActionResult Index()
        {
            return View();
        }

        public IActionResult Voters()
        {
            return View(new VotersViewModel() { Districts = GetDistrictList() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Voters(VotersViewModel model)
        {
            var precincts = VoterDb.GetPrecincts(model.District);

            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            if (model.Precinct < Int32.MaxValue)
            {
                // A single precinct list is served up directly
                TempData["FileName"] = $"{precincts[model.Precinct]}.txt";
                TempData["ContentType"] = "text/tab-separated-values";
                TempData["Bytes"] = Encoding.UTF8.GetBytes(VoterDb.GetVoters(model.District, model.Precinct));
            }
            else
            {
                // Build a zip file for all precinct lists
                var stream = new MemoryStream();
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in precincts)
                    {
                        var entry = zip.CreateEntry($"{item.Value}.txt");
                        using (var writer = new StreamWriter(entry.Open()))
                            writer.Write(VoterDb.GetVoters(model.District, item.Key));
                    }
                }

                // Save FileStreamResult arguments for subsequent GET
                TempData["FileName"] = $"LD{model.District}.zip";
                TempData["ContentType"] = "application/zip";
                TempData["Bytes"] = stream.ToArray();
            }

            // Serve up the download page and deliver file
            return View("Download");
        }

        private List<SelectListItem> GetDistrictList()
        {
            var list = new List<SelectListItem> { SelectPrompt };

            for (var i = 1; i < 50; ++i)
                list.Add(new SelectListItem { Value = i.ToString(), Text = $"LD {i}" });

            return list;
        }

        [HttpGet]
        public List<SelectListItem> GetPrecinctList(int district)
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var item in VoterDb.GetPrecincts(district).OrderBy(p => p.Value))
                list.Add(new SelectListItem { Value = item.Key.ToString(), Text = item.Value });

            // Add one more special entry to allow all precincts to be download as a zip archive
            list.Add(new SelectListItem { Value = Int32.MaxValue.ToString(), Text = "All Precincts" });

            return list;
        }

        public IActionResult Donors()
        {
            return View(new DonorsViewModel() { Districts = GetDistrictList(), Parties = GetPartyList() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Donors(DonorsViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            TempData["FileName"] = $"LD{model.District}-{model.Party}.txt";
            TempData["ContentType"] = "text/tab-separated-values";
            TempData["Bytes"] = Encoding.UTF8.GetBytes(DonorDb.GetDonors(model.Party, GetZipCodes(model.District)));

            // Serve up the download page and deliver file
            return View("Download");
        }

        private List<SelectListItem> GetPartyList()
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var item in DonorDb.Parties.Where(p => p.Inactive == null).OrderBy(p => p.Name))
                list.Add(new SelectListItem { Value = item.Code, Text = item.Name });

            return list;
        }

        private string GetZipCodes(int distict)
        {
            return VoterDb.ZipCodes.Where(z => z.LegislativeDistrict == distict).Select(z => z.ZipCodes).First();
        }

        public IActionResult Results()
        {
            return View(new ResultsViewModel() { Districts = GetKCDistrictList(), Years = GetYearList(), Elections = GetElectionList() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Results(ResultsViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            TempData["FileName"] = $"LD{model.District}-{model.Race} ({model.Year} {model.Election}).txt";
            TempData["ContentType"] = "text/tab-separated-values";
            TempData["Bytes"] = Encoding.UTF8.GetBytes(ResultDb.GetResults(model.District, model.Year, model.Election, model.Race, model.Entry));

            // Serve up the download page and deliver file
            return View("Download");
        }

        private List<SelectListItem> GetKCDistrictList()
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var i in ResultDb.Results.Where(i => i.LEG != null).Select(i => i.LEG).OrderBy(i => i).Distinct())
                list.Add(new SelectListItem { Value = i.ToString(), Text = $"LD {i}" });

            return list;
        }

        private List<SelectListItem> GetYearList()
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var i in ResultDb.Results.Where(i => i.Year != null).Select(i => i.Year).OrderBy(i => i).Distinct())
                list.Add(new SelectListItem { Value = $"{i}", Text = $"{i}" });

            return list;
        }

        private List<SelectListItem> GetElectionList()
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var i in ResultDb.Results.Where(i => i.Election != null).Select(i => i.Election).OrderBy(i => i).Distinct())
                list.Add(new SelectListItem { Value = i, Text = i });

            return list;
        }

        [HttpGet]
        public List<SelectListItem> GetRaceList(int district, int year, string election)
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var i in ResultDb.Results.Where(i => i.LEG == district && i.Year == year && i.Election == election && i.Race != null)
                .Select(i => i.Race).OrderBy(i => i).Distinct())
                list.Add(new SelectListItem { Value = i, Text = i });

            return list;
        }

        [HttpGet]
        public List<SelectListItem> GetEntryList(int district, int year, string election, string race)
        {
            var list = new List<SelectListItem> { SelectPrompt };

            foreach (var i in ResultDb.Results.Where(i => i.LEG == district && i.Year == year && i.Election == election && i.Race == race && i.CounterType != null)
                .Select(i => i.CounterType).OrderBy(i => i).Distinct())
                list.Add(new SelectListItem { Value = i, Text = i });

            return list;
        }

        public IActionResult Download()
        {
            if (TempData.Count() > 0)
            {
                // Download file stream to client
                return File((byte[])TempData["Bytes"], (string)TempData["ContentType"], (string)TempData["FileName"]);
            }
            else
            {
                // We lost our TempData!
                return View("Error");
            }
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}