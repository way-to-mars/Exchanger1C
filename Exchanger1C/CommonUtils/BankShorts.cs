using System;
using System.Collections.Generic;

namespace Exchanger1C
{
    internal class BankShorts
    {
        private static Dictionary<String, String> TopBanks = new Dictionary<String, String>() {
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

        public static string findShortName(String name) {
            if (name == null || name.Length == 0) return "";

            string upperName = name.ToUpper();
            foreach(KeyValuePair<string, string> bank in TopBanks)
                if (upperName.Contains(bank.Key)) return bank.Value;

            return "";
        }
    }
}
