using System;
using System.Collections.Generic;
using System.Linq;

namespace Exchanger1C.CommonUtils
{
    internal class BankShorts
    {
        private static Dictionary<string, string> topBanks = new Dictionary<string, string>() {
            { "СБЕРБАНК", "Сбер" },
            { "РОССЕЛЬХОЗБАНК", "РСХБ" },
            { "ВОСТОЧНЫЙ", "Вост" },
            { "КРОНА-БАНК", "Крона" },
            { "АЗИАТСКО-ТИХООКЕАНСКИЙ БАНК", "АТБ" },
            { "ПСБ", "ПСБ"},
            { "ПРОМСВЯЗЬБАНК", "ПСБ"},
            { "МЕТАЛЛИНВЕСТБАНК", "МетИнв"},
            { "ЮНИКРЕДИТ", "ЮниКр"},
            { "СИТИБАНК", "Сити"},
            { "ГПБ", "ГПБ"},
            { "ГАЗПРОМБАНК", "ГПБ"},
            { "ОТКРЫТИЕ", "Откр"},
            { "РОСБАНК", "Росбанк"},
            { "ВТБ", "ВТБ"},
            { "ЛАНТА-БАНК", "Ланта"},
            { "УРАЛСИБ", "Урал"},
            { "СОВКОМБАНК", "СовКом"},
            { "РАЙФФАЙЗЕНБАНК", "Райф"},
            { "АКЦЕПТ", "Акцепт"},
            { "МОДУЛЬБАНК", "Модуль"},
            { "Точка", "Точка"},
            { "ЛОКО-БАНК", "Локо"},
            { "АВАНГАРД", "Авангард"},
            { "ТИНЬКОФФ", "Тинь"},
            { "АЛЬФА-БАНК", "Альфа"},
            { "ЭКСПОБАНК", "Экспо"},
        };

        private static readonly List<string> wordsToExclude = new List<string>() {
            "ПАО", "(ПАО)",
            "ООО", "(ООО)",
            "ОАО", "(ОАО)",
            "АО", "(АО)",
            "ЗАО", "(ЗАО)",
            "Банк",
            "АКБ", "КБ", "НКБ", "УКБ", "ККБ", "СКБ", "АКИБ", "КИБ", "ИКБР",
            "РНКО", "НКО",
        };

        private static readonly char[] delimiterChars = { ' ', '-' };

        public static string FindShortName(string fullname)
        {
            if (fullname == null || fullname.Length == 0) return "";

            string upperName = fullname.ToUpper();
            foreach (KeyValuePair<string, string> bank in topBanks)
                if (upperName.Contains(bank.Key)) return bank.Value;

            return CreateShortName(fullname);
        }

        private static string CreateShortName(string fullname)
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
               
        private static string TryInitials(string input)  // transform to abbreviation if input has 3 or more words
        {
            string[] words = input.Split(delimiterChars);

            if (words.Length < 3) return input;

            return string.Concat(words.Select(word => char.ToUpper(word[0])));
        }

        private static string FilterSymbols(string input) => input.Replace("(", "").Replace(")", "").Replace("\"", "");

        private static bool IsAllowedWord(string input)
        {
            string upperInput = input.ToUpper();

            foreach (var word in wordsToExclude)
            {
                if (word.ToUpper() == upperInput) return false;
            }

            return true;
        }
    }
}