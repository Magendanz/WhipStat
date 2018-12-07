using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

using WhipStat.Models.LegTech;
using WhipStat.Models.PDC;

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


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=192.168.2.2;Database=LegTech;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>().HasKey(t => new { t.BillId, t.Biennium });
            modelBuilder.Entity<Vote>().HasKey(t => new { t.RollCallId, t.MemberId });
            modelBuilder.Entity<Score>().HasKey(t => new { t.MemberId, t.Year, t.PolicyArea });
        }

        public string GetLeaderboard(string chamber, short area, short begin, short end)
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
    }
}
