using System;

using WhipStat.DataAccess;

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
                //db.ScoreBills();
                //db.ScoreMembers();
                //db.GetTestimony(2014, 2021);  // Obsolete
                //db.ImportTestimonyDir(@"C:\Users\Chad\Projects\WhipStat\Datasets\LegTech\CSI Testifier Data");
                //db.FixupTestimony(@"C:\Users\Chad\Projects\WhipStat\Datasets\LegTech\CSI Testifier Data\House\CSI Testfier Data 2022.csv");
                //db.GetOrganizations();
                //db.MatchOrganizations();
                //db.ScoreAdvocacyRecords();
                //db.ScoreVotingRecords();
                //db.GetMeasures("Ballot Measures by LD (Support).csv", "Ballot Measures by LD (Oppose).csv");
                //db.GetPhotos("2023-24");
                //db.RenamePhotos(@"D:\Pictures\Legislature\Thumbnails\2019\Senate");
            }

            using (var db = new RecordDbContext())
            {
                //db.GetAVStats("2021-22", 5974, 2076);
                //db.GenerateMemberReports("2021-22", "House", "R", 2014);
                //db.BiparisanBillSponsorship();
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