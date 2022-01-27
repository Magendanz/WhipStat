using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using WhipStat.Helpers;
using WhipStat.Models.PDC;

namespace WhipStat.DataAccess
{
    public static class PdcAccess
    {
        readonly static Uri baseUri = new Uri("https://data.wa.gov/resource/");
        const int limit = 262144;

        public static List<Agent> GetAgents(short? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&employment_year={year}";
            return WebClient.SendAsync<List<Agent>>(HttpMethod.Get,
                new Uri(baseUri, $"bp5b-jrti.json?{query}")).Result;
        }

        public static List<Agency> GetAgencies(short? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&year={year}";
            return WebClient.SendAsync<List<Agency>>(HttpMethod.Get,
                new Uri(baseUri, $"c4ag-3cmj.json?{query}")).Result;
        }

        public static List<Employer> GetEmployers(short? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&employment_year={year}";
            return WebClient.SendAsync<List<Employer>>(HttpMethod.Get,
                new Uri(baseUri, $"xhn7-64im.json?{query}")).Result;
        }

        public static List<EmployerSum> GetEmployerSummaries(short? year = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&Year={year}";
            return WebClient.SendAsync<List<EmployerSum>>(HttpMethod.Get,
                new Uri(baseUri, $"biux-xiwe.json?{query}")).Result;
        }

        public static List<Committee> GetCommittees(short? year, string party, string filerType = null, string jurisdictionType = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!string.IsNullOrWhiteSpace(party))
                query += $"&party_code={party}";
            if (!string.IsNullOrWhiteSpace(filerType))
                query += $"&filer_type={filerType}";
            if (!string.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            return WebClient.SendAsync<List<Committee>>(HttpMethod.Get,
                new Uri(baseUri, $"d27u-zvri.json?{query}")).Result;
        }

        public static List<Committee> GetCommittees(string first, string last, string jurisdictionType = null)
        {
            var query = $"$limit={limit}";
            if (!string.IsNullOrWhiteSpace(first))
                query += $"&first_name={first}";
            if (!string.IsNullOrWhiteSpace(last))
                query += $"&last_name={last}";
            if (!string.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            return WebClient.SendAsync<List<Committee>>(HttpMethod.Get,
                new Uri(baseUri, $"d27u-zvri.json?{query}")).Result;
        }

        public static List<Expenditure> GetExpenditures(short? year = null, string filer_id = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!string.IsNullOrWhiteSpace(filer_id))
                query += $"&filer_id={filer_id}";
            return WebClient.SendAsync<List<Expenditure>>(HttpMethod.Get,
                new Uri(baseUri, $"tijg-9zyp.json?{query}")).Result;
        }

        public static List<Expenditure> GetExpensesByType(short? year = null, string entityType = null, string expenseCode = null, string jurisdictionType = null, string legislativeDistrict = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!string.IsNullOrWhiteSpace(entityType))
                query += $"&type={entityType}";
            if (!string.IsNullOrWhiteSpace(expenseCode))
                query += $"&code={expenseCode}";
            if (!string.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            if (!string.IsNullOrWhiteSpace(legislativeDistrict))
                query += $"&legislative_district={legislativeDistrict}";

            return WebClient.SendAsync<List<Expenditure>>(HttpMethod.Get,
                new Uri(baseUri, $"tijg-9zyp.json?{query}")).Result;
        }

        public static List<Contribution> GetContributions(short? year = null, string filer_id = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!string.IsNullOrWhiteSpace(filer_id))
                query += $"&filer_id={filer_id}";
            return WebClient.SendAsync<List<Contribution>>(HttpMethod.Get,
                new Uri(baseUri, $"kv7h-kjye.json?{query}")).Result;
        }

        public static List<Contribution> GetContributionsByType(short? year = null, string entityType = null, string jurisdictionType = null, string legislativeDistrict = null, string contributionType = null)
        {
            var query = $"$limit={limit}";
            if (year != null)
                query += $"&election_year={year}";
            if (!string.IsNullOrWhiteSpace(entityType))
                query += $"&code={entityType}";
            if (!string.IsNullOrWhiteSpace(jurisdictionType))
                query += $"&jurisdiction_type={jurisdictionType}";
            if (!string.IsNullOrWhiteSpace(legislativeDistrict))
                query += $"&legislative_district={legislativeDistrict}";
            if (!string.IsNullOrWhiteSpace(contributionType))
                query += $"&cash_or_in_kind={contributionType}";
            return WebClient.SendAsync<List<Contribution>>(HttpMethod.Get,
                new Uri(baseUri, $"kv7h-kjye.json?{query}")).Result;
        }

        private static bool Status(string filerId, IEnumerable<Committee> committees, string value)
        {
            var campaign = committees.FirstOrDefault(i => i.filer_id == filerId);
            return campaign?.general_election_status?.StartsWith(value) ?? false;
        }
    }
}
