using System.Collections.Generic;
using System.Linq;
using System;

namespace Exchanger
{
    internal static class DictionaryExtension
    {
        public static void AddOnce<K, V>(this Dictionary<K, V> map, K key, V value)
        {
            if (!map.ContainsKey(key)) map.Add(key, value);
        }

        public static void AddOrUpdate<K, V>(this Dictionary<K, V> map, K key, V value)
        {
            if (map.ContainsKey(key)) map[key] = value;
            else map.Add(key, value);
        }

        public static string GetOrEmpty(this Dictionary<string, string> map, string key)
        {
            return map.TryGetValue(key, out string result) ? result : "";
        }

        public static string GetOrEmpty(this Dictionary<string, string> map, string key1, string key2)
        {
            return map.TryGetValue(key1, out string result) ? result : map.TryGetValue(key2, out result) ? result : "";
        }
    }

    internal static class StringParsingExtensions
    {
        public static DateTime? ParseDate(this string dateTxt)
        {
            string[] split = dateTxt.Split('.');
            if (split == null || split.Length != 3) return null;

            if (!int.TryParse(split[2], out int year)) return null;
            if (!int.TryParse(split[1], out int month)) return null;
            if (!int.TryParse(split[0], out int day)) return null;

            return new DateTime(year, month, day); // год - месяц - день
        }

        public static int? ParseInt(this string numberTxt)
        {
            return int.TryParse(numberTxt, out int value) ? (int?)value : null;
        }

        public static double? ParseAmount(this string amountTxt)
        {
            long wholePart = 0;
            double multiplier = 1.0;
            string digitsTxt;
            string filteredTxt;

            try
            {
                digitsTxt = String.Concat(amountTxt.Where(char.IsDigit));
                wholePart = long.Parse(digitsTxt);
            }
            catch (Exception) { return null; }

            filteredTxt = String.Concat(amountTxt.Where(c => ".0123456789".Contains(c)));
            int dotPos = amountTxt.IndexOf('.');

            if (dotPos != -1)
            {
                multiplier = 1.0 / Math.Pow(10, digitsTxt.Length - dotPos);
            }

            return multiplier * wholePart;
        }

        public static string ParseKey(this string line)
        {
            int delim = line.IndexOf('=');
            if (delim > 0) return line.Substring(0, delim);
            return line;
        }

        public static string ParseValue(this string line)
        {
            int delim = line.IndexOf('=');
            if (delim > 0 && delim < line.Length - 1) return line.Substring(delim + 1, line.Length - delim - 1);
            return "";
        }

        public static string ShortenFirmName(this string fullname)
        {
            // 1. Searching for a quote-bounded substring:  abc"name"efg --> name, ООО "ПК "Сатурн" --> ПК Сатурн
            int leftQuote = fullname.IndexOf("\"");
            int RightQuote = fullname.LastIndexOf("\"");
            if (RightQuote - leftQuote > 1)
            {
                return fullname
                    .Substring(leftQuote + 1, RightQuote - leftQuote - 1)
                    .Trim()
                    .Replace("\"", string.Empty);
            }

            // 2. Exclude one of the prefixes given and return remaining substring
            string[] toExclude = { "ООО ", "АО ", "ПАО ", "ЗАО ", "ИП ",
                "ОБЩЕСТВО С ОГРАНИЧЕННОЙ ОТВЕТСТВЕННОСТЬЮ ",
                "АКЦИОНЕРНОЕ ОБЩЕСТВО ",
                "ПУБЛИЧНОЕ АКЦИОНЕРНОЕ ОБЩЕСТВО ",
                "ЗАКРЫТОЕ АКЦИОНЕРНОЕ ОБЩЕСТВО ",
                "ИНДИВИДУАЛЬНЫЙ ПРЕДПРИНИМАТЕЛЬ"};
            string upperName = fullname.ToUpper();
            foreach (string prefix in toExclude) {
                if (upperName.StartsWith(prefix)) {
                    int start = prefix.Length;
                    int len = fullname.Length - start;
                    return fullname
                        .Substring(start, len)
                        .Replace("\"", string.Empty)
                        .Trim();
                }
            }
            // 3. Unsuccesfull case
            return fullname
                .Replace("\"", string.Empty)
                .Trim();
        }

    }

    internal static class ToStringExtensions
    {
        public static string toFromDateString(this DateTime? datetime)
        {
            if (datetime == null) return string.Empty;
            var date = (DateTime)datetime;
            return $"от {date.ToString("dd.MM.yyyy")}";
        }

        public static string toNumberedString(this int? number)
        {
            if (number == null) return string.Empty;
            return $"№{(int)number}";
        }

        public static string toShortDocTypeString(this string docType)
        {
            if (docType.Length == 0) return string.Empty;
            switch (docType)
            {
                case "Платежное поручение": return "Пл.пор.";
                case "Банковский ордер": return "Банк.орд.";
                case "Платежное требование": return "Пл.треб.";
                case "Платежный ордер": return "Пл.орд.";
                default: return "Док.";
            }
        }
    }

}
