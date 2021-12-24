using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using WhipStat.Helpers;
using WhipStat.Models.LWS;

namespace WhipStat.DataAccess
{
    public class LwsAccess
    {
        readonly static Uri lwsBase = new Uri("http://wslwebservices.leg.wa.gov");
        readonly static Uri appBase = new Uri("https://app.leg.wa.gov");

        static public List<LegislationInfo> GetLegislationByYear(int year)
        => GetList<LegislationInfo>($"/LegislationService.asmx/GetLegislationByYear?year={year}");

        static public List<Legislation> GetLegislation(string biennium, int billNumber)
        => GetList<Legislation>($"/LegislationService.asmx/GetLegislation?biennium={biennium}&billNumber={billNumber}");

        static public List<RollCall> GetRollCalls(string biennium, int billNumber)
        => GetList<RollCall>($"/LegislationService.asmx/GetRollCalls?biennium={biennium}&billNumber={billNumber}");

        static public List<Sponsor> GetSponsors(string biennium, string billId)
        => GetList<Sponsor>($"/LegislationService.asmx/GetSponsors?biennium={biennium}&billId={billId}");

        static public List<Hearing> GetHearings(string biennium, int billNumber)
        => GetList<Hearing>($"/LegislationService.asmx/GetHearings?biennium={biennium}&billNumber={billNumber}");

        static public List<Member> GetMembers(string biennium)
        => GetList<Member>($"/SponsorService.asmx/GetSponsors?biennium={biennium}");

        static public List<Committee> GetCommittees(string biennium)
        => GetList<Committee>($"/CommitteeService.asmx/GetCommittees?biennium={biennium}");

        static public List<CommitteeMeeting> GetCommitteeMeetings(DateTime beginDate, DateTime endDate)
        => GetList<CommitteeMeeting>($"/CommitteeMeetingService.asmx/GetCommitteeMeetings?beginDate={beginDate:d}&endDate={endDate:d}");

        static public List<AgendaItem> GetAgendaItems(DateTime beginDate, DateTime endDate)
        {
            var meetings = GetCommitteeMeetings(beginDate, endDate);
            var results = new List<AgendaItem>();
            foreach (var meeting in meetings)
            {
                try
                {
                    results.AddRange(GetAgendaItems(meeting.Agency, meeting.Committees.First().Name, meeting.Date));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return results;
        }

        static public List<AgendaItem> GetAgendaItems(string biennium, int billNumber)
        {
            var hearings = GetHearings(biennium, billNumber);
            var results = new List<AgendaItem>();
            foreach (var hearing in hearings)
            {
                try
                {
                    results.AddRange(GetAgendaItems(hearing.CommitteeMeeting.Agency, hearing.CommitteeMeeting.Committees.First().Name, hearing.CommitteeMeeting.Date)
                    .Where(i => i.BillId == hearing.BillId));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return results;
        }

        static public List<AgendaItem> GetAgendaItems(string agency, string committee, DateTime date)
            => WebClient.SendAsync<List<AgendaItem>>(HttpMethod.Get,
                new Uri(appBase, $"/api/CommitteeMeetings/{agency}/{Uri.EscapeDataString(committee)}/{date:s}/AgendaItems")).Result;

        static public List<T> GetList<T>(string requestUri)
        {
            var response = GetResponse(requestUri);
            var xmlSer = new DataContractSerializer(typeof(List<T>));
            return xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<T>;
        }

        static public HttpResponseMessage GetResponse(string requestUri)
        {
            var retryCount = 3;

            using var client = new HttpClient();
            client.BaseAddress = lwsBase;
  retry:
            try
            {
                return client.GetAsync(requestUri).Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Thread.Sleep(10000);    // Wait for 10 seconds, just to give server a breath
                if (retryCount-- > 0)
                    goto retry;

                Debug.WriteLine("Retries exhausted! Request: {requestUri}");
                return new HttpResponseMessage();
            }
        }
    }
}
