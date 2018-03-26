using System;
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

            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}