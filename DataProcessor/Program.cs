using System;
using WhipStat.Data;

namespace DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting data processing...");

            using (var db = new LegTechDbContext())
            {
                //db.PopulateSubtables();
                //db.IdentifyMembers();
                //db.FixupMembers();
                //db.FixupVotes();
                //db.ScoreBills();
                db.ScoreMembers();
            }
        }
    }
}