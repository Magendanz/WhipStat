using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using WhipStat.Models.VRDB;

namespace WhipStat.Data
{
    public partial class VoterDbContext : DbContext
    {
        public DbSet<Voter> Voters { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Transaction> History { get; set; }
        public DbSet<ZipCode> ZipCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<VoterDbContext>();
            var configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration["VoterDb:SqlConnectionString"]);
        }

        public Dictionary<int, string> GetPrecincts(int district)
        {
            return Districts.Where(d => d.DistrictType == "Legislative").AsEnumerable()
                .Where(d => GetValue(d.DistrictCode) == district)
                .GroupBy(d => d.PrecinctCode).ToDictionary(g => g.Key, g => g.First().PrecinctName);
        }

        public string GetVoters(int district, int precinct)
        {
            var sb = new StringBuilder();

            var results = Voters.Where(v => v.LegislativeDistrict == district && v.PrecinctCode == precinct)
                .OrderBy(v => v.LName).ToList();

            // TODO: This is a tab-delimited list rignt now, but we should switch to CSV output
            sb.AppendLine("LName\tFName\tMName\tGender\tBirthdate\tCity\tState\tZipCode\tVoterID");
            foreach (var item in results)
                sb.AppendLine($"{item.LName}\t{item.FName}\t{item.MName}\t{item.Gender}\t{item.Birthdate:MM/dd/yyyy}\t{item.RegCity}\t{item.RegState}\t{item.RegZipCode}\t{item.StateVoterID}");

            return sb.ToString();
        }
        private static int GetValue(string str)
        {
            // These legislative district codes are very inconsistently formatted, so we're going to clean it up
            return Convert.ToInt32(new string(str.Where(c => char.IsDigit(c)).ToArray()));
        }
    }
}
