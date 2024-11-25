/// <summary>
/// Root class and functions for GrayCat library
/// (c) 2006-2025 Evgeniy Dolonin https://github.com/graycatsoft
/// </summary>

using System;

namespace GrayCat
{
    public static class Tools
    {
        public static string BytesToHex(byte[] value, bool isUpper = true)
        {
            string result = string.Empty;
            for (int i = 0; i < (value?.Length ?? 0); i++)
            {
                result += string.Format(isUpper ? "{0:X2}" : "{0:x2}", value?[i]);
            }
            return result;
        }

        public static byte[] HexToBytes(string value)
        {
            string s = value?.Trim()?.ToLower() ?? string.Empty;
            if (s.Length % 2 > 0)
            {
                s = '0' + s;
            }
            byte[] result = new byte[s.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(s.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return result;
        }

        public static string EmptyAsDefault(string value, string defValue)
        {
            return (string.IsNullOrWhiteSpace(value)) ? defValue : value;
        }

        public static byte[] Reverse(byte[] value)
        {
            int dl = value.Length / 2;
            int e = value.Length - 1;
            for (int i = 0; i < dl; i++)
            {
                (value[e], value[i]) = (value[i], value[e]);
                e--;
            }
            return value;
        }

        public static string ExtractImageType(ref byte[] data)
        {
            if (data?.Length > 8)
            {
                if ((data[0] == 0x089) && (data[1] == 0x050) && (data[2] == 0x04E) && (data[3] == 0x047) && (data[4] == 0x00D) && (data[5] == 0x00A) && (data[6] == 0x01A) && (data[7] == 0x00A))
                {
                    return "png";
                }
                if ((data[0] == 0x047) && (data[1] == 0x049) && (data[2] == 0x046) && (data[3] == 0x038) && ((data[4] == 0x037) || (data[4] == 0x039)) && (data[5] == 0x061))
                {
                    return "gif";
                }
                if ((data[0] == 0x0FF) && (data[1] == 0x0D8) && (data[2] == 0x0FF))
                {
                    return "jpg";
                }
                if ((data[0] == 0x042) && (data[1] == 0x04D))
                {
                    return "bmp";
                }
            }
            return string.Empty;
        }

    }

    public delegate int GettingLanguageIndex(char id);

    public class LanguageString
    {
        public LanguageString(GettingLanguageIndex indexer)
        {
            Indexer = indexer ?? throw new Exception("Indexer cannot be null");
            int size = Indexer('#');
            if (size == 0)
            {
                throw new Exception("The indexer returned an empty list");
            }
            Values = new string[size];
        }

        public string this[char index] { get => GetItem(index); set { SetItem(index, value); } }

        public string Default => Values[0];


        public void Set(string value)
        {
            Values[0] = value;
        }

        public void Set(string value0, string value1)
        {
            Values[0] = value0;
            if (Values.Length < 2)
            {
                CheckSize();
            }
            if (Values.Length > 1)
            {
                Values[1] = value1;
            }
        }

        public void Set(string value0, string value1, string value2)
        {
            Values[0] = value0;
            if (Values.Length < 3)
            {
                CheckSize();
            }
            if (Values.Length > 1)
            {
                Values[1] = value1;
            }
            if (Values.Length > 2)
            {
                Values[2] = value2;
            }
        }

        private string GetItem(char index)
        {
            int id = Indexer(index);
            if (id >= Values.Length)
            {
                CheckSize();
            }
            return (id < 0) ? Values[0] : Values[id];
        }

        private void SetItem(char index, string value)
        {
            int id = Indexer(index);
            if (id >= Values.Length)
            {
                CheckSize();
            }
            if (id >= 0)
            {
                Values[id] = value;
            }
        }

        private void CheckSize()
        {
            int NewSize = Indexer('#');
            if (NewSize > Values.Length)
            {
                string[] old = Values;
                Values = new string[NewSize];
                for (int i = 0; i < old.Length; i++)
                {
                    Values[i] = old[i];
                }
            }
        }

        private readonly GettingLanguageIndex Indexer;
        private string[] Values;
    }

    public class StorageNode
    {
        #nullable enable

        public enum NodeValueType { Empty, Text, Int, Long, Bool, Float, Curr, Time }

        protected string _Name = String.Empty;

        public NodeValueType ValueType { get; protected set; } = NodeValueType.Empty;

        protected object? _Value = null;

        protected virtual string SafeNodeName(string newName)
        {
            return newName;
        }

        public string Name { get { return _Name; } set { _Name = SafeNodeName(value); } }

        public int DefaultEncoding { get; set; } = 65001;

        public static string ConvertToText(int? value)
        {
            return (value ?? default).ToString();
        }

        public static string ConvertToText(long? value)
        {
            return (value ?? default).ToString();
        }

        public static string ConvertToText(double? value)
        {
            return (value ?? default).ToString("G").Replace(',', '.');
        }

        public static string ConvertToText(decimal? value)
        {
            return (value ?? default).ToString("F4").Replace(',', '.');
        }

        public static string ConvertToText(bool? value)
        {
            return (value ?? default) ? "true" : "false";
        }

        public static string ConvertToText(DateTime? value)
        {
            return (value ?? default).ToString("s");
        }

        public static int ConvertTextInt(string? value)
        {
            return int.TryParse(value, out int i) ? i : default;
        }

        public static long ConvertTextLong(string? value)
        {
            return long.TryParse(value, out long l) ? l : default;
        }

        public static bool ConvertTextBool(string? value)
        {
            return (value != null) && ((value.Trim().ToLower() == "true") || (int.Parse(AsNumeric(value)) > 0));
        }
                
        public static double ConvertTextFloat(string? value)
        {
            return (string.IsNullOrEmpty(value)) ? default : double.Parse(ReplaceDecimal(value));
        }

        public static decimal ConvertTextCurr(string? value)
        {
            return (string.IsNullOrEmpty(value)) ? default : decimal.Parse(ReplaceDecimal(value));
        }

        public static DateTime ConvertTextTime(string? value)
        {
            return (string.IsNullOrEmpty(value)) ? default : DateTime.ParseExact(value, "s", null);
        }

        public string? Text
        {
            get
            {
                return ValueType switch
                {
                    NodeValueType.Text => ((string?)_Value) ?? String.Empty,
                    NodeValueType.Int => ConvertToText((int?)_Value),
                    NodeValueType.Long => ConvertToText((long?)_Value),
                    NodeValueType.Bool => ConvertToText((bool?)_Value),
                    NodeValueType.Float => ConvertToText((double?)_Value),
                    NodeValueType.Curr => ConvertToText((decimal?)_Value),
                    NodeValueType.Time => ConvertToText((DateTime?)_Value),
                    _ => null,
                };
            }
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Text;
                _Value = value;
            }
        }

        public int? Int
        {
            get
            {
                return ValueType switch
                {
                    NodeValueType.Text => ConvertTextInt((string?)_Value),
                    NodeValueType.Int => ((int?)_Value) ?? default,
                    NodeValueType.Long => Convert.ToInt32(((long?)_Value) ?? default),
                    NodeValueType.Bool => (((bool?)_Value) ?? default) ? 1 : 0,
                    NodeValueType.Float => Convert.ToInt32(Math.Round(((double?)_Value) ?? default)),
                    NodeValueType.Curr => Convert.ToInt32(Math.Round(((decimal?)_Value) ?? default)),
                    NodeValueType.Time => Convert.ToInt32((((DateTime?)_Value) ?? default).ToOADate()),
                    _ => null,
                };
            }
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Int;
                _Value = value;
            }
        }

        public long? Long
        {
            get
            {
                return ValueType switch
                {
                    NodeValueType.Text => ConvertTextLong((string?)_Value),
                    NodeValueType.Int => ((int?)_Value) ?? default,
                    NodeValueType.Long => ((long?)_Value) ?? default,
                    NodeValueType.Bool => (((bool?)_Value) ?? default) ? 1 : 0,
                    NodeValueType.Float => Convert.ToInt64(Math.Round(((double?)_Value) ?? default)),
                    NodeValueType.Curr => Convert.ToInt64(Math.Round(((decimal?)_Value) ?? default)),
                    NodeValueType.Time => Convert.ToInt64((((DateTime?)_Value) ?? default).ToOADate()),
                    _ => null,
                };
            }
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Long;
                _Value = value;
            }
        }

        public bool? Bool
        {
            get => ValueType switch
            {
                NodeValueType.Text => ConvertTextBool((string?)_Value),
                NodeValueType.Int => (((int?)_Value) ?? default) > 0,
                NodeValueType.Long => (((long?)_Value) ?? default) > 0,
                NodeValueType.Bool => ((bool?)_Value) ?? default,
                NodeValueType.Float => (((double?)_Value) ?? default) > 0,
                NodeValueType.Curr => (((decimal?)_Value) ?? default) > 0,
                NodeValueType.Time => (((DateTime?)_Value) ?? default).ToOADate() > 0,
                _ => null,
            };
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Bool;
                _Value = value;
            }
        }

        public double? Float
        {
            get
            {
                return ValueType switch
                {
                    NodeValueType.Text => ConvertTextFloat((string?)_Value),
                    NodeValueType.Int => ((int?)_Value) ?? default,
                    NodeValueType.Long => ((long?)_Value) ?? default,
                    NodeValueType.Bool => (((bool?)_Value) ?? default) ? 1 : 0,
                    NodeValueType.Float => ((double?)_Value) ?? default,
                    NodeValueType.Curr => decimal.ToDouble(((decimal?)_Value) ?? default),
                    NodeValueType.Time => (((DateTime?)_Value) ?? default).ToOADate(),
                    _ => null,
                };
            }
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Float;
                _Value = value;
            }
        }

        public decimal? Curr
        {
            get
            {
                return ValueType switch
                {
                    NodeValueType.Text => ConvertTextCurr((string?)_Value),
                    NodeValueType.Int => ((int?)_Value) ?? default,
                    NodeValueType.Long => ((long?)_Value) ?? default,
                    NodeValueType.Bool => (((bool?)_Value) ?? default) ? 1 : 0,
                    NodeValueType.Float => ((decimal?)_Value) ?? default,
                    NodeValueType.Curr => ((decimal?)_Value) ?? default,
                    NodeValueType.Time => (decimal)((((DateTime?)_Value) ?? default).ToOADate()),
                    _ => null,
                };
            }
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Curr;
                _Value = value;
            }
        }

        public DateTime? Time
        {
            get
            {
                return ValueType switch
                {
                    NodeValueType.Text => ConvertTextTime((string?)_Value),
                    NodeValueType.Int => DateTime.FromOADate(((int?)_Value) ?? default),
                    NodeValueType.Long => DateTime.FromOADate(((long?)_Value) ?? default),
                    NodeValueType.Bool => DateTime.FromOADate((((bool?)_Value) ?? default) ? 1 : 0),
                    NodeValueType.Float => DateTime.FromOADate(((double?)_Value) ?? default),
                    NodeValueType.Curr => DateTime.FromOADate((double)(((decimal?)_Value) ?? default)),
                    NodeValueType.Time => ((DateTime?)_Value) ?? default,
                    _ => null,
                };
            }
            set
            {
                ValueType = (value == null) ? NodeValueType.Empty : NodeValueType.Time;
                _Value = value;
            }
        }

        protected virtual byte[]? GetBOM(int encoding = 50001)
        {
            if (((encoding == 50001) ? DefaultEncoding : encoding) == 65001)
            {
                byte[] b = new byte[3];
                b[0] = 239;
                b[1] = 187;
                b[2] = 191;
                return b;
            }
            else
            {
                return null;
            }
        }

        private static string AsNumeric(string value)
        {
            foreach (char c in value.Trim())
            {
                if (!Char.IsDigit(c))
                {
                    return "0";
                }
            }
            return value;
        }

        public static string ReplaceDecimal(string value)
        {
            string sp = System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
            return (sp == ".") ? value : value.Replace(".", sp);
        }

    }

}
