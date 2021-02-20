using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

using WhipStat.Helpers;
using WhipStat.Models.LegTech;

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

        public readonly string[] biennia = { "2019-20", "2017-18", "2015-16", "2013-14", "2011-12", "2009-10",
            "2007-08", "2005-06", "2003-04", "2001-02", "1999-00", "1997-98", "1995-96", "1993-94", "1991-92" };

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=192.168.2.2;Database=LegTech;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>().HasKey(t => new { t.BillId, t.Biennium });
            modelBuilder.Entity<Vote>().HasKey(t => new { t.RollCallId, t.MemberId });
            modelBuilder.Entity<Score>().HasKey(t => new { t.MemberId, t.Year, t.PolicyArea });
            modelBuilder.Entity<Testimony>().HasKey(t => new { t.LastName, t.FirstName, t.TimeOfSignIn });
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
                    if (!String.IsNullOrWhiteSpace(member.Party) && !members.Contains(member))
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
            Console.WriteLine($"Syncing legislation from LWS...");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Bills");
            var bills = new HashSet<Bill>();

            foreach (var biennium in biennia)
            {
                Console.WriteLine($"Retrieving bills for {biennium} biennium...");
                var year = Convert.ToInt32(biennium.Substring(0, 4));
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
            Console.WriteLine($"Updating bill info...");
            var areas = PolicyAreas.ToList();

            foreach (var bill in Bills.ToList())
            {
                Console.WriteLine($"  {bill}");
                try
                {
                    bill.AbbrTitle = GetTitle(bill);
                    //bill.Sponsors = GetSponsors(bill);        // We're currently not using this
                    bill.Committees = GetCommittees(bill);
                    bill.PolicyArea = GetBestPolicyArea(bill, areas)?.Id;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Update failed for bill {bill}!  Exception: {ex.Message}");
                }
                finally
                {
                    Bills.Update(bill);
                }
            }

            Console.WriteLine("Saving changes...\n");
            SaveChanges();
        }

        private string GetTitle(Bill bill)
            => LwsAccess.GetLegislation(bill.Biennium, bill.BillNumber).First(i => i.BillId == bill.BillId).ShortDescription;

        private string GetSponsors(Bill bill)
            => String.Join(",", LwsAccess.GetSponsors(bill.Biennium, bill.BillId).Select(i => i.Acronym));

        private string GetCommittees(Bill bill)
        {
            var list = new List<String>();
            foreach (var hearing in LwsAccess.GetHearings(bill.Biennium, bill.BillNumber).ToList())
                list.AddRange(hearing.CommitteeMeeting.Committees.Select(i => i.Acronym));

            return String.Join(",", list.Distinct());
        }

        private PolicyArea GetBestPolicyArea(Bill bill, List<PolicyArea> areas)
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

        public void GetRollCalls()
        {
            Console.WriteLine($"Syncing roll calls from LWS...");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.RollCalls");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Votes");
            var calls = new HashSet<RollCall>();
            var votes = Votes.ToList();
            var members = Members.ToList();

            foreach (var bill in Bills.ToList())
            {
                Console.WriteLine($"  {bill}");
                foreach (var roll in LwsAccess.GetRollCalls(bill.Biennium, bill.BillNumber))
                {
                    var id = roll.GetHashCode();

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

                    double score = ((RTotal > 0 ? RCount / RTotal : 0) - (DTotal > 0 ? DCount / DTotal : 0)) * 100;
                    calls.Add(new RollCall()
                    {
                        Id = id,
                        Agency = roll.Agency,
                        BillId = roll.BillId,
                        Biennium = roll.Biennium,
                        Motion = roll.Motion,
                        SequenceNumber = roll.SequenceNumber,
                        VoteDate = roll.VoteDate,
                        YeaVotes = roll.YeaVotes.Count,
                        NayVotes = roll.NayVotes.Count,
                        AbsentVotes = roll.AbsentVotes.Count,
                        ExcusedVotes = roll.ExcusedVotes.Count,
                        Score = score
                    });

                    votes.AddRange(roll.Votes.Select(i => new Vote()
                    {
                        RollCallId = id,
                        MemberId = i.MemberId,
                        MemberVote = i.VOte.Substring(0, 1).ToUpper()
                    }));
                }
            }

            Console.WriteLine("Saving changes...\n");
            RollCalls.AddRange(calls);
            Votes.AddRange(votes);
            SaveChanges();
        }

        public void ScoreBills()
        {
            Console.WriteLine($"Calculating bill scores...");
            Console.WriteLine("Preloading floor records...");

            var bills = Bills.ToList();
            var calls = RollCalls.ToList();

            foreach (var bill in bills)
            {
                Console.WriteLine($" Scoring {bill}");
                var votes = calls.Where(i => i.BillId == bill.BillId && i.Biennium == bill.Biennium).ToList();
                if (votes.Count() > 0)
                    bill.Score = votes.Average(i => i.Score);

            }

            Console.WriteLine("Saving changes...\n");
            SaveChanges();
        }

        public void ScoreMembers()
        {
            Console.WriteLine($"Calculating member scores...");
            Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Scores");
            var scores = new HashSet<Score>();

            Console.WriteLine("Preloading voting records...");
            var bills = Bills.ToList();
            var calls = RollCalls.ToList();

            foreach (var member in Members.OrderBy(i => i.LastName).ToList())
            {
                Console.WriteLine($" Scoring {member}");
                var votes = Votes.Where(i => i.MemberId == member.Id).ToList();

                foreach (var biennium in biennia)
                {
                    for (short area = 0; area <= PolicyAreas.Count(); ++area)
                    {
                        var tb = bills.Where(i => i.Biennium == biennium && (area == 0 || i.PolicyArea == area)).ToList();
                        var tc = calls.Where(i => tb.Exists(j => i.BillId == j.BillId && i.Biennium == j.Biennium)).ToList();

                        double total = 0;
                        int count = 0;
                        foreach (var vote in votes)
                        {
                            var call = tc.FirstOrDefault(i => i.Id == vote.RollCallId);
                            if (call != null)
                            {
                                switch (vote.MemberVote)
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
                            var year = Convert.ToInt16(biennium.Substring(0, 4));
                            scores.Add(new Score { MemberId = member.Id, Year = year, PolicyArea = area, Total = total, Count = count });
                        }
                    }
                }
            }

            Console.WriteLine("Saving changes...\n");
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
                                    LastName = t.LastName.Truncate(64),
                                    FirstName = t.FirstName.Truncate(64),
                                    Organization = t.Organization.Truncate(256),
                                    Street = t.Street.Truncate(64),
                                    City = t.City.Truncate(32),
                                    State = t.State.Truncate(2),
                                    Zip = t.Zip.Truncate(10),
                                    EmailAddress = t.EmailAddress.Truncate(64),
                                    PhoneNumber = t.PhoneNumber.Truncate(16),
                                    BillId = i.BillId.Truncate(16),
                                    Position = t.Position.Truncate(16),
                                    IsSpeaking = t.IsSpeaking,
                                    OutOfTown = t.OutOfTown,
                                    CalledUp = t.CalledUp,
                                    NoShow = t.NoShow,
                                    TimeOfSignIn = t.TimeOfSignIn
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
    }
}
