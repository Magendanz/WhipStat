using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WhipStat.Models.PDC;

namespace WhipStat.Data
{
    public partial class DonorDbContext : DbContext
    {
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Tally> Subtotals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=192.168.2.2;Database=PDC;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tally>().HasKey(t => new { t.DonorId, t.Year, t.Jurisdiction });
        }

        public string GetDonors(string party, string zips)
        {
            var sb = new StringBuilder();

            var results = Donations.Where(d => d.Pty == party && zips.Contains(d.Zip))
                .OrderBy(d => d.Name).ToList();

            // TODO: This is a tab-delimited list rignt now, but we should switch to CSV output
            sb.AppendLine("Name\tAddress\tCity\tZipCode\tDate\tAmount");
            foreach (var item in results)
                sb.AppendLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4:MM/dd/yyyy}\t{5:C}", item.Name, item.Address, item.City, item.Zip, item.Rcpt_Date, item.Amount));

            return sb.ToString();
        }

        public void GenerateAggregrates()
        {
            var scores = Subtotals.ToList();

            Console.WriteLine("Loading donor list...");
            foreach (var donor in Donors.ToList())
            {
                Console.WriteLine($"  Analyzing '{donor.Name}'...");
                var tallies = scores.Where(i => i.DonorId == donor.Id);
                donor.Aggregate = tallies.Sum(i => i.Total);
            }

            // Attempt to save changes to the database
            Console.WriteLine("Saving final changes...");
            SaveChanges();
        }
    }
}
