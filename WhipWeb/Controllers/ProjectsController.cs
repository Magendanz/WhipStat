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
using WhipStat.Models.LegTech;
using WhipStat.Models.ProjectViewModels;

namespace WhipStat.Controllers
{
    public class ProjectsController : Controller
    {
        VoterDbContext VoterDb = new VoterDbContext();
        DonorDbContext DonorDb = new DonorDbContext();
        ResultDbContext ResultDb = new ResultDbContext();
        RecordDbContext RecordDb = new RecordDbContext();

        SelectListItem SelectPrompt = new SelectListItem { Value = "0", Text = "Select...", Selected = true, Disabled = true };
        String TooltipHtml = System.IO.File.ReadAllText(@"Views\Projects\Tooltip.html");

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
            return VoterDb.ZipCodes.First(z => z.LegislativeDistrict == distict).ZipCodes;
        }

        public IActionResult Results()
        {
            return View(new ResultsViewModel() { Districts = GetKCDistrictList(), Years = GetYearList(), Elections = GetElectionList() });
        }

        [HttpPost]
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

        public IActionResult Records()
        {
            return View(new RecordsViewModel() { Areas = GetPolicyAreaList(), OddYears = GetYearList(2003), EvenYears = GetYearList(2004),
                Chambers = GetChamberList(), To = "2016" });
        }

        private List<SelectListItem> GetPolicyAreaList()
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "0", Text = "All Policy Areas", Selected = true } };

            foreach (var area in RecordDb.PolicyAreas)
                list.Add(new SelectListItem { Value = area.Id.ToString(), Text = area.Name });

            return list;
        }

        private List<SelectListItem> GetYearList(short start)
        {
            var list = new List<SelectListItem>();

            for (short year = start; year < 2017; year += 2)
                list.Add(new SelectListItem { Value = year.ToString(), Text = year.ToString() });

            return list;
        }

        private List<SelectListItem> GetChamberList()
        {
            return new List<SelectListItem> {
                new SelectListItem { Value = "0", Text = "Both Chambers", Selected = true },
                new SelectListItem { Value = "House", Text = "House of Representatives" },
                new SelectListItem { Value = "Senate", Text = "Senate" },

            };
        }

        [HttpGet]
        public DataTable GetDataTable(string chamber, int area, int from, int to)
        {
            var points = new List<Point>();
            var members = RecordDb.Members.Where(i => chamber == "0" || i.Agency == chamber).OrderBy(i => i.Party).ToList();

            foreach (var member in members)
            {
                var scores = RecordDb.Scores.Where(i => i.Member_Id == member.Id && i.PolicyArea == area && i.Year >= from && i.Year <= to).ToList();
                var total = scores.Sum(i => i.Total);
                var count = scores.Sum(i => i.Count);
                if (count > 10)
                {
                    var score = total / count;
                    points.Add(new Point { x = score, y = Stack(score, points), Label = GetTooltip(member, score), Series = member.Party });
                }
            }

            return ConvertPoints(points);
        }

        private double Stack(double x, List<Point> points)
        {
            const double dx = 1.0;
            const double dy = 1.0;
            double y = 0;

            while (points.Exists(p => Math.Abs(p.x - x) < dx && Math.Abs(p.y - y) < dy))
                y += dy;

            return y;
        }

        private string GetTooltip(Member member, double score)
        {
            return String.Format(TooltipHtml, member.Name, member.LastName, member.Agency, member.District, member.Party, score);
        }

        private DataTable ConvertPoints(List<Point> points)
        {
            var dt = new DataTable
            {
                cols = new List<ColInfo> { new ColInfo { label = "% bias", type = "number" } },
                rows = new List<DataPointSet>(),
                p = new Dictionary<string, string>()
            };

            var series = points.GroupBy(p => p.Series).Select(p => p.First().Series).ToList();
            var n = series.Count() * 2 + 1;
            foreach (var s in series)
            {
                dt.cols.Add(new ColInfo { label = s, type = "number", p = new Dictionary<string, string> { { "median", GetMedian(points, s).ToString() } } });
                dt.cols.Add(new ColInfo { role = "tooltip", type = "string", p = new Dictionary<string, string> { { "html", "true" } } });
            }

            foreach (var point in points)
            {
                var dps = new DataPointSet { c = new DataPoint[n] };
                var i = series.FindIndex(p => p == point.Series);
                dps.c[0] = new DataPoint { v = point.x.ToString() };
                dps.c[i * 2 + 1] = new DataPoint { v = point.y.ToString() };
                dps.c[i * 2 + 2] = new DataPoint { v = point.Label };
                dt.rows.Add(dps);
            }

            return dt;
        }

        private double GetMedian(List<Point> points, string series)
        {
            var list = points.Where(i => i.Series == series).OrderBy(i => i.x).Select(i => i.x);
            int index = list.Count() / 2;
            if (list.Count() % 2 == 0)
                return (list.ElementAt(index) + list.ElementAt(index - 1)) / 2;
            else
                return list.ElementAt(index);
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