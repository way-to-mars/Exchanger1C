using System;
using System.Collections.Generic;
using System.Linq;

namespace Exchanger1C
{
    internal class BankShorts
    {
        private static List<string> WordsToExclude = new List<string>() {
            "ПАО", "(ПАО)",
            "ООО", "(ООО)",
            "ОАО", "(ОАО)",
            "АО", "(АО)",
            "ЗАО", "(ЗАО)",
            "Банк",
            "АКБ", "КБ", "НКБ", "УКБ", "ККБ", "СКБ", "АКИБ", "КИБ", "ИКБР",
            "РНКО", "НКО",
        };

        private static char[] delimiterChars = { ' ', '-' };

        public static string FindShortName(string fullname)
        {
            if (fullname == null || fullname.Length == 0) return "";

            if (HasExactlyTwoQuotes(fullname))
            {
                string properName = SubstringBetweenQuotes(fullname);
                return TryInitials(FilterSymbols(properName));
            }

            return TryInitials(string.Join(" ", FilterSymbols(fullname).Split(' ').Where(IsAllowedWord)));
        }

        private static bool HasExactlyTwoQuotes(string input) => input.Count(ch => ch == '"') == 2;

        private static string SubstringBetweenQuotes(string input)
        {
            int firstIndex = input.IndexOf('"');
            int lastIndex = input.LastIndexOf('"');
            int newLength = lastIndex - firstIndex - 1;

            if (firstIndex == -1 || lastIndex == -1 || newLength < 1 || lastIndex >= input.Length) return input;

            return input.Substring(firstIndex + 1, newLength);
        }
               
        private static string TryInitials(string input)  // transform to abbreviation if it has 3 or more words
        {
            string[] words = input.Split(delimiterChars);

            if (words.Length < 3) return input;

            return string.Join("", words.Select(word => char.ToUpper(word[0])));
        }

        private static string FilterSymbols(string input) => input.Replace("(", "").Replace(")", "").Replace("\"", "");

        private static bool IsAllowedWord(string input)
        {
            string upperInput = input.ToUpper();

            foreach (var word in WordsToExclude)
            {
                if (word.ToUpper() == upperInput) return false;
            }

            return true;
        }
    }
}