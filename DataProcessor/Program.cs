using System;
using System.IO;
using WhipStat.Data;

namespace DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting data processing...");

            using (var db = new RecordDbContext())
            {
                //db.ProcessNewVotes();
                //db.UpdateMembers();
                //db.FixupMembers();
                //db.IdentifyPhotos();
                //db.UpdateCommittees();
                //db.UpdatePolicyAreas();
                //db.UpdateBillInfo();
                //db.ScoreBills();
                //db.ScoreMembers();
            }

            using (var db = new DonorDbContext())
            {
                //db.GenerateAggregrates();
            }

            using (var db = new ResultDbContext())
            {
                //File.WriteAllText("Candidates.tsv", db.GetResults("LD5 Republican Candidatess.txt", 5));
                //File.WriteAllText("Ballot Measures.tsv", db.GetResults("Conservative Ballot Measures.txt", 5));
                //File.WriteAllText("Turnout.tsv", db.GetResults("Precinct Turnout.txt", 5));
            }

            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}