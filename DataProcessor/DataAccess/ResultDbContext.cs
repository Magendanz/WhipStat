using System;
using System.Collections.Generic;
using System.IO;
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

        public string GetResults(string path, int district)
        {
            var sb = new StringBuilder("Year\tElection\tRace\tEntry\t");

            // Get current list of precincts (not the ones at the time of the election)
            var precincts = Precincts.Where(i => i.LEG == district).OrderBy(i => i.PrecinctName).ToList();
            var names = precincts.Select(i => i.PrecinctName);
            var codes = precincts.Select(i => i.PrecinctCode);
            sb.AppendJoin("\t", names);
            sb.AppendLine("\tAverage");

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                Console.WriteLine(line);
                var args = line.Split(",");
                var year = Convert.ToInt16(args[0]);
                var election = args[1];
                var race = args[2];
                var entry = args[3];
                sb.Append($"{year}\t{election}\t{race}\t{entry}\t");

                // Get the requested election results
                var results = Results.Where(i => codes.Contains(i.PrecinctCode) && i.Year == year && i.Election == election && i.Race == race).ToList();

                double totalmatches = results.Where(i => i.CounterType == entry).Sum(i => i.Count);
                double totalballots = results.Where(i => i.CounterType == "Times Counted").Sum(i => i.Count);
                double totalvotes = totalballots - results.Where(i => i.CounterType.EndsWith(" Voted")).Sum(i => i.Count);
                double totalvoters = results.Where(i => i.CounterType == "Registered Voters").Sum(i => i.Count);
                double percentvote = totalmatches / totalvotes;
                double percentturnout = totalballots / totalvoters;

                var turnout = entry == "Times Counted";
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

                    sb.AppendFormat("{0:P2}\t", turnout ? (ballots / voters - percentturnout) : (matches / votes - percentvote));
                }
                sb.AppendLine(String.Format("{0:P2}", turnout ? percentturnout : percentvote));
            }

            return sb.ToString();
        }
    }
}
