using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using WhipStat.Data;
using WhipStat.Helpers;
using WhipStat.Models.LegTech;
using WhipStat.Models.Fundraising;
using WhipStat.Models.ProjectViewModels;
using System.Security.Cryptography;

namespace WhipStat.Controllers
{
    public class ProjectsController : Controller
    {
        const string SessionKeyFileName = "_FileName";
        const string SessionKeyContentType = "_ContentType";
        const string SessionKeyContents = "_Contents";

        readonly VoterDbContext VoterDb = new VoterDbContext();
        readonly DonorDbContext DonorDb = new DonorDbContext();
        readonly ResultDbContext ResultDb = new ResultDbContext();
        readonly RecordDbContext RecordDb = new RecordDbContext();

        readonly SelectListItem SelectPrompt = new SelectListItem { Value = "0", Text = "Select...", Selected = true, Disabled = true };
        readonly string MemberTooltipHtml = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\html\MemberTooltip.html"));
        readonly string DonorTooltipHtml = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\html\DonorTooltip.html"));
        readonly string DonorPointStyle = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\html\DonorPointStyle.html"));

        private readonly IWebHostEnvironment _hostingEnvironment;
        public ProjectsController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Voters()
        {
            return View(new VotersViewModel()
            {
                Districts = GetDistrictList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisableRequestSizeLimit]
        public IActionResult Voters(VotersViewModel model)
        {
            var precincts = VoterDb.GetPrecincts(model.District);

            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            if (model.Precinct < Int32.MaxValue)
            {
                // A single precinct list is served up directly
                HttpContext.Session.SetString(SessionKeyFileName, $"{precincts[model.Precinct]}.tsv");
                HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
                HttpContext.Session.SetString(SessionKeyContents, VoterDb.GetVoters(model.District, model.Precinct));
            }
            else
            {
                // Build a zip file for all precinct lists
                var stream = new MemoryStream();
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (var item in precincts)
                    {
                        var entry = zip.CreateEntry($"{item.Value}.tsv");
                        using var writer = new StreamWriter(entry.Open());
                        writer.Write(VoterDb.GetVoters(model.District, item.Key));
                    }
                }

                // Save FileStreamResult arguments for subsequent GET
                HttpContext.Session.SetString(SessionKeyFileName, $"LD{model.District}.zip");
                HttpContext.Session.SetString(SessionKeyContentType, "application/zip");
                HttpContext.Session.SetString(SessionKeyContents, Encoding.UTF8.GetString(stream.ToArray()));
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
        [DisableRequestSizeLimit]
        public IActionResult Donors(DonorsViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            HttpContext.Session.SetString(SessionKeyFileName, $"LD{model.District}-{model.Party}.tsv");
            HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
            HttpContext.Session.SetString(SessionKeyContents, DonorDb.GetDonors(model.Party, GetZipCodes(model.District)));

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
            return View(new ResultsViewModel()
            {
                Districts = GetKCDistrictList(),
                Years = GetYearList(),
                Elections = GetElectionList()
            });
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Results(ResultsViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            HttpContext.Session.SetString(SessionKeyFileName, $"LD{model.District}-{model.Race} ({model.Year} {model.Election}).tsv");
            HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
            HttpContext.Session.SetString(SessionKeyContents, ResultDb.GetResults(model.District, model.Year, model.Election, model.Race, model.Entry));

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
            return View(new RecordsViewModel()
            {
                Areas = GetPolicyAreaList(),
                OddYears = GetYearList(1991, 2),
                EvenYears = GetYearList(1992, 2),
                Chambers = GetChamberList(),
                From = 2013,
                To = 2022
            });
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Records(RecordsViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            var area = model.Area == "0" ? "All Policy Areas" : RecordDb.PolicyAreas.Single(i => i.Id == Convert.ToInt32(model.Area)).Name;
            var chamber = model.Chamber == "0" ? "Both Chambers" : model.Chamber;
            HttpContext.Session.SetString(SessionKeyFileName, $"Partisan Leaderboard - {area}, {chamber} ({model.From}-{model.To % 100:N2}).tsv");
            HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
            HttpContext.Session.SetString(SessionKeyContents, RecordDb.GetPartisanLeaderboard(Convert.ToInt16(model.Area), model.Chamber, model.From, model.To));

            // Serve up the download page and deliver file
            return View("Download");
        }

        private List<SelectListItem> GetPolicyAreaList()
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "0", Text = "All Policy Areas", Selected = true } };

            foreach (var area in RecordDb.PolicyAreas)
                list.Add(new SelectListItem { Value = area.Id.ToString(), Text = area.Name });

            return list;
        }

        private static List<SelectListItem> GetYearList(int start, short increment = 1)
        {
            var list = new List<SelectListItem>();

            // This is the end of the current biennium
            var end = (DateTime.Today.Year + 1) / 2 * 2;

            for (var year = start; year <= end; year += increment)
                list.Add(new SelectListItem { Value = year.ToString(), Text = year.ToString() });

            return list;
        }

        private static List<SelectListItem> GetChamberList()
            => new List<SelectListItem> {
                new SelectListItem { Value = "0", Text = "Both Chambers", Selected = true },
                new SelectListItem { Value = "House", Text = "House of Representatives" },
                new SelectListItem { Value = "Senate", Text = "Senate" }
            };

        [HttpGet]
        public DataTable GetMemberDataTable(string chamber, short area, short from, short to)
        {
            var points = new List<Point>();
            var members = RecordDb.Members.Where(i => chamber == "0" || i.Agency == chamber).OrderBy(i => i.Party).ToList();

            foreach (var member in members)
            {
                var scores = RecordDb.Scores.Where(i => i.MemberId == member.Id && i.PolicyArea == area && i.Year >= from && i.Year <= to).ToList();
                var total = scores.Sum(i => i.Total);
                var count = scores.Sum(i => i.Count);
                if (count > 10)
                {
                    var score = total / count;
                    points.Add(new Point { x = score, y = Stack(score, points), Label = GetTooltip(member, "Partisan bias", score), Series = member.Party });
                }
            }

            return ConvertMemberPoints(points);
        }

        private static double Stack(double x, List<Point> points)
        {
            const double dx = 1.0;
            const double dy = 1.0;
            double y = 0;

            while (points.Exists(p => Math.Abs(p.x - x) < dx && Math.Abs(p.y - y) < dy))
                y += dy;

            return y;
        }

        private static DataTable ConvertMemberPoints(List<Point> points)
        {
            var dt = new DataTable
            {
                cols = new List<ColInfo> { new ColInfo { label = "% bias", type = "number" } },
                rows = new List<DataPointSet>(),
                p = new Dictionary<string, string>()
            };

            var series = points.Select(i => i.Series).Distinct().OrderBy(i => i).ToList();
            var n = series.Count * 2 + 1;
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

        [HttpGet]
        public ActionResult Portrait(int id)
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "thumbnails", id + ".jpg");
            if (!System.IO.File.Exists(path))
                path = Path.Combine(_hostingEnvironment.WebRootPath, "thumbnails", "placeholder.jpg");

            return base.PhysicalFile(path, "image/jpeg");
        }

        private static double GetMedian(List<Point> points, string series)
        {
            var list = points.Where(i => i.Series == series).OrderBy(i => i.x).Select(i => i.x);
            int index = list.Count() / 2;
            if (list.Count() % 2 == 0)
                return (list.ElementAt(index) + list.ElementAt(index - 1)) / 2;
            else
                return list.ElementAt(index);
        }

        public IActionResult OrgDonors()
        {
            return View(new OrgDonorsViewModel()
            {
                OddYears = GetYearList(2007),
                EvenYears = GetYearList(2008),
                Jurisdictions = GetJurisdictionList(),
                From = "2007",
                To = "2022"
            });
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult OrgDonors(OrgDonorsViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            HttpContext.Session.SetString(SessionKeyFileName, $"Donor Leaderboard - {model.Jurisdiction} Races ({model.From}-{model.To[^2..]}).tsv");
            HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
            HttpContext.Session.SetString(SessionKeyContents, DonorDb.GetLobbyLeaders(model.Jurisdiction, Convert.ToInt16(model.From), Convert.ToInt16(model.To)));

            // Serve up the download page and deliver file
            return View("Download");
        }

        private static List<SelectListItem> GetJurisdictionList()
        {
            return new List<SelectListItem> {
                new SelectListItem { Value = "Legislative", Text = "Legislative" },
                new SelectListItem { Value = "Statewide", Text = "Statewide" },
            };
        }

        [HttpGet]
        public DataTable GetDonorDataTable(string jurisdiction, short from, short to)
        {
            var points = new List<Point>();
            var donors = DonorDb.Donors.Where(i => i.Aggregate > 10000).ToList();
            foreach (var donor in donors)
            {
                var subtotals = DonorDb.Subtotals.Where(i => i.DonorId == donor.Id && i.Jurisdiction == jurisdiction && (i.Year >= from && i.Year <= to)).ToList();
                if (subtotals.Count > 0)
                {
                    var tallies = subtotals.Where(i => i.DonorId == donor.Id).ToList();
                    var total = tallies.Sum(i => i.Total);
                    var bias = (tallies.Sum(i => i.Republican) - tallies.Sum(i => i.Democrat)) / total;
                    var winning = (double) tallies.Sum(i => i.Wins) / tallies.Sum(i => i.Campaigns);
                    points.Add(new Point { x = bias, y = total, Label = GetTooltip(donor, total, bias, winning) });
                }
            }

            return ConvertDonorPoints(points);
        }

        private DataTable ConvertDonorPoints(List<Point> points)
        {
           var dt = new DataTable
            {
                cols = new List<ColInfo> {
                    new ColInfo { label = "% bias", type = "number" },
                    new ColInfo { label = "Contributions", type = "number" },
                    new ColInfo { role = "style", type = "string" },
                    new ColInfo { role = "tooltip", type = "string", p = new Dictionary<string, string> { { "html", "true" } } }
                },
                rows = new List<DataPointSet>(),
                p = new Dictionary<string, string>()
            };

            foreach (var point in points)
            {
                dt.rows.Add(new DataPointSet { c = new DataPoint[] {
                    new DataPoint { v = point.x.ToString() },
                    new DataPoint { v = point.y.ToString() },
                    new DataPoint { v = GetPointStyle(point.x) },
                    new DataPoint { v = point.Label } }
                });
            }

            return dt;
        }

        private string GetTooltip(Models.LWS.Member member, string label, double score) => string.Format(MemberTooltipHtml, member.Id, member.Name, member.Agency, member.District, member.Party, label, score);
        private string GetTooltip(Donor donor, double total, double bias, double winning) => String.Format(DonorTooltipHtml, donor.Name, total, bias, winning);
        private string GetPointStyle(double value) => String.Format(DonorPointStyle, ColorUtilities.GetIndexedColorOnGradient(value + 0.5, "#4285F4", "#DB4437"));

        public IActionResult Advocacy()
        {
            return View(new AdvocacyViewModel()
            {
                OddYears = GetYearList(2013, 2),
                EvenYears = GetYearList(2014, 2),
                Chambers = GetChamberList(),
                From = 2013,
                To = 2022
            });
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Advocacy(AdvocacyViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            var chamber = model.Chamber == "0" ? "Member" : model.Chamber;
            var orgId = RecordDb.Organizations.First(i => i.Name == model.Organization).Id;
            HttpContext.Session.SetString(SessionKeyFileName, $"{chamber} Correlation - {model.Organization} ({model.From}-{model.To}).tsv");
            HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
            HttpContext.Session.SetString(SessionKeyContents, RecordDb.GetMemberLeaderboard(orgId, model.Chamber, model.From, model.To));

            // Serve up the download page and deliver file
            return View("Download");
        }

        [HttpGet]
        public string[] GetOrganizations()
        {
            return RecordDb.Organizations
                .Join(RecordDb.Testimonies, i => i.Id, j => j.OrgId, (i, j) => i)
                .Distinct().Select(i => i.Name).OrderBy(i => i).ToArray();
        }

        [HttpGet]
        public DataTable GetBillRatings(string organization, short from, short to)
        {
            var dt = new DataTable
            {
                cols = new List<ColInfo> {
                    new ColInfo { label = "Biennium", type = "string" },
                    new ColInfo { label = "Bill", type = "string" },
                    new ColInfo { label = "Title", type = "string" },
                    new ColInfo { label = "Count", type = "number" },
                    new ColInfo { label = "Testifying", type = "number" },
                    new ColInfo { label = "Support", type = "number" }
                },
                rows = new List<DataPointSet>(),
                p = new Dictionary<string, string>()
            };

            var org = RecordDb.Organizations.FirstOrDefault(i => i.Name == organization);
            if (org != null)
            {
                var records = RecordDb.AdvocacyRecords.Where(i => i.Id == org.Id && i.Biennium.CompareTo($"{from}") >= 0 && i.Biennium.CompareTo($"{to}") <= 0)
                    .Join(RecordDb.Bills, i => new { Y = i.Biennium, N = i.BillNumber }, j => new { Y = j.Biennium, N = j.BillNumber }, (i, j) => new { i.Biennium, i.BillNumber, j.AbbrTitle, i.Sponsor, i.Votes, i.Support })
                    .Distinct().OrderBy(i => i.Biennium).ThenBy(i => i.BillNumber).ToList();

                foreach (var record in records)
                    dt.rows.Add(new DataPointSet
                    {
                        c = new DataPoint[] {
                    new DataPoint { v = record.Biennium },
                    new DataPoint { v = record.BillNumber.ToString() },
                    new DataPoint { v = record.AbbrTitle },
                    new DataPoint { v = record.Votes.ToString() },
                    new DataPoint { v = record.Sponsor.ToString() },
                    new DataPoint { v = $"{(double)record.Support / record.Votes:P0}" } }
                    });
            }

            return dt;
        }

        [HttpGet]
        public DataTable GetVotingRecordDataTable(string organization, string chamber, short from, short to)
        {
            var points = new List<Point>();
            var org = RecordDb.Organizations.FirstOrDefault(i => i.Name == organization);
            if (org != null)
            {
                var members = RecordDb.Members.Where(i => chamber == "0" || i.Agency == chamber).OrderBy(i => i.Party).ToList();
                var records = RecordDb.AdvocacyRecords.Where(i => i.Id == org.Id && i.Biennium.CompareTo($"{from}") > 0 && i.Biennium.CompareTo($"{to}") < 0)
                    .Join(RecordDb.VotingRecords, i => new { Y = i.Biennium, N = i.BillNumber }, j => new { Y = j.Biennium, N = j.BillNumber },
                    (i, j) => new { j.Id, j.Sponsor, j.Votes, j.Support, Count = i.Votes, Testify = i.Sponsor, Favor = i.Support }).ToList();

                foreach (var member in members)
                {
                    var tally = records.Where(i => i.Id == member.Id);
                    int n = tally.Count();
                    if (n > 0)
                    {
                        var score = tally.Average(i => 100.0 * i.Support / i.Votes * i.Favor / i.Count);   // Quick-n-dirty correlation!
                        points.Add(new Point { x = score, y = Stack(score, points), Label = GetTooltip(member, "Correlation", score), Series = member.Party });
                    }
                }
            }

            return ConvertMemberPoints(points);
        }

        public IActionResult Download()
        {
            try
            {
                // Download file stream to client
                var fileDownloadName = HttpContext.Session.GetString(SessionKeyFileName);
                var contentType = HttpContext.Session.GetString(SessionKeyContentType);
                var fileContents = Encoding.UTF8.GetBytes(HttpContext.Session.GetString(SessionKeyContents));
                return File(fileContents, contentType, fileDownloadName);
            }
            catch
            {
                // We lost our session data!
                return View("Error");
            }
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}