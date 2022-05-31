using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

using WhipStat.Models.LegTech;
using Microsoft.Extensions.Configuration;

namespace WhipStat.Data
{
    public partial class RecordDbContext : DbContext
    {
        public DbSet<Vote> Votes { get; set; }
        public DbSet<RollCall> RollCalls { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Models.LWS.Member> Members { get; set; }
        public DbSet<Models.LWS.Committee> Committees { get; set; }
        public DbSet<PolicyArea> PolicyAreas { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Testimony> Testimonies { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AdvocacyRecord> AdvocacyRecords { get; set; }
        public DbSet<VotingRecord> VotingRecords { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<RecordDbContext>();
            var configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration["RecordDb:SqlConnectionString"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>().HasKey(t => new { t.BillId, t.Biennium });
            modelBuilder.Entity<Vote>().HasKey(t => new { t.RollCallId, t.MemberId });
            modelBuilder.Entity<Score>().HasKey(t => new { t.MemberId, t.Year, t.PolicyArea });
            modelBuilder.Entity<AdvocacyRecord>().HasKey(t => new { t.Id, t.BillNumber, t.Biennium });
            modelBuilder.Entity<VotingRecord>().HasKey(t => new { t.Id, t.BillNumber, t.Biennium });
        }

        public string GetPartisanLeaderboard(short area, string chamber, short begin, short end)
        {
            var sb = new StringBuilder();
            var members = Members.Where(i => chamber == "0" || i.Agency == chamber).OrderBy(i => i.LastName).ToList();

            sb.AppendLine("Last Name\tFirst Name\tDistrict\tParty\tScore");
            foreach (var member in members)
            {
                var scores = Scores.Where(i => i.MemberId == member.Id && i.PolicyArea == area && i.Year >= begin && i.Year <= end).ToList();
                var total = scores.Sum(i => i.Total);
                var count = scores.Sum(i => i.Count);
                if (count > 10)
                    sb.AppendLine($"{member.LastName}\t{member.FirstName}\t{member.District}\t{member.Party}\t{total / count:N1}");
            }

            return sb.ToString();
        }

        public string GetMemberLeaderboard(int organization, string chamber, short begin, short end)
        {
            var sb = new StringBuilder();
            var members = Members.Where(i => chamber == "0" || i.Agency == chamber).OrderBy(i => i.LastName).ToList();
            var records = AdvocacyRecords.Where(i => i.Id == organization && i.Biennium.CompareTo($"{begin}") > 0 && i.Biennium.CompareTo($"{end}") < 0)
                .Join(VotingRecords, i => new { Y = i.Biennium, N = i.BillNumber }, j => new { Y = j.Biennium, N = j.BillNumber },
                (i, j) => new { j.Id, j.Sponsor, j.Votes, j.Support, Count = i.Votes, Testify = i.Sponsor, Favor = i.Support }).ToList();

            sb.AppendLine("Last Name\tFirst Name\tDistrict\tParty\tCorrelation");
            foreach (var member in members)
            {
                var tally = records.Where(i => i.Id == member.Id);
                int n = tally.Count();
                if (n > 0)
                {
                    var score = tally.Average(i => 100.0 * i.Support / i.Votes * i.Favor / i.Count);
                    sb.AppendLine($"{member.LastName}\t{member.FirstName}\t{member.District}\t{member.Party}\t{score:N1}");
                }
            }
            return sb.ToString();
        }
    }
}
