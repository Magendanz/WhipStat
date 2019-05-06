using System;
using System.IO;
using WhipStat.Data;
using WhipStat.Services;

namespace DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting data processing...\n");

            using (var db = new RecordDbContext())
            {
                //db.GetMembers();
                //db.GetCommittees();
                //db.GetBills();
                //db.UpdateBillInfo();
                //db.GetRollCalls();
                //db.ScoreMembers();
                db.RenamePhotos(@"D:\Pictures\Legislature\Thumbnails\2019\Senate");
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
        }
    }
}