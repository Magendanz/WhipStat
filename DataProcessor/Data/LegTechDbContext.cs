using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WhipStat.Models.LegTech;
using WhipStat.Models.PDC;

namespace WhipStat.Data
{
    public partial class LegTechDbContext : DbContext
    {
        public DbSet<Vote> Votes { get; set; }
        public DbSet<RollCall> RollCalls { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Member> Members { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=192.168.2.2;Database=LegTech;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>().HasKey(t => new { t.LastName, t.FirstName });
            modelBuilder.Entity<Bill>().HasKey(t => new { t.BillNumber, t.Year });
        }

        public void PopulateSubtables()
        {
            Console.WriteLine("Retrieving distinct list of member names...");
            var members = Members.ToList();
            var newMembers = Votes.Select(i => new Member {
                LastName = i.LastName,
                FirstName = i.FirstName })
                .Distinct().Where(i => !members.Contains(i)).ToList();

            Console.WriteLine("Adding to Members table...");
            Members.AddRange(newMembers);

            Console.WriteLine("Retrieving distinct list of bills...");
            var bills = Bills.ToList();
            var newBills = Votes.Select(i => new Bill {
                BillNumber = GetValue(i.LegNum),
                Year = GetValue(i.Biennium),
                ShortBillId = i.ShortBillId,
                AbbrTitle = i.AbbrTitle
            })
                .Distinct().Where(i => !bills.Contains(i)).ToList();

            Console.WriteLine("Adding to Bills table...");
            Bills.AddRange(newBills);

            Console.WriteLine("Retrieving distinct list of roll call votes...");
            var calls = RollCalls.ToList();
            var newCalls = Votes.Select(i => new RollCall {
                RollCall_Id = i.RollCall_Vote,
                BillNumber = GetValue(i.LegNum),
                Year = GetValue(i.Biennium),
                Yea_total = i.Yea_total,
                Nay_total = i.Nay_total,
                Excused_total = i.Excused_total,
                Absent_total = i.Absent_total,
                Agency = i.Agency,
                Motion_description = i.Motion_description,
                Amendment_description = i.Amendment_description
            })
                .Distinct().Where(i => !calls.Contains(i)).ToList();

            Console.WriteLine("Adding to RollCalls table...");
            RollCalls.AddRange(newCalls);

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void IdentifyMembers()
        {
            Console.WriteLine("Retrieving member list...");
            var members = Members.ToList();

            using (var db = new DonorDbContext())
            {
                foreach (var member in members)
                {
                    var pool = db.Campaigns.Where(i => i.LDis != null && i.Name.IndexOf(member.LastName, StringComparison.OrdinalIgnoreCase) == 0)
                        .Distinct().ToList();
                    if (pool.Count > 0)
                    {
                        var matches = pool.Where(i => i.Name.IndexOf(member.FirstName, StringComparison.OrdinalIgnoreCase) > 0).ToList();
                        var selection = PickMatch(matches.Count > 0 ? matches : pool, member);
                        member.Party = selection.Pty;
                        member.District = Convert.ToInt16(selection.LDis);
                    }
                    else
                    {
                        Console.WriteLine($"Manual entry required for {member}:");
                        Console.Write(" Party: ");
                        member.Party = Console.ReadLine();
                        Console.Write(" District: ");
                        member.District = Convert.ToInt16(Console.ReadLine());
                    }
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

            var member = members.Where(i => i.LastName == "Tom").Single();
            member.Party = "D";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Where(i => i.LastName == "Jarrett").Single();
            member.Party = "D";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Where(i => i.LastName == "Miloscia").Single();
            member.Party = "R";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Where(i => i.LastName == "Campbell").Single();
            member.Party = "R";
            Console.WriteLine($"Updating {member}...");
            Members.Update(member);

            member = members.Where(i => i.LastName == "Anderson" && i.FirstName == "Dave").Single();
            Console.WriteLine($"Deleting {member}...");
            Members.Remove(member);

            member = members.Where(i => i.LastName == "Morris" && i.FirstName == "Betty").Single();
            Console.WriteLine($"Deleting {member}...");
            Members.Remove(member);

            member = members.Where(i => i.LastName == "Mielke" && i.FirstName == "Todd").Single();
            Console.WriteLine($"Deleting {member}...");
            Members.Remove(member);

            member = members.Where(i => i.LastName == "Cooper" && i.FirstName == "David").Single();
            Console.WriteLine($"Deleting {member}...");
            Members.Remove(member);

            member = members.Where(i => i.LastName == "O'Brien" && i.FirstName == "John").Single();
            Console.WriteLine($"Deleting {member}...");
            Members.Remove(member);

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void FixupVotes()
        {
            Console.WriteLine("Retrieving bogus votes...");
            var votes = Votes.Where(i => i.RollCall_Vote == 1752).ToList();

            var vote = votes.Where(i => i.LastName == "Anderson").Single();
            vote.FirstName = "Glenn";
            Console.WriteLine($"Updating {vote}...");
            Votes.Update(vote);

            vote = votes.Where(i => i.LastName == "Cooper").Single();
            vote.FirstName = "Mike";
            Console.WriteLine($"Updating {vote}...");
            Votes.Update(vote);

            vote = votes.Where(i => i.LastName == "Mielke").Single();
            vote.FirstName = "Tom";
            Console.WriteLine($"Updating {vote}...");
            Votes.Update(vote);

            vote = votes.Where(i => i.LastName == "Morris").Single();
            vote.FirstName = "Jeff";
            Console.WriteLine($"Updating {vote}...");
            Votes.Update(vote);

            vote = votes.Where(i => i.LastName == "O'Brien").Single();
            vote.FirstName = "Al";
            Console.WriteLine($"Updating {vote}...");
            Votes.Update(vote);

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        public void ScoreBills()
        {
            Console.WriteLine("Preloading member and bill lists...");
            var bills = Bills.ToList();
            var members = Members.ToList();
            var rollCalls = RollCalls.Where(i => String.IsNullOrWhiteSpace(i.Amendment_description)).ToList();

            foreach (var bill in bills)
            {
                var calls = rollCalls.Where(i => i.BillNumber == bill.BillNumber && i.Year == bill.Year);
                foreach (var call in calls)
                {
                    var votes = Votes.Where(i => i.RollCall_Vote == call.RollCall_Id).ToList();
                    double RTotal = 0, DTotal = 0, RCount = 0, DCount = 0;
                    foreach (var vote in votes)
                    {
                        var member = members.Where(i => i.LastName == vote.LastName && i.FirstName == vote.FirstName).Single();
                        var supported = vote.Member_vote.StartsWith("Y", StringComparison.OrdinalIgnoreCase);
                        if (member.Party == "R")
                        {
                            ++RTotal;
                            if (supported)
                                ++RCount;
                        }
                        if (member.Party == "D")
                        {
                            ++DTotal;
                            if (supported)
                                ++DCount;
                        }
                    }

                    // Roll call score is the percentage difference between Republican and Democrat support
                    call.Score = (RCount / RTotal - DCount / DTotal) * 100;
                    RollCalls.Update(call);
                }

                // Bill score is the average of its roll call scores
                bill.Score = calls.Average(i => i.Score);

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
            var calls = RollCalls.Where(i => String.IsNullOrWhiteSpace(i.Amendment_description)
//                                    && (i.Year > 2012)
                                    ).ToList();
            var votes = Votes.Where(i => String.IsNullOrWhiteSpace(i.Amendment_description)
//                                    && (i.Biennium == "2013-14" || i.Biennium == "2015-16")
                                    && (i.Member_vote[0] == 'Y' || i.Member_vote[0] == 'N'))
                                    .ToList();

            foreach (var member in members)
            {
                var mv = votes.Where(i => i.LastName == member.LastName && i.FirstName == member.FirstName);

                // Member score is the average of all roll call scores
                member.Score = mv.Average(i => Score(i, calls));

                Console.WriteLine($"{member} - Score: {member.Score:N1}");
                Members.Update(member);
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving changes...");
            SaveChanges();
        }

        private short GetValue(string str)
        {
            // Bill number or year should ALWAYS be four characters long
            return Convert.ToInt16(str.Substring(0, 4));
        }

        private Campaign PickMatch(List<Campaign> options, Member member)
        {
            if ((options.Count > 1) && (options.GroupBy(i => i.LDis + i.Pty).Count() > 1))
            {
                while (true)
                {
                    Console.WriteLine($"Pick the option that best matches '{member}':");
                    foreach (var option in options)
                    {
                        Console.Write($" {option.Name}?");
                        var key = Console.ReadKey();
                        Console.WriteLine();
                        if (key.KeyChar == 'y')
                            return option;
                    }
                }
            }

            return options.Last();
        }

        private double? Score(Vote v, List<RollCall> rc)
        {
            var call = rc.Where(j => j.RollCall_Id == v.RollCall_Vote).Single();
            if (v.Member_vote.StartsWith("Y"))
                return call.Score;
            else if (v.Member_vote.StartsWith("N"))
                return -call.Score;
            else
                return 0;
        }
    }
}
