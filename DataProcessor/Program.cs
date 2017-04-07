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
                //db.FixupMembers();
                //db.MatchPolicyAreas();
                //db.ScoreBills();
                db.ScoreMembers();
            }
        }
    }
}