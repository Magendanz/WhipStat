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

using WhipStat.DataAccess;
using WhipStat.Helpers;
using WhipStat.Models.LegTech;
using WhipStat.Models.Fundraising;
using WhipStat.Models.ProjectViewModels;

using WhipWeb.Models;

namespace WhipStat.Controllers
{
    public class ProjectsController : Controller
    {
        const string SessionKeyFileName = "_FileName";
        const string SessionKeyContentType = "_ContentType";
        const string SessionKeyContents = "_Contents";

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
            var precincts = TrfAccess.GetPrecincts($"Legislative District {model.District}");

            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            if (model.Precinct < int.MaxValue)
            {
                // A single precinct list is served up directly
                HttpContext.Session.SetString(SessionKeyFileName, $"LD {model.District} - {precincts[model.Precinct]}.csv");
                HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
                HttpContext.Session.SetString(SessionKeyContents, TrfAccess.GetVoters(model.District, model.Precinct, model.Inactive));
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
                        writer.Write(TrfAccess.GetVoters(model.District, item.Key));
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
                list.Add(new SelectListItem { Value = i.ToString(), Text = $"Legislative District {i}" });

            return list;
        }

        [HttpGet]
        public List<SelectListItem> GetPrecinctList(int district)
        {
            var precincts = TrfAccess.GetPrecincts($"Legislative District {district}");
            var list = precincts.Select(i => new SelectListItem { Value = i.Key.ToString(), Text = i.Value }).OrderBy(j => j.Text).ToList();

            // Add one more special entry to allow all precincts to be download as a zip archive
            list.Add(new SelectListItem { Value = int.MaxValue.ToString(), Text = "All Precincts" });

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
            HttpContext.Session.SetString(SessionKeyContents, DonorDb.GetDonors(model.Party));

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
                To = 2024
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
                    points.Add(new Point { x = score, y = Stack(score, points), Id = member.Id, Series = member.Party, Label = GetTooltip(member, "Partisan bias", score) });
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
                dps.c[i * 2 + 1] = new DataPoint { v = point.y.ToString(), p = new Dictionary<string, string> { { "id", point.Id.ToString() } } };
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
                To = "2024"
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
        private string GetTooltip(Donor donor, double total, double bias, double winning) => string.Format(DonorTooltipHtml, donor.Name, total, bias, winning);
        private string GetPointStyle(double value) => string.Format(DonorPointStyle, ColorUtilities.GetIndexedColorOnGradient(value + 0.5, "#4285F4", "#DB4437"));

        public IActionResult Advocacy()
        {
            return View(new AdvocacyViewModel()
            {
                OddYears = GetYearList(2013, 2),
                EvenYears = GetYearList(2014, 2),
                Chambers = GetChamberList(),
                From = 2013,
                To = 2024
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
                        points.Add(new Point { x = score, y = Stack(score, points), Id = member.Id, Series = member.Party, Label = GetTooltip(member, "Correlation", score) });
                    }
                }
            }

            return ConvertMemberPoints(points);
        }

        public IActionResult Fidelity()
        {
            return View(new FidelityViewModel()
            {
                OddYears = GetYearList(2013, 2),
                EvenYears = GetYearList(2014, 2),
                Chambers = GetChamberList(),
                From = 2013,
                To = 2024
            });
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Fidelity(FidelityViewModel model)
        {
            // Note: We're implementing the POST-REDIRECT-GET (PRG) design pattern
            // Do the time consuming work now, while loading indicator is displayed
            var chamber = model.Chamber == "0" ? "Member" : model.Chamber;
            HttpContext.Session.SetString(SessionKeyFileName, $"{chamber} Constituent Fidelity ({model.From}-{model.To}).tsv");
            HttpContext.Session.SetString(SessionKeyContentType, "text/tab-separated-values");
            HttpContext.Session.SetString(SessionKeyContents, RecordDb.GetFidelityLeaderboard(model.Chamber, model.From, model.To));

            // Serve up the download page and deliver file
            return View("Download");
        }

        [HttpGet]
        public DataTable GetBallotMeasures(int member, short from, short to)
        {
            var dt = new DataTable
            {
                cols = new List<ColInfo> {
                    new ColInfo { label = "Year", type = "string" },
                    new ColInfo { label = "Measure", type = "string" },
                    new ColInfo { label = "Bill", type = "number" },
                    new ColInfo { label = "Description", type = "string" },
                    new ColInfo { label = "Member", type = "number" },
                    new ColInfo { label = "Voters", type = "number" },
                    new ColInfo { label = "Match", type = "string" }
                },
                rows = new List<DataPointSet>(),
                p = new Dictionary<string, string>()
            };

            var district = RecordDb.Members.First(i => i.Id == member).District;
            var records = (from b in RecordDb.Measures where b.Year >= @from && b.Year <= @to && b.BillNumber.HasValue
                           join d in RecordDb.DistrictResults on new { I = b.Id, D = district } equals new { I = d.MeasureId, D = d.District }
                           join v in RecordDb.VotingRecords on new { Y = b.Biennium, N = b.BillNumber.Value, M = member } equals new { Y = v.Biennium, N = v.BillNumber, M = v.Id }
                           select new { b.Year, b.Name, v.BillNumber, b.Description, MemberSupport = v.Support, VoterSupport = 100.0 * d.Support / (d.Support + d.Oppose) }).ToList();

            foreach (var record in records)
                dt.rows.Add(new DataPointSet
                {
                    c = new DataPoint[] {
                        new DataPoint { v = record.Year.ToString() },
                        new DataPoint { v = record.Name },
                        new DataPoint { v = record.BillNumber.ToString() },
                        new DataPoint { v = record.Description },
                        new DataPoint { v = record.MemberSupport > 0 ? "Y" : "N" },
                        new DataPoint { v = record.VoterSupport.ToString("N1") + "%" },
                        new DataPoint { v = record.MemberSupport > 0 ^ record.VoterSupport > 50.0 ? "N" : "Y" } }
                });

            return dt;
        }

        [HttpGet]
        public DataTable GetFidelityDataTable(string chamber, short from, short to)
        {
            var points = new List<Point>();
            var members = RecordDb.Members.Where(i => chamber == "0" || i.Agency == chamber).OrderBy(i => i.Party).ToList();
            var records = (from b in RecordDb.Measures where b.Year >= @from && b.Year <= @to && b.BillNumber.HasValue
                           join d in RecordDb.DistrictResults on b.Id equals d.MeasureId
                           join m in RecordDb.Members on d.District equals m.District
                           join v in RecordDb.VotingRecords on new { Y = b.Biennium, N = b.BillNumber.Value, M = m.Id } equals new { Y = v.Biennium, N = v.BillNumber, M = v.Id }
                           select new { v.Id, MemberSupport = v.Support > 0, VoterSupport = d.Support > d.Oppose }).ToList();

            foreach (var member in members)
            {
                var tally = records.Where(i => i.Id == member.Id);
                double n = tally.Count();
                if (n > 0)
                {
                    var score = tally.Count(i => !(i.MemberSupport ^ i.VoterSupport)) / n * 100;
                    points.Add(new Point { x = score, y = Stack(score, points), Id = member.Id, Series = member.Party, Label = GetTooltip(member, "Fidelity", score) });
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