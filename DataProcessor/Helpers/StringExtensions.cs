using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WhipStat.Helpers
{
    public static class StringExtensions
    {
        public static bool WildcardMatch(this string str, string pattern)
            => Regex.IsMatch(str, WildCardToRegular(pattern));

        private static string WildCardToRegular(string str)
            => "^" + Regex.Escape(str).Replace("\\?", ".").Replace("\\*", ".*") + "$";

        public static string Index(this string[] array, int index)
           => index >= 0 && index < array.Count() ? array[index] : array[0];

        public static string Join(string separator, params string[] args)
            => String.Join(separator, args.Where(i => !String.IsNullOrWhiteSpace(i)).ToArray());

        public static string Truncate(this string str, int maxLength)
        {
            if (String.IsNullOrWhiteSpace(str))
                return null;

            var s = str.Trim();
            return s.Length <= maxLength ? s : s.Substring(0, maxLength);
        }

        public static bool Contains(this string str, string value, StringComparison comparisonType)
            => str.IndexOf(value, comparisonType) >= 0;

        public static string ToTitleCase(this string str)
            => Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());

        public static string ToSentenceCase(this string str)
            => String.IsNullOrWhiteSpace(str) ? str : str.First().ToString().ToUpper() + str.Substring(1).ToLower();

        public static string ToAlphaNumeric(this string str)
            => Regex.Replace(str, "[^a-zA-Z0-9 ]", String.Empty);

        public static string ToNumeric(this string str)
            => Regex.Replace(str, "[^0-9]", String.Empty);

        public static string ToZipCode(this string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                // Strip out non-digits
                str = str.ToNumeric();
                if (UInt64.TryParse(str, out UInt64 num))
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
            if (!String.IsNullOrEmpty(str))
            {
                // Strip out non-digits
                str = str.ToNumeric();
                if (UInt64.TryParse(str, out UInt64 num))
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

        public static bool IsValidEmail(this string str)
            => Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
