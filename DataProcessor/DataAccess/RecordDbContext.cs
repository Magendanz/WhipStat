using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using WhipStat.Helpers;
using WhipStat.Models.LegTech;

using System.Text;
using System.Text.RegularExpressions;

namespace WhipStat.DataAccess
{
    public partial class RecordDbContext : DbContext
    {
        public DbSet<Models.LWS.Member> Members { get; set; }
        public DbSet<Models.LWS.Committee> Committees { get; set; }

        public DbSet<PolicyArea> PolicyAreas { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<RollCall> RollCalls { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Testimony> Testimonies { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AdvocacyRecord> AdvocacyRecords { get; set; }
        public DbSet<VotingRecord> VotingRecords { get; set; }
        public DbSet<Measure> Measures { get; set; }
        public DbSet<DistrictResult> DistrictResults { get; set; }


        public readonly string[] biennia = { "2021-22", "2019-20", "2017-18", "2015-16", "2013-14", "2011-12", "2009-10",
            "2007-08", "2005-06", "2003-04", "2001-02", "1999-00", "1997-98", "1995-96", "1993-94", "1991-92" };

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<RecordDbContext>();
            var configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration["RecordDb:SqlConnectionString"]);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>().HasKey(t => new { t.BillId, t.Biennium });
            modelBuilder.Entity<Vote>().HasKey(t => new { t.RollCallId, t.MemberId });
            modelBuilder.Entity<Score>().HasKey(t => new { t.MemberId, t.Year, t.PolicyArea });
            modelBuilder.Entity<AdvocacyRecord>().HasKey(t => new { t.Id, t.BillNumber, t.Biennium });
            modelBuilder.Entity<VotingRecord>().HasKey(t => new { t.Id, t.BillNumber, t.Biennium });
            modelBuilder.Entity<DistrictResult>().HasKey(t => new { t.MeasureId, t.District });
        }

        public void GetMembers()
        {
            Console.WriteLine($"Syncing member table from LWS...");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Members");
            var members = new HashSet<Models.LWS.Member>();

            foreach (var biennium in biennia)
            {
                Console.WriteLine($"Retrieving members for {biennium} biennium...");
                foreach (var member in LwsAccess.GetMembers(biennium))
                    if (!string.IsNullOrWhiteSpace(member.Party))
                        members.Add(member);
            }

            Console.WriteLine("Saving changes...\n");
            Members.AddRange(members);
            SaveChanges();
        }

        public void GetCommittees()
        {
            Console.WriteLine($"Syncing committee table from LWS...");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Committees");
            var committees = new HashSet<Models.LWS.Committee>();

            foreach (var biennium in biennia)
            {
                Console.WriteLine($"Retrieving committees for {biennium} biennium...");
                foreach (var committee in LwsAccess.GetCommittees(biennium))
                    if (!committees.Contains(committee))
                        committees.Add(committee);
            }

            Console.WriteLine("Saving changes...\n");
            Committees.AddRange(committees);
            SaveChanges();
        }

        public void GetBills()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Bills");
            var bills = new HashSet<Bill>();

            Console.WriteLine($"Syncing legislation from LWS...");
            foreach (var biennium in biennia)
            {
                Console.WriteLine($"Retrieving bills for {biennium} biennium...");
                var year = Convert.ToInt32(biennium[..4]);
                var newBills = LwsAccess.GetLegislationByYear(year + 1);
                newBills.AddRange(LwsAccess.GetLegislationByYear(year));
                foreach (var bill in newBills)
                    bills.Add(new Bill()
                    {
                        BillNumber = bill.BillNumber,
                        Biennium = bill.Biennium,
                        BillId = bill.BillId,
                    });
            }

            Console.WriteLine("Saving changes...\n");
            Bills.AddRange(bills);
            SaveChanges();
        }

        public void UpdateBillInfo()
        {
            Console.Write($"Updating bill info...");
            var areas = PolicyAreas.ToList();
            var bills = Bills.ToList();
            int count = bills.Count, i = 0;

            foreach (var bill in bills)
            {
                try
                {
                    bill.AbbrTitle = GetTitle(bill);
                    bill.Sponsors = GetSponsors(bill);
                    bill.Committees = GetCommittees(bill);
                    bill.PolicyArea = GetBestPolicyArea(bill, areas)?.Id;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Update failed for bill {bill}!  Exception: {ex.Message}");
                }

                Bills.Update(bill);
                ReportProgress((double)++i / count);
            }

            Console.WriteLine("\nSaving changes...\n");
            SaveChanges();
        }

        private static string GetTitle(Bill bill)
            => LwsAccess.GetLegislation(bill.Biennium, bill.BillNumber).First(i => i.BillId == bill.BillId).ShortDescription;

        private static string GetSponsors(Bill bill)
            => string.Join(",", LwsAccess.GetSponsors(bill.Biennium, bill.BillId).Select(i => i.Acronym));

        private static string GetCommittees(Bill bill)
        {
            var list = new List<string>();
            foreach (var hearing in LwsAccess.GetHearings(bill.Biennium, bill.BillNumber))
                list.AddRange(hearing.CommitteeMeeting.Committees.Select(i => i.Acronym));

            return string.Join(",", list.Distinct());
        }

        private static PolicyArea GetBestPolicyArea(Bill bill, List<PolicyArea> areas)
        {
            int bestScore = 0;
            PolicyArea bestPolicy = null;

            foreach (var area in areas)
            {
                var score = 0;
                var committees = bill.Committees.Split(',');
                foreach (var committee in area.Committees.Split(','))
                    if (committees.Contains(committee))
                        ++score;
                foreach (var keyword in area.Keywords.Split(','))
                    if (bill.AbbrTitle?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        ++score;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPolicy = area;
                }
            }

            return bestPolicy;
        }

        public void GetRollCalls()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.RollCalls");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Votes");

            var calls = new HashSet<RollCall>();
            var votes = Votes.ToHashSet();
            var members = Members.ToList();
            var bills = Bills.ToList();
            int count = bills.Count, i = 0;

            Console.Write($"Syncing roll calls from LWS...");
            foreach (var bill in bills)
            {
                foreach (var roll in LwsAccess.GetRollCalls(bill.Biennium, bill.BillNumber))
                {
                    // Roll call score is the percentage difference between Republican and Democrat support
                    double RTotal = 0, DTotal = 0, RCount = 0, DCount = 0;
                    foreach (var vote in roll.Votes)
                    {
                        var member = members.SingleOrDefault(i => i.Id == vote.MemberId);
                        if (member != null)
                        {
                            var supported = vote.VOte.StartsWith("Y");
                            switch (member.Party)
                            {
                                case "R":
                                    ++RTotal;
                                    if (supported)
                                        ++RCount;
                                    break;
                                case "D":
                                    ++DTotal;
                                    if (supported)
                                        ++DCount;
                                    break;
                            }
                        }
                    }

                    var id = roll.GetHashCode();
                    double score = ((RTotal > 0 ? RCount / RTotal : 0) - (DTotal > 0 ? DCount / DTotal : 0)) * 100;
                    calls.Add(new RollCall()
                    {
                        Id = id,
                        BillNumber = short.Parse(roll.BillId[^4..]),      // Last four chars should be number
                        BillId = roll.BillId,
                        Biennium = roll.Biennium,
                        Agency = roll.Agency,
                        Motion = roll.Motion,
                        SequenceNumber = roll.SequenceNumber,
                        VoteDate = roll.VoteDate,
                        YeaVotes = roll.YeaVotes.Count,
                        NayVotes = roll.NayVotes.Count,
                        AbsentVotes = roll.AbsentVotes.Count,
                        ExcusedVotes = roll.ExcusedVotes.Count,
                        Score = score
                    });

                    foreach (var vote in roll.Votes)
                    {
                        votes.Add(new Vote()
                        {
                            RollCallId = id,
                            MemberId = vote.MemberId,
                            MemberVote = vote.VOte[0..1].ToUpperInvariant()
                        });
                    }
                }
                ReportProgress((double)++i / count);
            }

            Console.WriteLine("\nSaving changes...\n");
            RollCalls.AddRange(calls);
            Votes.AddRange(votes);
            SaveChanges();
        }

        public void ScoreBills()
        {
            Console.WriteLine("Preloading floor records...");

            var bills = Bills.ToList();
            var calls = RollCalls.ToList();
            int count = bills.Count, i = 0;

            Console.Write($"Calculating bill scores...");
            foreach (var bill in bills)
            {
                var votes = calls.Where(i => i.BillId == bill.BillId && i.Biennium == bill.Biennium).ToList();
                if (votes.Count > 0)
                    bill.Score = votes.Average(i => i.Score);

                ReportProgress((double)++i / count);
            }

            Console.WriteLine("\nSaving changes...\n");
            SaveChanges();
        }

        public void ScoreMembers()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Scores");
            var scores = new HashSet<Score>();
            var members = Members.ToList();
            int count = members.Count * biennia.Length, i = 0;

            Console.Write($"Calculating member scores...");
            foreach (var biennium in biennia)
            {
                var year = Convert.ToInt16(biennium[0..4]);

                foreach (var member in members)
                {
                    var votes = from v in Votes
                                join r in RollCalls on v.RollCallId equals r.Id
                                join b in Bills on r.BillNumber equals b.BillNumber
                                where v.MemberId == member.Id && r.Biennium == biennium
                                select new { v.MemberVote, r.Score.Value, b.PolicyArea };
                    
                    for (short area = 0; area <= PolicyAreas.Count(); ++area)
                    {
                        double t = 0;
                        int c = 0;
                        foreach (var vote in votes.ToList())
                        {
                            if (area == 0 || vote.PolicyArea == area)
                            {
                                switch (vote.MemberVote)
                                {
                                    case "Y":
                                        t += vote.Value;
                                        ++c;
                                        break;
                                    case "N":
                                        t -= vote.Value;
                                        ++c;
                                        break;
                                }
                            }
                        }
                        if (c > 0)
                            scores.Add(new Score { MemberId = member.Id, Year = year, PolicyArea = area, Total = t, Count = c });
                    }
                    ReportProgress((double)++i / count);
                }
            }

            Console.WriteLine("\nSaving changes...\n");
            Scores.AddRange(scores);
            SaveChanges();
        }

        public void GetTestimony(int start, int end)
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Testimonies");

            for (int year = start; year <= end; year++)
            {
                var test = new HashSet<Testimony>();
                Console.Write($"Year: {year}");
                var meetings = LwsAccess.GetCommitteeMeetings(new DateTime(year, 1, 1), new DateTime(year, 12, 31));
                Console.Write($" ({meetings.Count}) ");
                foreach (var meeting in meetings)
                {
                    try
                    {
                        var items = LwsAccess.GetAgendaItems(meeting.Agency, meeting.Committees.First().Name, meeting.Date);
                        foreach (var i in items)
                            foreach (var t in i.Testifiers)
                                test.Add(new Testimony
                                {
                                    LastName = t.LastName.Trim(64),
                                    FirstName = t.FirstName.Trim(64),
                                    Organization = t.Organization.Trim(256),
                                    Street = t.Street.Trim(64),
                                    City = t.City.Trim(32),
                                    State = t.State.Trim(2),
                                    Zip = t.Zip.Trim(10),
                                    Email = t.EmailAddress.Trim(64),
                                    Phone = t.PhoneNumber.Trim(16),
                                    BillId = i.BillId.Trim(16),
                                    Position = t.Position.Trim(16),
                                    Testify = t.IsSpeaking,
                                    OutOfTown = t.OutOfTown,
                                    CalledUp = t.CalledUp,
                                    NoShow = t.NoShow,
                                    MeetingDate = t.TimeOfSignIn
                                });
                    }
                    catch { }
                    Console.Write(".");
                }
                Console.WriteLine("\nSaving changes...\n");
                Testimonies.AddRange(test);
                SaveChanges();
            }
        }

        public void ImportTestimonyDir(string path)
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Testimonies");

            var files = Directory.EnumerateFiles(path, "*.csv", SearchOption.AllDirectories);
            foreach (var file in files)
                ImportTestimony(file);
        }

        public void ImportTestimony(string path)
        {
            var test = new List<Testimony>();
            var table = DataTable.New.ReadCsv(path);
            var agency = path.Contains("House", StringComparison.InvariantCultureIgnoreCase) ? "House"
                : path.Contains("Senate", StringComparison.InvariantCultureIgnoreCase) ? "Senate" : null;
            int count = table.NumRows, i = 0;

            Console.Write($"Importing {Path.GetFileName(path)}...");
            foreach (var row in table.Rows)
            {
                for (int j = 0; j < row.Values.Count; j ++)
                {
                    if (row.Values[j] == "NULL")
                        row.Values[j] = null;
                }
                var record = new Testimony
                {
                    LastName = GetByNames(row, "LastName", "Last Name")?.ToTitleCase().Trim(64),
                    FirstName = GetByNames(row, "FirstName", "First Name")?.ToTitleCase().Trim(64),
                    Organization = row["Organization"]?.ToTitleCase().Trim(256),
                    Street = row["Street"]?.ToTitleCase().Trim(64),
                    City = row["City"]?.ToTitleCase().Trim(32),
                    State = row["State"]?.ToUpperInvariant().Trim(2),
                    Zip = row["Zip"]?.Trim(10),
                    Phone = GetByNames(row, "Phone", "PhoneNumber")?.ToPhoneNumber().Trim(32),
                    Email = row["Email"]?.ToLowerInvariant().Trim(64),
                    Agency = agency,
                    CommitteeName = row["CommitteeName"]?.Trim(32),
                    AbbrTitle = GetByNames(row, "AgendaItem", "AbbrTitle")?.ToTitleCase().Trim(64),
                    BillId = GetByNames(row, "BillId", "BillNumber")?.ToUpperInvariant().Trim(16),
                    Position = row["Position"]?.ToTitleCase().Trim(16),
                    Testify = GetBoolByNames(row, "Testify"),
                    OutOfTown = GetBoolByNames(row, "OutOfTown", "IsOutOfTown"),
                    CalledUp = GetBoolByNames(row, "CalledUp"),
                    NoShow = GetBoolByNames(row, "NoShow", "NotPresent")
                };

                if (string.IsNullOrWhiteSpace(record.LastName) || string.IsNullOrWhiteSpace(record.FirstName))
                    continue;

                // Now, fill in some of our missing properties
                if (!string.IsNullOrWhiteSpace(record.BillId))
                {
                    record.BillId = KeywordParser.Filter(record.BillId);
                    if (short.TryParse(record.BillId[^4..], out var num))
                        record.BillNumber = num;
                }
                else if (!string.IsNullOrWhiteSpace(record.AbbrTitle))
                {
                    var matches = Regex.Match(record.AbbrTitle, @"^([HSBE]{1,4}B\s\d{4})\s(.+)$");
                    if (matches.Success)
                    {
                        record.BillId = matches.Groups[1].Value;
                        if (short.TryParse(record.BillId[^4..], out var num))
                            record.BillNumber = num;
                        record.AbbrTitle = matches.Groups[2].Value;
                    }
                }

                var date = GetByNames(row, "MeetingDate", "StartTime", "TimeSignedIn");
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var result))
                    record.MeetingDate = result;
                record.Biennium = ToBiennium(record.MeetingDate.Year);

                test.Add(record);
                ReportProgress((double)++i / count);
            }

            Console.WriteLine("\nSaving changes...\n");
            Testimonies.AddRange(test);
            SaveChanges();
        }

        private static string ToBiennium(int year)
        {
            var end = ++year & -2;
            return $"{end - 1:D4}-{end % 100:D2}";
        }

        public void FixupTestimony(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.Latin1);
            var cols = lines[0].Split(',');

            int iLastName = Array.IndexOf(cols, "LastName");
            int iTimeSignedIn = Array.IndexOf(cols, "TimeSignedIn");
            int iStreet = Array.IndexOf(cols, "Street");
            int iCity= Array.IndexOf(cols, "City");
            int iZip = Array.IndexOf(cols, "Zip");
            int iExtra = Array.FindIndex(cols, val => string.IsNullOrEmpty(val));
            var pattern = "\"([^\"]*)\"";

            Console.WriteLine("Reading addresses...\n");
            var addresses = DataTable.New.ReadCsv(@"C:\Users\Chad\Projects\WhipStat\Datasets\LegTech\297-042021_Final_Production\Combined\2021_H&S_exportTestifiers.csv");

            // Targeted search and replace
            Console.WriteLine("Fixing up records...\n");
            for (int i = 0; i < lines.Length; i++)
            {
                var values = Regex.Replace(lines[i], pattern, string.Empty).Split(',');
                if (values[iLastName].Trim() == string.Empty && !string.IsNullOrWhiteSpace(values[iExtra]))
                {
                    // LastName field is empty
                    for (int j = iLastName; j < values.Length - 1; j++)
                        values[j] = values[j + 1];
                    values[values.Length - 1] = string.Empty;

                    var previous = Regex.Replace(lines[i - 1], pattern, string.Empty).Split(',');
                    values[iTimeSignedIn] = previous[iTimeSignedIn];
                    lines[i] = string.Join(',', values);
                }
                if (values[iZip].Contains('/'))
                {
                    // Bad zip code
                    values[iZip] = string.Empty;
                    foreach (var row in addresses.Rows)
                    {
                        if (row["LastName"].Equals(values[iLastName], StringComparison.OrdinalIgnoreCase) 
                            && row["Street"].Equals(values[iStreet], StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(row["Zip"]) && row["Zip"].Length == 5)
                        {
                            values[iZip] = row["Zip"];
                            break;
                        }
                    }
                    lines[i] = string.Join(',', values);
                }
            }
            File.WriteAllLines(path, lines, Encoding.Latin1);
        }

        private string GetByNames(Row row, params string[] names)
        {
            foreach (var name in names)
            {
                if (row.ColumnNames.Contains(name) && !string.IsNullOrWhiteSpace(row[name]))
                    return row[name];
            }
            return null;
        }

        private bool GetBoolByNames(Row row, params string[] names)
        {
            var value = GetByNames(row, names)?.ToLower();
            return value == "yes" || value == "true" || value == "1";
        }

        public void GetOrganizations()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Organizations");

            Console.WriteLine("Loading organizaitons from PDC...\n");
            var orgs = new HashSet<Organization>();
            var employers = PdcAccess.GetEmployers().OrderByDescending(i => i.employment_year).ThenBy(i => i.employer_name);
            var groups = employers.GroupBy(i => i.employer_id);
            foreach (var group in groups)
            {
                var employer = group.First();
                orgs.Add(new Organization
                {
                    Id = Convert.ToInt32(employer.employer_id),
                    Name = employer.employer_name.IsUpper() ? employer.employer_name.ToTitleCase() : employer.employer_name
                });
            }

            Console.WriteLine("\nSaving changes...\n");
            Organizations.AddRange(orgs);
            SaveChanges();
        }

        public void MatchOrganizations()
        {
            // Load up our dictionary of canonized names
            Console.WriteLine("Loading dictionary of canonized nicknames and abbreviations...");
            var wordParser = new KeywordParser("Data/Aliases.csv");

            Console.WriteLine("Loading testimony...");
            var orgs = Organizations.ToList();
            var testimonies = Testimonies.ToList();
            int count = testimonies.Count, i = 0;

            // We don't preserve the keywords, so we need to rebuild them here
            foreach (var org in orgs)
                org.Keywords = wordParser.Parse(org.Name);

            Console.Write("Matching organizations...");
            foreach (var testimony in testimonies)
            {
                testimony.OrgId = null;
                // Look for a matching organization
                if (!string.IsNullOrWhiteSpace(testimony.Organization))
                {
                    // Look for common keywords in organization name and pick best match
                    var words = wordParser.Parse(testimony.Organization);
                    if (words.Length > 0)
                    {
                        var org = BestKeywordMatch(orgs, words, 0.8);
                        if (org != null)
                            testimony.OrgId = org.Id;
                    }
                }
                Testimonies.Update(testimony);
                ReportProgress((double)++i / count);
            }
            Console.WriteLine("\nSaving changes...\n");
            SaveChanges();
        }

        public void ScoreVotingRecords()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.VotingRecords");
            var members = Members.ToList();
            string[] biennia = { "2021-22", "2019-20", "2017-18", "2015-16", "2013-14", "2011-12" };

            Console.WriteLine($"Calculating voting records...");
            foreach (var biennium in biennia)
            {
                Console.Write($"Biennium: {biennium}");
                var records = new HashSet<VotingRecord>();
                var bills = Bills.Where(i => i.Biennium == biennium).ToList();
                int count = bills.Count, i = 0;

                var votes = (from v in Votes join r in RollCalls on v.RollCallId equals r.Id
                            where r.Biennium == biennium && r.Motion.Contains("Final Passage")
                            select new { r.Id, r.BillNumber, v.MemberId, v.MemberVote }).OrderBy(i => i.Id).ToList();

                foreach (var bill in bills)
                {
                    var sponsors = bill.Sponsors.Split(",");
                    foreach (var member in members)
                    {
                        var tally = votes.Where(i => i.BillNumber == bill.BillNumber && i.MemberId == member.Id 
                                                && (i.MemberVote == "Y" || i.MemberVote == "N")).ToList();
                        if (tally.Count == 0)
                            continue;
                        var sponsor = Array.IndexOf(sponsors, member.Acronym) + 1;
                        var last = tally.Last();
                        records.Add(new VotingRecord
                        {
                            Id = member.Id,
                            BillNumber = bill.BillNumber,
                            Biennium = biennium,
                            Sponsor = (short)sponsor,
                            Votes = (short)tally.Count,
                            Support = (short)(last.MemberVote == "Y" ? 1 : -1)
                        });
                    }
                    ReportProgress((double)++i / count);
                }
                Console.WriteLine("\nSaving changes...\n");
                VotingRecords.AddRange(records);
                SaveChanges();
            }
        }

        public void ScoreAdvocacyRecords()
        {
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.AdvocacyRecords");
            var testimonies = Testimonies.ToList();
            string[] biennia = { "2021-22", "2019-20", "2017-18", "2015-16", "2013-14" };

            Console.WriteLine($"Calculating advocacy records...");
            foreach (var biennium in biennia)
            {
                Console.Write($"Biennium: {biennium}");
                var records = new HashSet<AdvocacyRecord>();
                var orgs = testimonies.Where(i => i.Biennium == biennium && i.OrgId.HasValue && i.BillNumber.HasValue)
                    .GroupBy(i => i.OrgId);
                int count = orgs.Count(), i = 0;

                foreach (var org in orgs)
                {
                    var bills = org.GroupBy(i => i.BillNumber);
                    foreach (var bill in bills)
                    {
                        var support = bill.Sum(i => i.Position == "Pro" ? 1 : i.Position == "Con" ? -1 : 0);
                        var sponsor = bill.Sum(i => i.Testify ? 1 : 0);
                        records.Add(new AdvocacyRecord
                        {
                            Id = org.Key.Value,
                            BillNumber = bill.Key.Value,
                            Biennium = biennium,
                            Sponsor = (short)sponsor,
                            Votes = (short)bill.Count(),
                            Support = (short)support
                        });
                    }
                    ReportProgress((double)++i / count);
                }
                Console.WriteLine("\nSaving changes...\n");
                AdvocacyRecords.AddRange(records);
                SaveChanges();
            }
        }

        private static Organization BestKeywordMatch(IEnumerable<Organization> items, string[] keywords, double threshold)
        {
            Organization result = null;
            double max = 0;
            foreach (var item in items)
            {
                var match = item.Keywords.KeywordMatch(keywords);
                if (match >= 1.0)
                    return item;
                else if (match > max)
                {
                    // If we don't find an exact match, keep track of the best match
                    max = match;
                    result = item;
                }
            }

            return max >= threshold ? result : null;
        }

        public void GenerateMemberReports(string biennium, string chamber, string party, short start)
        {
            Console.WriteLine($"Retrieving lobbyist employer summaries...");
            var summaries = PdcAccess.GetEmployerSummaries();

            Console.WriteLine($"Retrieving {chamber} members for {biennium} biennium...");
            var members = LwsAccess.GetMembers(biennium).Where(i => i.Party == party && i.Agency == chamber).ToList();
            foreach (var member in members)
            {
                Console.WriteLine($"Generating report for {member.LongName}...");
                var sb = new StringBuilder();
                var records = VotingRecords.Where(i => i.Id == member.Id)
                    .Join(AdvocacyRecords, i => new { Y = i.Biennium, N = i.BillNumber }, j => new { Y = j.Biennium, N = j.BillNumber },
                    (i, j) => new { j.Id, Count = i.Votes, Favor = i.Support, j.Votes, j.Support}).ToList();

                sb.AppendLine("Organization\tCount\tCorrelation\tContributions");
                var orgs = records.GroupBy(i => i.Id).Join(Organizations, i => i.Key, j => j.Id,
                    (i, j) => new { j.Id, j.Name, Count = i.Count(), Score = i.Average(i => 100.0 * i.Support / i.Votes * i.Favor / i.Count) });

                foreach (var org in orgs.OrderByDescending(i => i.Score).ThenBy(j => j.Name))
                {
                    var total = summaries.Where(i => i.employer_nid == org.Id.ToString() && i.year > start).Sum(j => j.political);
                    sb.AppendLine($"{org.Name}\t{org.Count}\t{org.Score:N1}\t{total:C}");
                }

                File.WriteAllText($"{member.Name}.tsv", sb.ToString());
            }
        }

        public void GetAVStats(string biennium, params int[] bills)
        {
            var members = LwsAccess.GetMembers(biennium);

            foreach (var billNumber in bills)
            {
                var rolls = LwsAccess.GetRollCalls(biennium, billNumber);
                Console.WriteLine($"Analyzing Bill: {billNumber}");
                var finalHouse = rolls.Where(i => i.Agency == "House").OrderByDescending(i => i.VoteDate).First();
                Console.WriteLine($"House Final: {finalHouse.YeaVotes.Count} - {finalHouse.NayVotes.Count}");

                var finalSenate = rolls.Where(i => i.Agency == "Senate").OrderByDescending(i => i.VoteDate).First();
                Console.WriteLine($"Senate Final: {finalSenate.YeaVotes.Count} - {finalSenate.NayVotes.Count}");
                Console.WriteLine();

                var hdcYea = finalHouse.Votes.Count(i => i.VOte.StartsWith("Y") && members.First(j => j.Id == i.MemberId).Party == "D");
                var hdcNay = finalHouse.Votes.Count(i => i.VOte.StartsWith("N") && members.First(j => j.Id == i.MemberId).Party == "D");
                Console.WriteLine($"House Democrats: {hdcYea} - {hdcNay}");
                var sdcYea = finalSenate.Votes.Count(i => i.VOte.StartsWith("Y") && members.First(j => j.Id == i.MemberId).Party == "D");
                var sdcNay = finalSenate.Votes.Count(i => i.VOte.StartsWith("N") && members.First(j => j.Id == i.MemberId).Party == "D");
                Console.WriteLine($"Senate Democrats: {sdcYea} - {sdcNay}");
                Console.WriteLine();

                var hrcYea = finalHouse.Votes.Count(i => i.VOte.StartsWith("Y") && members.First(j => j.Id == i.MemberId).Party == "R");
                var hrcNay = finalHouse.Votes.Count(i => i.VOte.StartsWith("N") && members.First(j => j.Id == i.MemberId).Party == "R");
                Console.WriteLine($"House Republicans: {hrcYea} - {hrcNay}");
                var srcYea = finalSenate.Votes.Count(i => i.VOte.StartsWith("Y") && members.First(j => j.Id == i.MemberId).Party == "R");
                var srcNay = finalSenate.Votes.Count(i => i.VOte.StartsWith("N") && members.First(j => j.Id == i.MemberId).Party == "R");
                Console.WriteLine($"Senate Republicans: {srcYea} - {srcNay}");
                Console.WriteLine();

                Console.WriteLine($"Sen. Mullet: {finalSenate.Votes.First(i => i.Name == "Mullet").VOte}");
                Console.WriteLine($"Rep. Ramos: {finalHouse.Votes.First(i => i.Name == "Ramos").VOte}");
                Console.WriteLine($"Rep. Callan: {finalHouse.Votes.First(i => i.Name == "Callan").VOte}");
                Console.WriteLine();
            }
        }

        public void BiparisanBillSponsorship()
        {
            string[] biennia = { "2021-22", "2019-20", "2017-18", "2015-16", "2013-14" };

            foreach (var biennium in biennia)
            {
                Console.WriteLine($"Analyzing biennium {biennium}...");
                var members = LwsAccess.GetMembers(biennium);
                var tally = new Dictionary<string, short>();

                foreach (var bill in Bills.Where(i => i.Biennium == biennium).ToArray())
                {
                    var sponsors = bill.Sponsors.Split(',');
                    var party = members.FirstOrDefault(i => i.Acronym == sponsors[0])?.Party;
                    if (party != null)
                    {
                        foreach (var member in members)
                        {
                            if (party != member.Party && sponsors.Contains(member.Acronym))
                            {
                                if (!tally.ContainsKey(member.Name))
                                    tally.Add(member.Name, 0);
                                tally[member.Name]++;
                            }
                        }
                    }
                }

                var ranking = tally.OrderByDescending(i => i.Value).ToArray();
                foreach (var item in ranking)
                    Console.WriteLine($"  {item.Key} - {item.Value}");
            }
        }

        public void GetMeasures(string supportPath, string opposePath)
        {
            // Load the CSV file
            Console.WriteLine("Loading ballot measure results by LD...");
            var support = DataTable.New.ReadCsv(supportPath);
            var oppose = DataTable.New.ReadCsv(opposePath);

            Debug.Assert(support.NumRows == oppose.NumRows);
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Measures");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.DistrictResults");
            var mesaures = new HashSet<Measure>();
            var results = new HashSet<DistrictResult>();

            var supRows = support.Rows.ToList();
            var oppRows = oppose.Rows.ToList();
            for (int i = 0; i < supRows.Count; i++)
            {
                var billNumber = oppRows[i]["BillNumber"];
                var year = short.Parse(oppRows[i]["Year"]);
                var measure = new Measure()
                {
                    Id = i + 1,
                    Year = year,
                    Name = oppRows[i]["Name"],
                    Description = oppRows[i]["Description"],
                    BillNumber = string.IsNullOrWhiteSpace(billNumber) ? null : short.Parse(billNumber),
                    Biennium = ToBiennium(year)
                };
                mesaures.Add(measure);

                for (short d = 1; d < 50; d++)
                {
                    var result = new DistrictResult()
                    {
                        MeasureId = measure.Id,
                        District = d,
                        Support = int.Parse(supRows[i]["LD " + d]),
                        Oppose = int.Parse(oppRows[i]["LD " + d])
                    };
                    results.Add(result);
                }
            }

            Console.WriteLine("Saving changes...\n");
            Measures.AddRange(mesaures);
            DistrictResults.AddRange(results);
            SaveChanges();
        }

        public void RenamePhotos(string path)
        {
            Console.WriteLine($"Renaming member photos...");
            var chamber = Path.GetFileNameWithoutExtension(path);

            foreach (var file in Directory.GetFiles(path))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var member = Members.FirstOrDefault(i => String.Equals(i.Agency, chamber, StringComparison.OrdinalIgnoreCase) 
                                                      && String.Equals(i.LastName, name, StringComparison.OrdinalIgnoreCase));
                if (member != null)
                    File.Move(file, Path.Combine(Path.GetDirectoryName(file), member.Id + ".jpg"));
            }
        }
        private static void ReportProgress(double complete)
        {
            const int padding = 8;

            Console.CursorVisible = false;
            var str = $"{complete:P1}".PadLeft(padding);
            Console.Write(str + new string('\b', padding));
        }
    }
}
