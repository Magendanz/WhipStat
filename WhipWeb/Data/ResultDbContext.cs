using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WhipStat.Models.Elections;

namespace WhipStat.Data
{
    public partial class ResultDbContext : DbContext
    {
        public DbSet<Result> Results { get; set; }
        public DbSet<Precinct> Precincts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=192.168.2.2;Database=Elections;Trusted_Connection=True;");
        }

        public string GetResults(int district, int year, string election, string race, string entry)
        {
            var sb = new StringBuilder();

            // Get current list of precincts (not the ones at the time of the election)
            var precincts = Precincts.Where(i => i.LEG == district).OrderBy(i => i.PrecinctName).ToList();
            var codes = precincts.Select(i => i.PrecinctCode);

            // Get the requested election results
            var results = Results.Where(i => codes.Contains(i.PrecinctCode) && i.Year == year && i.Election == election && i.Race == race).ToList();

            double totalcount = results.Where(i => i.CounterType == entry).Sum(i => i.Count);
            double totalballots = results.Where(i => i.CounterType == "Times Counted").Sum(i => i.Count);
            double totalvoters = results.Where(i => i.CounterType == "Registered Voters").Sum(i => i.Count);
            double percentvote = totalcount / totalballots;
            double percentturnout = totalballots / totalvoters;

            // TODO: This is a tab-delimited list rignt now, but we should switch to CSV output
            sb.AppendLine("Precinct\tResult\tTurnout");
            foreach (var precinct in precincts)
            {
                double count = 0, ballots = 0, voters = 0;
                var result = results.Where(i => i.PrecinctCode == precinct.PrecinctCode);
                if (result.Any())
                {
                    count = result.Where(i => i.CounterType == entry).Select(i => i.Count).Single();
                    ballots = result.Where(i => i.CounterType == "Times Counted").Select(i => i.Count).Single();
                    voters = result.Where(i => i.CounterType == "Registered Voters").Select(i => i.Count).Single();
                }
                sb.AppendLine(String.Format("{0}\t{1:P2}\t{2:P2}", precinct.PrecinctName, count / ballots - percentvote, ballots / voters - percentturnout));
            }
            sb.AppendLine(String.Format("Average:\t{0:P2}\t{1:P2}", percentvote, percentturnout));

            return sb.ToString();
        }
    }
}
