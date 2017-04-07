using System;
using System.Collections.Generic;
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

        public void PopulateBills()
        {
            for (short year = 2003; year < 2017; year += 2)
            {
                Console.WriteLine($"Retrieving bills in {year}...");
                var bills = Bills.Where(i => i.Year == year).ToList();

                foreach (var bill in bills)
                {
                    bill.Sponsors = GetSponsors(bill.BillNumber, year);
                    Console.WriteLine($"{bill}: {bill.Sponsors}");
                    Bills.Update(bill);
                }

                // Attempt to save changes to the database
                Console.WriteLine("Saving changes...");
                SaveChanges();
            }
        }

        public void PopulateCommittees()
        {
            var committees = Committees.ToList();
            var newCommittees = GetCommittees().Where(i => !committees.Contains(i));

            Console.WriteLine("Adding to Committee table...");
            Committees.AddRange(newCommittees);

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void IdentifyMembers()
        {
            Console.WriteLine("Retrieving member list...");
            var members = Members.ToList();
            var sponsors = GetSponsors();

            using (var db = new DonorDbContext())
            {
                foreach (var member in members)
                {
                    var match = sponsors.FirstOrDefault(i => i.Equals(member));
                    if (match != null)
                    {
                        member.Id = match.Id;
                        member.Name = match.Name;
                        member.LongName = match.LongName;
                        member.Agency = match.Agency;
                        member.Acronym = match.Acronym;
                        member.District = match.District;
                        member.Party = match.Party;
                        member.Phone = match.Phone;
                        member.Email = match.Email;
                    }

                    Console.WriteLine($"Updating {member}...");
                    Members.Update(member);
                }
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void FixupMembers()
        {
            Console.WriteLine("Retrieving member list...");
            var members = Members.ToList();

            var member = members.Single(i => i.LastName == "Tom");
            member.Party = "D";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Single(i => i.LastName == "Jarrett");
            member.Party = "D";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Single(i => i.LastName == "Miloscia");
            member.Party = "R";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Single(i => i.LastName == "Campbell");
            member.Party = "R";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Single(i => i.LastName == "Owen");
            member.Id = 321;
            member.Name = "Brad Owen";
            member.LongName = "Lieutenant Governor";
            member.Acronym = "LG";
            member.Agency = "Senate";
            member.Phone = "(360) 786-7700";
            member.Email = "ltgov@ltgov.wa.gov";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void MatchPolicyAreas()
        {
            var areas = PolicyAreas.ToList();
            foreach (var bill in Bills)
            {
                int bestScore = 0;
                short bestPolicy = 0;
                bill.PolicyArea = null;
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
                        bestPolicy = area.Id;
                    }
                }
                if (bestScore > 0)
                    bill.PolicyArea = bestPolicy;

                var match = areas.FirstOrDefault(i => i.Id == bestPolicy);
                Console.WriteLine($"{bill.AbbrTitle} -> {match?.Name}");
                Bills.Update(bill);
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
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
            Console.WriteLine("Preloading voting records...");
            var members = Members.OrderBy(i => i.LastName).ToList();
            var bills = Bills.ToList();

            foreach (var member in members)
            {
                Console.WriteLine($" Scoring {member}");
                var votes = Votes.Where(i => i.Member_Id == member.Id).ToList();

                for (short year = 2003; year < 2017; year += 2)
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
            var xmlSer = new DataContractSerializer(typeof(List<Member>));
            var list = new List<Member>();
            string[] biennia = { "2015-16", "2013-14", "2011-12", "2009-10", "2007-08", "2005-06", "2003-04" };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                foreach (var biennium in biennia)
                {
                    Console.WriteLine($"Retrieving members for {biennium} biennium...");
                    var response = client.GetAsync($"/SponsorService.asmx/GetSponsors?biennium={biennium}").Result;
                    var result = xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<Member>;
                    list.AddRange(result.Where(i => !list.Contains(i)));
                }
            }

            return list;
        }

        private string GetSponsors(short bill, short year)
        {
            var biennium = $"{year}-{(year + 1) % 100:D2}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                var response = client.GetAsync($"/LegislationService.asmx/GetSponsors?biennium={biennium}&billId={bill}").Result;
                return GetAcronyms(response.Content.ReadAsStringAsync().Result);
            }
        }

        private List<Committee> GetCommittees()
        {
            var xmlSer = new DataContractSerializer(typeof(List<Committee>));
            var list = new List<Committee>();
            string[] biennia = { "2015-16", "2013-14", "2011-12", "2009-10", "2007-08", "2005-06", "2003-04" };

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

        private string GetCommittees(short bill, short year)
        {
            var biennium = $"{year}-{(year + 1) % 100:D2}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
                var response = client.GetAsync($"/LegislationService.asmx/GetHearings?biennium={biennium}&billNumber={bill}").Result;
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
