using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using WhipStat.Models.LWS;

namespace WhipStat.Services
{
    public class LWS
    {
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

        static public List<T> GetList<T>(string requestUri)
        {
            var response = GetResponse(requestUri);
            var xmlSer = new DataContractSerializer(typeof(List<T>));
            return xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<T>;
        }

        static public HttpResponseMessage GetResponse(string requestUri)
        {
            var retryCount = 3;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
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
}
