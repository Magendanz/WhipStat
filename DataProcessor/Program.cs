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
                db.GetMembers();
                db.GetCommittees();
                db.GetBills();
                db.UpdateBillInfo();
                db.GetRollCalls();
                db.ScoreBills();
                db.ScoreMembers();
                //db.RenamePhotos(@"D:\Pictures\Legislature\Thumbnails\2019\Senate");
            }

            using (var db = new RecordDbContext())
            {
                //db.GetAVStats("2019-20", 1087);
                //db.GetAVStats("2019-20", 1324);
                //db.GetAVStats("2019-20", 1652);
                //db.GetAVStats("2019-20", 1873);
                //db.GetAVStats("2019-20", 2158);
                //db.GetAVStats("2019-20", 2167);
                //db.GetAVStats("2019-20", 5581);
                //db.GetAVStats("2019-20", 5993);
                //db.GetAVStats("2019-20", 5997);
                //db.GetAVStats("2019-20", 5998);
                //db.GetAVStats("2019-20", 6004);
                //db.GetAVStats("2019-20", 6016);
                //db.GetAVStats("2019-20", 8200);
                //db.GetAVStats("2019-20", 1000);
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