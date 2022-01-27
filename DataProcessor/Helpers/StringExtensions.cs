using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace WhipStat.Helpers
{
    /// <summary>
    /// String utility library
    /// </summary>
    public static class StringExtensions
    {
        static readonly TextInfo ti = CultureInfo.CurrentCulture.TextInfo;  // Improve performance by calling TextInfo string methods
        static readonly HashSet<string> minor_words = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "as", "at", "but", "by", "for", "if", "in", "nor",
            "of", "off", "on", "or", "per", "so", "the", "to", "up", "via", "yet"
        };
        static readonly HashSet<string> abbreviations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "N", "S", "E", "W", "NE", "NW", "SE", "SW", "PO", "AD", "BC",
            "AK", "AZ", "AR", "CA", "DE", "DC", "FL", "GA", "GU", "ID",
            "IL", "IA", "KS", "KY", "MD", "MA", "MI", "MN", "MO", "MT",
            "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK",
            "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA",
            "WA", "WV", "WI", "WY", "USA", "NATO", "FBI", "CIA", "DOD",
            "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XL", "XXL",
            "LLC", "LP", "PLC", "PLLC", "MD", "DDS", "AAA", "ABC"
            // "AL", "CO", "CT", "HI", "IN", "LA", "ME", "MS", "OH", "OR" - State abbreviations w/ conflicts
        };

        public static double KeywordMatch(this string strA, string strB)
            => strA.KeywordMatch(strB.Split());

        public static double KeywordMatch(this string str, string[] keywords)
            => str.Split().KeywordMatch(keywords);

        public static double KeywordMatch(this string[] words, string[] keywords)
        {
            var len = (words.Length + keywords.Length) / 2;
            return len > 0 ? (double)words.Count(i => keywords.Contains(i)) / len : 0d;
        }

        public static bool WildcardMatch(this string str, string pattern)
            => Regex.IsMatch(str, pattern.WildCardToRegular());

        private static string WildCardToRegular(this string str)
            => "^" + Regex.Escape(str).Replace("\\?", ".").Replace("\\*", ".*") + "$";

        private static string WildCardToSQL(this string str)
            => str?.Replace('*', '%').Replace('?', '_').Replace('!', '^');

        public static string ToSQL(this string str)
            => str?.Replace("'", "''");

        public static string Index(this string[] array, int index)
            => index >= 0 && index < array.Length ? array[index] : array[0];

        public static string Join(string separator, params string[] args)
            => string.Join(separator, args.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray());

        public static string Trim(this string str, int maxLength)
            => string.IsNullOrEmpty(str) ? null : str.Trim().Truncate(maxLength);

        public static string Truncate(this string str, int maxLength)
            => string.IsNullOrEmpty(str) ? null : str.Length <= maxLength ? str : str[..maxLength];

        public static string ToSentenceCase(this string str)
            => string.IsNullOrEmpty(str) ? str : ti.ToUpper(str[0]) + ti.ToLower(str[1..]);

        public static string ToAlphaNumeric(this string str)
            => new string(str.Where(char.IsLetterOrDigit).ToArray());

        public static string ToNumeric(this string str)
            => new string(str.Where(char.IsDigit).ToArray());

        public static string ToTitleCase(this string str, char separator = ' ')
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var words = str.Split(separator, StringSplitOptions.TrimEntries);
            for (int i = 0; i < words.Length; ++i)
            {
                if (i > 0 && minor_words.Contains(words[i]))
                    words[i] = words[i].ToLowerInvariant();
                else if (abbreviations.Contains(words[i]))
                    words[i] = words[i].ToUpperInvariant();
                else if (words[i].Contains('-'))
                    words[i] = words[i].ToTitleCase('-');
                else
                    words[i] = words[i].ToSentenceCase();
            }

            return string.Join(separator, words);
        }

        public static string ToZipCode(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                // Strip out non-digits
                str = str.ToNumeric();
                if (ulong.TryParse(str, out var num))
                {
                    if (str.Length <= 5)
                        return num.ToString("D5");
                    if (str.Length == 9)
                        return num.ToString("#####-####");
                }
            }

            return str;
        }

        public static string ToPhoneNumber(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                // Strip out non-digits
                str = str.ToNumeric();
                if (ulong.TryParse(str, out var num))
                {
                    if (str.Length == 7)
                        return num.ToString("###-####");
                    if (str.Length == 10)
                        return num.ToString("(###) ###-####");
                    if (str.Length > 10)
                        return num.ToString("(###) ###-#### x" + new String('#', (str.Length - 10)));
                }
            }

            return str;
        }

        public static bool IsUpper(this string str)
            => !str.Any(i => char.IsLower(i));

        public static bool IsLower(this string str)
            => !str.Any(i => char.IsUpper(i));

        public static bool IsValidEmail(this string str)
            => Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public static string ToIndexedName(this string str, int i = -1)
        {
            var words = str.Trim().Split();
            if (i < 0)
                i += words.Length;
            return i < words.Length ? words[i].ToTitleCase() : null;
        }
    }
}