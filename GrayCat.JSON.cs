using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GrayCat.JSON
{
    #nullable enable

    public class JSONNode : StorageNode, IEnumerable<JSONNode>
    {

        public JSONNode(int defaultEncoding = 65001)
        {
            DefaultEncoding = defaultEncoding;
        }

        private bool IsManualObject = false;

        private bool ChildrenIsNamed()
        {
            foreach (JSONNode j in _Items)
            {
                if (!string.IsNullOrEmpty(j.Name))
                {
                    return true;
                }
            }
            return false;
        }

        private readonly List<JSONNode> _Items = new();

        private JSONNode? GetItem(int index)
        {
            JSONNode? r = null;
            if ((index >= 0) && (index < _Items.Count))
            {
                r = _Items[index];
            }
            else
            {
                for (int i = _Items.Count; i <= index; i++)
                {
                    r = Add();
                }
            }
            return r;
        }

        private JSONNode GetItem(string index)
        {
            JSONNode? r = null;
            string s = index.Trim();
            int n = s.IndexOf(' ');
            string a = (n >= 0) ? s[..n] : s;
            s = (n >= 0) ? s[(n + 1)..] : string.Empty;
            for (int i = 0; i < _Items.Count; i++)
            {
                if (_Items[i].Name == a)
                {
                    r = _Items[i];
                    break;
                }
            }
            r ??= Add(a);
            if (!string.IsNullOrEmpty(s))
            {
                r = r[s];
            }
            return r;
        }

        protected override string SafeNodeName(string newName)
        {
            return newName.Trim().Replace(' ', '_');
        }

        protected static string YamlText(string value)
        {
            return (value.Contains('\n')) ? '"' + value + '"' : (System.Text.RegularExpressions.Regex.IsMatch(value, "(^[-]?[0-9]*[.]?[0-9]+$)|([#&%:*]+)")) ? "'" + value + "'" : value;
        }

        private string InternalSave(int level, bool IsNamed = false)
        {
            StringBuilder r = new();
            string indent = string.Empty;
            string retstr = (level >= 0) ? "\r\n" : string.Empty;
            for (int i = 0; i < level; i++)
            {
                indent += "  ";
            }
            if (string.IsNullOrEmpty(indent))
            {
                r.Append(indent);
            }
            if (!string.IsNullOrEmpty(_Name))
            {
                r.Append('"' + _Name + "\":" + ((level >= 0) ? " " : string.Empty));
            }
            else
            if (IsNamed)
            {
                r.Append("\"Item\":" + ((level >= 0) ? " " : string.Empty));
            }
            if (Count > 0)
            {
                bool cn = IsObject;
                r.Append((cn) ? '{' : '[');
                for (int i = 0; i < Count; i++)
                {
                    if (i != 0)
                    {
                        r.Append(',');
                    }
                    r.Append(retstr + _Items[i].InternalSave((level >= 0) ? level + 1 : level, cn));
                }
                r.Append(retstr + indent + ((cn) ? '}' : ']'));
            }
            else
            {
                r.Append(ValueType switch
                {
                    NodeValueType.Text or NodeValueType.Time => '"' + JSONShielding(Text ?? string.Empty) + '"',
                    NodeValueType.Empty => "null",
                    _ => Text,
                });
            }
            return r.ToString();
        }

        private static string JSONShielding(string value)
        {
            StringBuilder? r = null;
            int sp = 0;
            string rp;
            for (int i = 0; i < value.Length; i++)
            {
                rp = value[i] switch
                {
                    '"' => "\\\"",
                    '\r' => "\\r",
                    '\n' => "\\n",
                    '\t' => "\\t",
                    _ => string.Empty
                };
                if (!string.IsNullOrEmpty(rp))
                {
                    r ??= new();
                    if (i > sp)
                    {
                        r.Append(value[sp..i]);
                    }
                    r.Append(rp);
                    sp = i + 1;
                }
            }
            if ((r != null) && (sp < value.Length))
            {
                r.Append(value[sp..]);
            }
            return (r == null) ? value : r.ToString();
        }

        private string InternalYamlSave(int level = 0, bool parentNamed = true, bool subParent = false)
        {
            string result = string.Empty;
            if (parentNamed)
            {
                if (!subParent)
                {
                    for (int i = 1; i < level; i++)
                    {
                        result += "  ";
                    }
                }
            }
            else
            {
                for (int i = 2; i < level; i++)
                {
                    result += "  ";
                }
                result += "- ";
            }
            if (!string.IsNullOrEmpty(_Name))
            {
                result += _Name + ':' + ((Count > 0) ? "\r\n" : " ");
            }
            if (Count > 0)
            {
                bool cn = IsObject;
                for (int i = 0; i < Count; i++)
                {
                    if (i != 0)
                    {
                        result += "\r\n";
                    }
                    result += _Items[i].InternalYamlSave((parentNamed) ? level + 1 : level, cn, (i == 0) && !parentNamed);
                }
            }
            else
            {
                result += ValueType switch
                {
                    NodeValueType.Text or NodeValueType.Time => YamlText(Text ?? string.Empty),
                    NodeValueType.Empty => (IsObject) ? "{}" : "[]",
                    _ => Text,
                };
            }
            return result;
        }

        public int Count => _Items.Count;

        public JSONNode? Parent { get; private set; } = null;

        public JSONNode? this[int index] => GetItem(index);

        public JSONNode this[string index] => GetItem(index);

        public JSONNode Add(string itemName = "")
        {
            JSONNode result = new()
            {
                Name = itemName,
                Parent = this
            };
            _Items.Add(result);
            return result;
        }

        public string Save(bool isCompact = true)
        {
            return ((Count == 0) && (!string.IsNullOrWhiteSpace(Name))) ? "{" + InternalSave((isCompact) ? -1 : 0) + "}" : InternalSave((isCompact) ? -1 : 0);
        }

        public string SaveYaml()
        {
            return InternalYamlSave();
        }

        public byte[] SaveAs(bool isCompact = false, int encoding = 50001, bool includeBOM = false)
        {
            MemoryStream m = new();
            SaveAs(m, isCompact, encoding, includeBOM);
            byte[] b = new byte[m.Length];
            m.Read(b, 0, b.Length);
            m.Close();
            return b;
        }

        public byte[] SaveYamlAs(int encoding = 50001, bool includeBOM = false)
        {
            MemoryStream m = new();
            SaveYamlAs(m, encoding, includeBOM);
            byte[] b = new byte[m.Length];
            m.Read(b, 0, b.Length);
            m.Close();
            return b;
        }

        public void SaveAs(Stream json, bool isCompact = false, int encoding = 50001, bool includeBOM = false)
        {
            int enc = (encoding == 50001) ? DefaultEncoding : encoding;
            if (includeBOM)
            {
                json.Write(GetBOM(enc));
            }
            json.Write(Encoding.GetEncoding(enc).GetBytes(Save(isCompact)));
        }

        public void SaveYamlAs(Stream json, int encoding = 50001, bool includeBOM = false)
        {
            int enc = (encoding == 50001) ? DefaultEncoding : encoding;
            if (includeBOM)
            {
                json.Write(GetBOM(enc));
            }
            json.Write(Encoding.GetEncoding(enc).GetBytes(SaveYaml()));
        }

        public void SaveToFile(string fileName, bool isCompact = false, int encoding = 50001, bool includeBOM = false)
        {
            using FileStream f = File.Create(fileName);
            SaveAs(f, isCompact, encoding, includeBOM);
            f.Close();
        }

        public void SaveYamlToFile(string fileName, int encoding = 50001, bool includeBOM = false)
        {
            using FileStream f = File.Create(fileName);
            SaveYamlAs(f, encoding, includeBOM);
            f.Close();
        }

        public bool Remove(JSONNode item)
        {
            return _Items.Remove(item);
        }

        public bool IsObject
        {
            get { return IsManualObject || ChildrenIsNamed(); }
            set { IsManualObject = value; }
        
        }

        private static void DefinitionVarType(JSONNode node, string value)
        {
            string val = value.Trim().ToLower();
            if ((val.Length > 0) && (val[0] == '"') && (val[^1] == '"'))
            {
                node.Text = value.Trim()[1..^1];
            }
            else if (val == "null")
            {
                node.Int = null;
            }
            else if (val == "true")
            {
                node.Bool = true;
            }
            else if (val == "false")
            {
                node.Bool = false;
            }
            else if (int.TryParse(val, out int val1))
            {
                node.Int = val1;
            }
            else if (long.TryParse(val, out long val2))
            {
                node.Long = val2;
            }
            else
            {
                string[] fn = val.Split('.');
                if (fn.Length > 1)
                {
                    if (fn[1].Length > 2)
                    {
                        if (double.TryParse(ReplaceDecimal(val), out double val3))
                        {
                            node.Float = val3;
                        }
                        else
                        {
                            node.Text = value;
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(ReplaceDecimal(val), out decimal val4))
                        {
                            node.Curr = val4;
                        }
                        else
                        {
                            node.Text = value;
                        }
                    }
                }
                else
                {
                    node.Text = value;
                }
            }
        }

        private static void DefinitionTextType(JSONNode node, string value)
        {
            if (value.Contains('T') && value.Contains('-') && value.Contains(':') && DateTime.TryParse(value.Trim(), out DateTime val))
            {
                node.Time = val;
            }
            else
            {
                node.Text = value;
            }

        }

        public void Load(string json)
        {
            JSONNode? cur = null;
            int level = 0, wordStart = 0;
            bool isWord = false, isValue = false, isSafe = false;
            string ValName = string.Empty;
            List<bool> levelArray = new();
            for (int i = 0; i < json.Length; i++)
            {
                switch (json[i])
                {
                    case '{':
                        if (!isWord)
                        {
                            cur = (level == 0) ? this : cur!.Add();
                            if (isValue)
                            {
                                cur.Name = ValName;
                            }
                            level++;
                            if (level > levelArray.Count)
                            {
                                levelArray.Add(false);
                            }
                            else
                            {
                                levelArray[level - 1] = false;
                            }
                            wordStart = i + 1;
                            isValue = false;
                            ValName = string.Empty;
                        }
                        isSafe = false;
                        break;
                    case '}':
                        if (!isWord)
                        {
                            if (isValue)
                            {
                                cur ??= this;
                                DefinitionVarType(cur.Add(ValName), json[wordStart..i]);
                                ValName = string.Empty;
                                isValue = false;
                            }
                            level--;
                            cur = (level == 0) ? null : cur!.Parent;
                            wordStart = i + 1;
                        }
                        isSafe = false;
                        break;
                    case '[':
                        if (!isWord)
                        {
                            cur = (level == 0) ? this : cur!.Add();
                            if (isValue)
                            {
                                cur.Name = ValName;
                            }
                            level++;
                            if (level > levelArray.Count)
                            {
                                levelArray.Add(true);
                            }
                            else
                            {
                                levelArray[level - 1] = true;
                            }
                            wordStart = i + 1;
                            isValue = false;
                            ValName = string.Empty;
                        }
                        isSafe = false;
                        break;
                    case ']':
                        if (!isWord)
                        {
                            if (isValue)
                            {
                                cur ??= this;
                                DefinitionVarType(cur.Add(ValName), json[wordStart..i]);
                                ValName = string.Empty;
                                isValue = false;
                            }
                            if (levelArray[level - 1] && (i - wordStart > 0))
                            {
                                DefinitionVarType(cur!.Add(), json[wordStart..i]);
                            }
                            level--;
                            cur = (level == 0) ? null : cur!.Parent;
                            wordStart = i + 1;
                        }
                        isSafe = false;
                        break;
                    case '\\':
                        if (isWord)
                        {
                            isSafe = true;
                        }
                        break;
                    case '"':
                        if (!isSafe)
                        {
                            if (isWord)
                            {
                                if (isValue)
                                {
                                    cur ??= this;
                                    DefinitionTextType(cur.Add(ValName), json[wordStart..i]);
                                    ValName = string.Empty;
                                    isValue = false;
                                }
                                else
                                {
                                    ValName = json[wordStart..i];
                                }
                            }
                            else
                            {
                                if (!levelArray[level - 1])
                                {
                                    wordStart = i + 1;
                                }
                            }
                            isWord = !isWord;

                        }
                        isSafe = false;
                        break;
                    case ':':
                        if (!isWord)
                        {
                            isValue = ValName.Length > 0;
                            wordStart = i + 1;
                        }
                        isSafe = false;
                        break;
                    case ',':
                        if (!isWord && isValue)
                        {
                            cur ??= this;
                            DefinitionVarType(cur.Add(ValName), json[wordStart..i]);
                            isValue = false;
                            ValName = string.Empty;
                        }
                        if (levelArray[level - 1] && (i - wordStart > 0))
                        {
                            DefinitionVarType(cur!.Add(), json[wordStart..i]);
                        }
                        wordStart = i + 1;
                        isSafe = false;
                        break;
                    default:
                        isSafe = false;
                        break;
                }
            }
        }

        public IEnumerator<JSONNode> GetEnumerator()
        {
            return new JSONNodeEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class JSONNodeEnumerator : IEnumerator<JSONNode>
        {
            public JSONNodeEnumerator(JSONNode xn) { enumerable = xn; }

            int index = -1;
            readonly JSONNode enumerable;

            public JSONNode Current
            {
                get { return enumerable[index]!; }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < enumerable.Count - 1)
                {
                    index++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}
