using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using WhipStat.Models.LegTech;
using WhipStat.Models.PDC;
using System.Text.RegularExpressions;

namespace WhipStat.Data
{
    public partial class RecordDbContext : DbContext
    {
        public DbSet<NewVote> NewVotes { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<RollCall> RollCalls { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Committee> Committees { get; set; }
        public DbSet<PolicyArea> PolicyAreas { get; set; }
        public DbSet<Score> Scores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=192.168.2.2;Database=LegTech;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vote>().HasKey(t => new { t.RollCall_Id, t.Member_Id });
            modelBuilder.Entity<Bill>().HasKey(t => new { t.BillNumber, t.Year });
            modelBuilder.Entity<Score>().HasKey(t => new { t.Member_Id, t.Year, t.PolicyArea });
        }

        public void ProcessNewVotes()
        {
            // Uncomment the following lines only if rebuilding from scratch
            //Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.Votes");
            //Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.RollCalls");

            Console.WriteLine("Retrieving member list...");
            var members = Members.ToList();
            var sponsors = GetSponsors("2017-18");
            Console.WriteLine("Retrieving bills...");
            var bills = Bills.ToList();
            var areas = PolicyAreas.ToList();
            Console.WriteLine("Clearing old scores...");
            Console.WriteLine("Retrieving roll call votes...");
            var rolls = RollCalls.ToList();

            Console.WriteLine("Processing new votes...");
            foreach (var nv in NewVotes.OrderBy(i => i.RollCall_VoteId).ToList())
            {
                var year = Convert.ToInt16(nv.Biennium.Substring(0, 4));
                var roll = new RollCall()
                {
                    RollCall_Id = nv.RollCall_VoteId,
                    BillNumber = nv.LegNum,
                    Year = year,
                    Yea_total = nv.Yea_total,
                    Nay_total = nv.Nay_total,
                    Excused_total = nv.Excused_total,
                    Absent_total = nv.Absent_total,
                    Agency = nv.Agency,
                    Motion_description = nv.Motion_description,
                    Amendment_description = nv.Amendment_description
                };
                if (!rolls.Contains(roll))
                {
                    rolls.Add(roll);
                    RollCalls.Add(roll);

                    Console.WriteLine($"Processing roll call #{nv.RollCall_VoteId}...");
                    SaveChanges();
                }
                var bill = new Bill()
                {
                    BillNumber = nv.LegNum,
                    Year = year,
                    ShortBillId = nv.ShortBillId,
                    AbbrTitle = nv.AbbrTitle
                };
                if (!bills.Contains(bill))
                {
                    bill.Sponsors = GetSponsors(bill);
                    bill.Committees = GetCommittees(bill);
                    bill.PolicyArea = GetBestPolicyArea(bill, areas)?.Id;
                    bills.Add(bill);
                    Bills.Add(bill);
                }
                var member = members.FirstOrDefault(i => String.Equals(i.LastName, nv.LastName) && String.Equals(i.FirstName, nv.FirstName));
                if (member == null)
                {
                    member = sponsors.FirstOrDefault(i => String.Equals(i.LastName, nv.LastName) && String.Equals(i.FirstName, nv.FirstName));
                    if (member != null)
                    {
                        members.Add(member);
                        Members.Add(member);
                    }
                    else
                        Console.WriteLine($"  Couldn't find new member: {nv.LastName}, {nv.FirstName}");
                }
                var vote = new Vote()
                {
                    RollCall_Id = nv.RollCall_VoteId,
                    Member_Id = member.Id,
                    Member_vote = nv.Member_vote.Substring(0, 1).ToUpper()
                };

                Votes.Add(vote);
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving final changes...");
            SaveChanges();
        }

        public void UpdateMembers()
        {
            Console.WriteLine("Retrieving member list...");
            var members = Members.ToList();
            var sponsors = GetSponsors();

            foreach (var member in members)
            {
                var sponsor = sponsors.FirstOrDefault(i => i.Equals(member));
                if (sponsor != null)
                {
                    member.Id = sponsor.Id;
                    member.Name = sponsor.Name;
                    member.LongName = sponsor.LongName;
                    member.Agency = sponsor.Agency;
                    member.Acronym = sponsor.Acronym;
                    member.District = sponsor.District;
                    member.Party = sponsor.Party;
                    member.Phone = sponsor.Phone;
                    member.Email = sponsor.Email;
                }

                Console.WriteLine($"Updating {member}...");
                Members.Update(member);
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void FixupMembers()
        {
            Console.WriteLine("Retrieving member list...");
            var members = Members.ToList();
            var sponsors = GetSponsors();

            foreach (var member in members)
            {
                // Important that this list is ordered with most recent first
                var match = sponsors.FirstOrDefault(i => i.Id == member.Id);
                if (match != null)
                {
                    member.LastName = match.LastName;
                    member.FirstName = match.FirstName;
                    member.Name = match.Name;
                    member.LongName = match.LongName;
                    member.Acronym = match.Acronym;
                    member.Agency = match.Agency;
                    member.Party = match.Party;
                    member.District = match.District;
                    member.Email = match.Email;
                    member.Phone = match.Phone;
                }

                if (member.LastName == "Owen")
                {
                    member.Id = 321;
                    member.Name = "Brad Owen";
                    member.LongName = "Lieutenant Governor";
                    member.Acronym = "LG";
                    member.Agency = "Senate";
                    member.Phone = "(360) 786-7700";
                    member.Email = "ltgov@ltgov.wa.gov";
                }

                Console.WriteLine($"Updating {member}...");
           }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void IdentifyPhotos()
        {
            Console.WriteLine("Retrieving member list...");
            var members = Members.OrderByDescending(i => i.Id).ToList();

            foreach (var member in members)
            {
                // Sort matches so that most current is at the top (path descending)
                var files = Directory.GetFiles(@"d:\Pictures\Legislature\Thumbnails\", $"*{member.LastName}*.jpg", SearchOption.AllDirectories).OrderByDescending(i => i).ToList();
                if (files.Count() == 0)
                {
                    // Need to do these later by hand
                    Console.WriteLine($"No match found for {member} ({member.Id})!");
                }
                else if (files.Count() == 1)
                {
                    // We only got one match, so it's our best (and only) guess
                    MoveMemberPhoto(member, files[0]);
                }
                else if (files.Count() > 1)
                {
                    // Sort matches so that most current is at the top (path descending)
                    var matches = files.Where(i => i.Contains(member.FirstName)).OrderByDescending(i => i).ToList();
                    if (matches.Count() == 0)
                    {
                        // If we have no exact match, we take the most current close match
                        Console.WriteLine($"Using best guess for {member} ({member.Id}).");
                        MoveMemberPhoto(member, files[0]);
                    }
                    else
                    {
                        // If we have multiple exact matches, we take the most current
                        MoveMemberPhoto(member, matches[0]);
                    }
                }
            }
        }

        private void MoveMemberPhoto(Member member, string path)
        {
            File.Copy(path, @"d:\Pictures\Legislature\Indexed\" + member.Id + ".jpg", true);
        }

        public void UpdateBillInfo()
        {
            var areas = PolicyAreas.ToList();

            for (short year = 2003; year <= 2017; year += 2)
            {
                Console.WriteLine($"Retrieving bills in {year}...");
                var bills = Bills.Where(i => i.Year == year).ToList();

                foreach (var bill in bills)
                {
                    if (bill.Sponsors == null)
                    {
                        bill.Sponsors = GetSponsors(bill);
                        Console.WriteLine($"{bill} sponsors: {bill.Sponsors}");
                        Bills.Update(bill);
                    }
                    if (bill.Committees == null)
                    {
                        bill.Committees = GetCommittees(bill);
                        Console.WriteLine($"{bill} committees: {bill.Sponsors}");
                    }
                    if (bill.PolicyArea == null)
                    {
                        bill.PolicyArea = GetBestPolicyArea(bill, areas)?.Id;
                        Console.WriteLine($"{bill} policy area: {bill.PolicyArea}");
                    }
                }

                // Attempt to save changes to the database
                Console.WriteLine("Saving changes...");
                SaveChanges();
            }
        }

        public void UpdateCommittees()
        {
            var committees = Committees.ToList();
            var newCommittees = GetCommittees().Where(i => !committees.Contains(i));

            Console.WriteLine("Adding to Committee table...");
            Committees.AddRange(newCommittees);

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void UpdatePolicyAreas()
        {
            var areas = PolicyAreas.ToList();
            foreach (var bill in Bills)
            {
                var area = GetBestPolicyArea(bill, areas);
                if (area != null)
                {
                    bill.PolicyArea = area.Id;
                    Console.WriteLine($"{bill.AbbrTitle} -> {area.Name}");
                    Bills.Update(bill);
                }
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }
        public PolicyArea GetBestPolicyArea(Bill bill, List<PolicyArea> areas)
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
                    if (bill.AbbrTitle.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        ++score;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPolicy = area;
                }
            }

            return bestPolicy;
        }

        public void ScoreBills()
        {
            Console.WriteLine("Preloading member and bill lists...");
            var bills = Bills.ToList();
            var members = Members.ToList();
            var rollCalls = RollCalls.ToList();

            foreach (var bill in bills)
            {
                var calls = rollCalls.Where(i => i.BillNumber == bill.BillNumber && i.Year == bill.Year);
                foreach (var call in calls)
                {
                    var votes = Votes.Where(i => i.RollCall_Id == call.RollCall_Id).ToList();
                    double RTotal = 0, DTotal = 0, RCount = 0, DCount = 0;
                    foreach (var vote in votes)
                    {
                        var member = members.Single(i => i.Id == vote.Member_Id);
                        var supported = vote.Member_vote == "Y";
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

                    // Roll call score is the percentage difference between Republican and Democrat support
                    call.Score = (RCount / RTotal - DCount / DTotal) * 100;
                    RollCalls.Update(call);
                }

                // Bill score is the average of its roll call scores (excluding amendments)
                bill.Score = calls.Where(i => String.IsNullOrWhiteSpace(i.Amendment_description)).Average(i => i.Score);

                Console.WriteLine($"{bill} - Score: {bill.Score:N1}");
                Bills.Update(bill);
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void ScoreMembers()
        {
            Console.WriteLine("Clearing old scores...");
            Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.Scores");
            Console.WriteLine("Preloading voting records...");
            var members = Members.OrderBy(i => i.LastName).ToList();
            var bills = Bills.ToList();

            foreach (var member in members)
            {
                Console.WriteLine($" Scoring {member}");
                var votes = Votes.Where(i => i.Member_Id == member.Id).ToList();

                for (short year = 2003; year <= 2017; year += 2)
                {
                    for (short area = 0; area <= PolicyAreas.Count(); ++area)
                        ScoreMember(year, area, votes, bills);
                }
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        private void ScoreMember(short year, short area, List<Vote> votes, List<Bill> bills)
        {
            var hashcodes = bills.Where(i => i.Year == year && (area == 0 || i.PolicyArea == area)).Select(i => i.GetHashCode()).ToList();
            var calls = RollCalls.Where(i => hashcodes.Contains((i.Year << 16) ^ i.BillNumber)).ToList();

            double total = 0;
            int count = 0;
            foreach (var vote in votes)
            {
                var call = calls.FirstOrDefault(i => i.RollCall_Id == vote.RollCall_Id);
                if (call != null)
                {
                    switch (vote.Member_vote)
                    {
                        case "Y":
                            total += call.Score.Value;
                            ++count;
                            break;
                        case "N":
                            total -= call.Score.Value;
                            ++count;
                            break;
                    }
                }
            }
            if (count > 0)
            {
                // Will need to change this for re-runs, but hopefully EF will implement AddOrUpdate by then
                Scores.Add(new Score { Member_Id = votes.First().Member_Id, Year = year, PolicyArea = area, Total = total, Count = count });
            }
        }

        private List<Member> GetSponsors()
        {
            var list = new List<Member>();
            string[] biennia = { "2017-18", "2015-16", "2013-14", "2011-12", "2009-10", "2007-08", "2005-06", "2003-04" };

            foreach (var biennium in biennia)
                list.AddRange(GetSponsors(biennium));

            return list;
        }

        private List<Member> GetSponsors(string biennium)
        {
            var xmlSer = new DataContractSerializer(typeof(List<Member>));
            var list = new List<Member>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                Console.WriteLine($"Retrieving members for {biennium} biennium...");
                var response = client.GetAsync($"/SponsorService.asmx/GetSponsors?biennium={biennium}").Result;
                var result = xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<Member>;
                list.AddRange(result.Where(i => !list.Contains(i)));
            }

            return list;
        }

        private string GetSponsors(Bill bill)
        {
            var biennium = $"{bill.Year}-{(bill.Year + 1) % 100:D2}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                var response = client.GetAsync($"/LegislationService.asmx/GetSponsors?biennium={biennium}&billId={bill.BillNumber}").Result;
                return GetAcronyms(response.Content.ReadAsStringAsync().Result);
            }
        }

        private List<Committee> GetCommittees()
        {
            var xmlSer = new DataContractSerializer(typeof(List<Committee>));
            var list = new List<Committee>();
            string[] biennia = { "2017-18", "2015-16", "2013-14", "2011-12", "2009-10", "2007-08", "2005-06", "2003-04" };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                foreach (var biennium in biennia)
                {
                    Console.WriteLine($"Retrieving committees for {biennium} biennium...");
                    var response = client.GetAsync($"/CommitteeService.asmx/GetCommittees?biennium={biennium}").Result;
                    var result = xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<Committee>;
                    list.AddRange(result.Where(i => !list.Contains(i)));
                }
            }

            return list;
        }

        private string GetCommittees(Bill bill)
        {
            var biennium = $"{bill.Year}-{(bill.Year + 1) % 100:D2}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                var response = client.GetAsync($"/LegislationService.asmx/GetHearings?biennium={biennium}&billNumber={bill.BillNumber}").Result;
                return GetAcronyms(response.Content.ReadAsStringAsync().Result);
            }
        }

        private string GetAcronyms(string input)
        {
            var regex = new Regex(@"<Acronym>(.+?)<");
            var matches = regex.Matches(input).Cast<Match>().Select(i => i.Groups[1].Value).Distinct();
            return String.Join(",", matches);
        }
    }
}
