using System;
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

        public static double KeywordMatch(this string strA, string strB)
            => strA.KeywordMatch(strB.Split());

        public static double KeywordMatch(this string str, string[] keywords)
            => str.Split().KeywordMatch(keywords);

        public static double KeywordMatch(this string[] words, string[] keywords)
        {
            var len = Math.Min(words.Length, keywords.Length);
            return len > 0 ? (double)words.Count(i => keywords.Contains(i)) / len : 0d;
        }

        public static bool WildcardMatch(this string str, string pattern)
            => Regex.IsMatch(str, WildCardToRegular(pattern));

        private static string WildCardToRegular(string str)
            => "^" + Regex.Escape(str).Replace("\\?", ".").Replace("\\*", ".*") + "$";

        private static string WildCardToSQL(string str)
            => str.Replace('*', '%').Replace('?', '_').Replace('!', '^');

        public static string Index(this string[] array, int index)
            => index >= 0 && index < array.Length ? array[index] : array[0];

        public static string Join(string separator, params string[] args)
            => String.Join(separator, args.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray());

        public static string Trim(this string str, int maxLength)
            => string.IsNullOrEmpty(str) ? null : str.Trim().Truncate(maxLength);

        public static string Truncate(this string str, int maxLength)
            => string.IsNullOrEmpty(str) ? null : str.Length <= maxLength ? str : str[..maxLength];

        public static string ToTitleCase(this string str)
            => string.IsNullOrWhiteSpace(str) ? str : ti.ToTitleCase(ti.ToLower(str));

        public static string ToSentenceCase(this string str)
            => string.IsNullOrWhiteSpace(str) ? str : ti.ToUpper(str[0]) + ti.ToLower(str[1..]);

        public static string ToAlphaNumeric(this string str)
            => new string(str.Where(char.IsLetterOrDigit).ToArray());

        public static string ToNumeric(this string str)
            => new string(str.Where(char.IsDigit).ToArray());

        public static bool IsValidEmail(this string str)
            => Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

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

        public static string ToIndexedName(this string str, int i = -1)
        {
            var words = str.Trim().Split();
            if (i < 0)
                i += words.Length;
            return i < words.Length ? words[i].ToTitleCase() : null;
        }
    }
}