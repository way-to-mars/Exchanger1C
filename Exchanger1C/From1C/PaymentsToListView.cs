using System.Collections.Generic;
using System.Text;

namespace Exchanger
{
    internal class PaymentsToListView
    {
        private static char[] trimChars = { ' ', '\n' };
        public string Number
        {
            get { return GetOrDefault("Номер", "n/a"); }
            set { _parameters["Номер"] = value; }
        }
        public string Date
        {
            get { return GetOrDefault("Дата", "n/a"); }
            set { _parameters["Дата"] = value; }
        }
        public string Sum
        {
            get
            {
                string value = GetOrDefault("Сумма", "");
                if (value.Length == 0)
                    return "n/a";
                return RubleKop.FromString(value).ToString();
            }
            set { _parameters["Сумма"] = value; }
        }
        public string Name
        {
            get { return GetOrDefault("Плательщик1", "n/a"); }
            set { _parameters["Плательщик1"] = value; }
        }
        public string Description
        {
            get { return GetOrDefault("НазначениеПлатежа", "n/a"); }
            set { _parameters["НазначениеПлатежа"] = value; }
        }

        public string NativeCode
        {
            get
            {
                return _stringBuilder.ToString();
            }
            set { }
        }

        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private StringBuilder _stringBuilder;

        public PaymentsToListView() { }

        private string GetOrDefault(string key, string defaultValue)
        {
            string value;
            if (key == null || !this._parameters.TryGetValue(key, out value))
                return defaultValue;
            return value ?? "n/a";
        }

        public void AddString(string line1c)
        {
            string[] line_split = line1c.Trim(trimChars).Split('=');
            string key = line_split[0];
            string value;
            if (line_split.Length > 1)
                value = line_split[1];
            else
                value = null;
            _parameters[key] = value;

            if (_stringBuilder == null) _stringBuilder = new StringBuilder(line1c.Trim(trimChars));
            else
            {
                _stringBuilder.Append("\n");
                _stringBuilder.Append(line1c.Trim(trimChars));
            }

        }
    }
}
