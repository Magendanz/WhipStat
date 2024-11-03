using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using WhipStat.Models.Elections;

namespace WhipStat.DataAccess
{
    public partial class ResultDbContext : DbContext
    {
        public DbSet<Result> Results { get; set; }
        public DbSet<Precinct> Precincts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<ResultDbContext>();
            var configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration["ResultDb:SqlConnectionString"]);
        }

        public string GetResults(int district, int year, string election, string race, string entry)
        {
            var sb = new StringBuilder();

            // Get current list of precincts (not the ones at the time of the election)
            var precincts = Precincts.Where(i => i.LEG == district).OrderBy(i => i.PrecinctName).ToList();
            var codes = precincts.Select(i => i.PrecinctCode);

            // Get the requested election results
            var results = Results.Where(i => codes.Contains(i.PrecinctCode) && i.Year == year && i.Election == election && i.Race == race).ToList();

            double totalmatches = results.Where(i => i.CounterType == entry).Sum(i => i.Count);
            double totalballots = results.Where(i => i.CounterType == "Times Counted").Sum(i => i.Count);
            double totalvotes = totalballots - results.Where(i => i.CounterType.EndsWith(" Voted")).Sum(i => i.Count);
            double totalvoters = results.Where(i => i.CounterType == "Registered Voters").Sum(i => i.Count);
            double percentvote = totalmatches / totalvotes;
            double percentturnout = totalballots / totalvoters;

            // TODO: This is a tab-delimited list rignt now, but we should switch to CSV output
            sb.AppendLine("Precinct\tResult\tTurnout");
            foreach (var precinct in precincts)
            {
                double matches = 0, ballots = 0, votes = 0, voters = 0;
                var result = results.Where(i => i.PrecinctCode == precinct.PrecinctCode);
                if (result.Any())
                {
                    matches = result.Single(i => i.CounterType == entry).Count;
                    ballots = result.Single(i => i.CounterType == "Times Counted").Count;
                    votes = ballots - result.Where(i => i.CounterType.EndsWith(" Voted")).Sum(i => i.Count);
                    voters = result.Single(i => i.CounterType == "Registered Voters").Count;
                }
                sb.AppendLine(string.Format("{0}\t{1:P2}\t{2:P2}", precinct.PrecinctName, matches / votes - percentvote, ballots / voters - percentturnout));
            }
            sb.AppendLine(string.Format("Average:\t{0:P2}\t{1:P2}", percentvote, percentturnout));

            return sb.ToString();
        }
    }
}
