using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace WhipStat.Models.LWS
{
    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class RollCall
    {
        [DataMember(IsRequired = true, Order = 0)]
        public string Agency { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string BillId { get; set; }
        [DataMember(IsRequired = true, Order = 2)]
        public string Biennium { get; set; }
        [DataMember(IsRequired = true, Order = 3)]
        public string Motion { get; set; }
        [DataMember(IsRequired = true, Order = 4)]
        public short SequenceNumber { get; set; }
        [DataMember(IsRequired = true, Order = 5)]
        public DateTime VoteDate { get; set; }
        [DataMember(IsRequired = true, Order = 6)]
        public VoteCount YeaVotes { get; set; }
        [DataMember(IsRequired = true, Order = 7)]
        public VoteCount NayVotes { get; set; }
        [DataMember(IsRequired = true, Order = 8)]
        public VoteCount AbsentVotes { get; set; }
        [DataMember(IsRequired = true, Order = 9)]
        public VoteCount ExcusedVotes { get; set; }
        [DataMember(IsRequired = true, Order = 10)]
        public List<Vote> Votes { get; set; }

        public override string ToString()
        {
            var date = $"{VoteDate:u}".Remove(10);
            return $"{date}-{Agency[0]}{SequenceNumber:D3}";
        }

        public override bool Equals(Object obj)
        {
            return VoteDate == ((RollCall)obj).VoteDate
                && Agency == ((RollCall)obj).Agency
                && SequenceNumber == ((RollCall)obj).SequenceNumber;
        }

        public override int GetHashCode()
        {
            DateTime StartDate = new DateTime(1991, 1, 1);   // Information only available back to 1991
            string[] Agencies = { "Senate", "House" };

            var days = (int)(VoteDate - StartDate).TotalDays;
            var agency = Array.IndexOf(Agencies, Agency);
            return (days << 14) + (agency << 10) + SequenceNumber;
        }
    }

    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class VoteCount
    {
        [DataMember(IsRequired = true, Order = 0)]
        public short Count { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string MembersVoting { get; set; }
    }

    [DataContract(Namespace = "http://WSLWebServices.leg.wa.gov/")]
    public class Vote
    {
        [DataMember(IsRequired = true, Order = 0)]
        public int MemberId { get; set; }
        [DataMember(IsRequired = true, Order = 1)]
        public string Name { get; set; }
        [DataMember(IsRequired = true, Order = 2)]
        public string VOte { get; set; }
    }
}
