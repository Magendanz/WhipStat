using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml;

using DataAccess;

using WhipStat.Helpers;
using WhipStat.Models;

namespace WhipStat.DataAccess
{
  public static class TrfAccess
  {
    readonly static Uri baseUri = new Uri("https://trf-mobile.azurewebsites.net");

    public static Dictionary<int, string> GetPrecincts(string district)
    {
      return WebClient.SendAsync<Dictionary<int, string>>(HttpMethod.Get,
          new Uri(baseUri, $"precincts?type=Legislative&name={district}")).Result;
    }

        public static List<Voter> GetVoters(string last, string first, string city, bool inactive = false)
        {
            var query = $"last={last}";
            if (!string.IsNullOrWhiteSpace(first))
                query += $"&first={first}";
            if (!string.IsNullOrWhiteSpace(city))
                query += $"&city={city}";
            if (inactive)
                query += "&inactive=true";

            return WebClient.SendAsync<List<Voter>>(HttpMethod.Get, new Uri(baseUri, $"voter?{query}")).Result;
        }

        public static string GetVoters(int district, int precinct, bool inactive = false)
        {
            var query = $"district={district}";
            if (precinct < int.MaxValue)
                query += $"&precinct={precinct}";
            if (inactive)
                query += "&inactive=true";
            var voters = WebClient.SendAsync<List<Voter>>(HttpMethod.Get, new Uri(baseUri, $"voters?{query}")).Result;

            DataTable dt = DataTable.New.FromEnumerable(voters);
            return dt.SaveToString();
        }
    }
}