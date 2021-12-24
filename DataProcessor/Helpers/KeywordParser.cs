using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WhipStat.Helpers
{
    public class KeywordParser
    {
        readonly KeyValuePair<string, string>[] dictionary;

        public KeywordParser(string path = null)
        {
            // Load optional dictionary with abbreviations, aliases and nicknames
            dictionary = string.IsNullOrEmpty(path) ? null : File.ReadAllLines(path)
                .Select(i => i.Split(','))
                .Select(j => new KeyValuePair<string, string>($" {j[0]} ", $" {j[1]}"))
                .ToArray();
        }

        public string[] Parse(string str)
        {
            // Cast to lowercase and decode any HTML entities (e.g. &AMP;)
            str = WebUtility.HtmlDecode(str.ToLowerInvariant());

            // Filter out uninteresting characters and pad with spaces
            str = " " + Filter(str) + " ";

            if (dictionary != null)
            {
                // Replace abbreviations, aliases and nicknames with canonical names
                foreach (var entry in dictionary)
                    str = str.Replace(entry.Key, entry.Value);
            }

            // Return result split into words
            return str.Trim().Split().Distinct().ToArray();
        }

        public static string Filter(string str)
        {
            var sb = new StringBuilder(str.Length);
            foreach (char c in str)
                sb.Append(char.IsLetterOrDigit(c) || c == '&' ? c : ' ');
            return sb.ToString();
        }
    }
}