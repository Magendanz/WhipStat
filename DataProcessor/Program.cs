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
                db.IdentifyPhotos();
                //db.UpdateBillInfo();
                //db.UpdateCommittees();
                //db.UpdatePolicyAreas();
                //db.ScoreBills();
                //db.ScoreMembers();
            }

            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}