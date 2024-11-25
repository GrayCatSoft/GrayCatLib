/// <summary>
/// Class for working with XML documents
/// GrayCat library
/// (c) 2006-2025 Evgeniy Dolonin https://github.com/graycatsoft
/// </summary>

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GrayCat.XML
{
    #nullable enable

    public class XMLNode : StorageNode, IEnumerable<XMLNode>
    {
        #region Public-Members

        public int Tag { get; set; } = 0;
        
        public object? TagObject { get; set; } = null;

        public XMLNode(string name, int defaultEncoding = 65001)
        {
            Parent = null;
            DefaultEncoding = defaultEncoding;
            LastEncoding = DefaultEncoding;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IsItem = new XMLNodeIsItem() { Owner = this };
            if (name != null)
            {
                if ((name.Length > 0) && (name[0] == '<'))
                {
                    Load(name);
                }
                else
                {
                    Name = name;
                }
            }
        }

        public XMLNode(int defaultEncoding = 65001)
        {
            Parent = null;
            DefaultEncoding = defaultEncoding;
            LastEncoding = DefaultEncoding;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IsItem = new XMLNodeIsItem() { Owner = this };
        }

        public XMLNode(string name, XMLNode parent)
        {
            Parent = parent;
            if (Parent != null)
            {
                DefaultEncoding = Parent.DefaultEncoding;
                LastEncoding = DefaultEncoding;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IsItem = new XMLNodeIsItem() { Owner = this };
            if ((name.Length > 0) && (name[0] == '<'))
            {
                Load(name);
            }
            else
            {
                Name = name;
            }
        }

        public XMLNode(XMLNode parent)
        {
            Parent = parent;
            if (Parent != null)
            {
                DefaultEncoding = Parent.DefaultEncoding;
                LastEncoding = DefaultEncoding;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IsItem = new XMLNodeIsItem() { Owner = this };
        }

        ~XMLNode()
        {
            Clear();
            Attr.Clear();
            Parent?.InternalRemove(ParentIndex);
        }

        public string SchemaName
        {
            get
            {
                int i = _Name.IndexOf(':');
                if (i >= 0)
                {
                    return _Name[..i];
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                _Name = SafeNodeName(value) + ':' + ShortName;
            }
        }

        public string ShortName
        {
            get
            {
                int i = _Name.IndexOf(':');
                if (i >= 0)
                {
                    return _Name[(i + 1)..];
                }
                else
                {
                    return _Name;
                }
            }
            set
            {
                string schema = SchemaName;
                _Name = ((schema.Length > 0) ? schema + ':' : string.Empty) + SafeNodeName(value);
            }
        }

        public int Count => _Items.Count;

        public XMLNode Add(string name = "item")
        {
            XMLNode n = new(SafeNodeName(name), this);
            _Items.Add(n);
            return n;
        }

        public XMLNode Insert(int newIndex, string name = "item")
        {
            XMLNode n = new(SafeNodeName(name), this);
            _Items.Insert(newIndex, n);
            return n;
        }

        public void Insert(int newIndex, int count = 1, string name = "item")
        {
            for (int i = 0; i < count; i++)
            {
                XMLNode n = new(SafeNodeName(name), this);
                _Items.Insert(newIndex, n);
            }
        }

        public XMLNode? FindByAttribute(string attributeName, string value, bool isRecursion = false)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                XMLNode x = _Items[i];
                if (x.Attr.Exist(attributeName) && (x.Attr[attributeName].Text == value))
                {
                    n = x;
                    break;
                }
            }
            if ((n == null) && isRecursion)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    n = _Items[i].FindByAttribute(attributeName, value, isRecursion);
                    if (n != null)
                    {
                        break;
                    }
                }
            }
            return n;
        }

        public XMLNode? FindByAttribute(string attributeName, int value, bool isRecursion = false)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                XMLNode x = _Items[i];
                if (x.Attr.Exist(attributeName) && (x.Attr[attributeName].Int == value))
                {
                    n = x;
                    break;
                }
            }
            if ((n == null) && isRecursion)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    n = _Items[i].FindByAttribute(attributeName, value, isRecursion);
                    if (n != null)
                    {
                        break;
                    }
                }
            }
            return n;
        }

        public XMLNode? FindByAttribute(string attributeName, long value, bool isRecursion = false)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                XMLNode x = _Items[i];
                if (x.Attr.Exist(attributeName) && (x.Attr[attributeName].Long == value))
                {
                    n = x;
                    break;
                }
            }
            if ((n == null) && isRecursion)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    n = _Items[i].FindByAttribute(attributeName, value, isRecursion);
                    if (n != null)
                    {
                        break;
                    }
                }
            }
            return n;
        }

        public XMLNode? FindByAttribute(string attributeName, bool value, bool isRecursion = false)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                XMLNode x = _Items[i];
                if (x.Attr.Exist(attributeName) && (x.Attr[attributeName].Bool == value))
                {
                    n = x;
                    break;
                }
            }
            if ((n == null) && isRecursion)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    n = _Items[i].FindByAttribute(attributeName, value, isRecursion);
                    if (n != null)
                    {
                        break;
                    }
                }
            }
            return n;
        }

        public XMLNode? FindByAttribute(string attributeName, double value, bool isRecursion = false)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                XMLNode x = _Items[i];
                if (x.Attr.Exist(attributeName) && (x.Attr[attributeName].Float == value))
                {
                    n = x;
                    break;
                }
            }
            if ((n == null) && isRecursion)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    n = _Items[i].FindByAttribute(attributeName, value, isRecursion);
                    if (n != null)
                    {
                        break;
                    }
                }
            }
            return n;
        }

        public XMLNode? FindByAttribute(string attributeName, DateTime value, bool isRecursion = false)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                XMLNode x = _Items[i];
                if (x.Attr.Exist(attributeName) && (x.Attr[attributeName].Time == value))
                {
                    n = x;
                    break;
                }
            }
            if ((n == null) && isRecursion)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    n = _Items[i].FindByAttribute(attributeName, value, isRecursion);
                    if (n != null)
                    {
                        break;
                    }
                }
            }
            return n;
        }

        public XMLNode? FindByTag(int tag)
        {
            XMLNode? n = null;
            for (int i = 0; i < _Items.Count; i++)
            {
                if (_Items[i].Tag == tag)
                {
                    n = _Items[i];
                    break;
                }
            }
            return n;
        }

        public XMLNode? Find(string name, bool caseSens = true)
        {
            XMLNode? result = null;
            foreach (XMLNode a in _Items)
            {
                if (caseSens)
                {
                    if (a.Name == name.Trim())
                    {
                        result = a;
                        break;
                    }
                }
                else
                {
                    if (a.Name.ToLower() == name.Trim().ToLower())
                    {
                        result = a;
                        break;
                    }
                }
            }
            return result;
        }

        public XMLNode? Find(string schemaName, string shortName, bool caseSens = true)
        {
            return Find(schemaName + ':' + shortName, caseSens);
        }

        public void Clear()
        {
            foreach (XMLNode n in _Items)
            {
                n.Clear(); 
                // принудителный деструктор
            }
            _Items.Clear();
        }

        public void Close()
        {
            Clear();
            Attr.Clear();
            Name = string.Empty;
        }

        public void Delete(int index)
        {
            if ((index >= 0) && (index < Count))
            {
                _Items[index].Clear(); 
                // принудительный деструктор
                _Items.RemoveAt(index);
            }
        }

        public void Empty()
        {
            Parent?.InternalRemove(ParentIndex);
            Clear();
            Attr.Clear();
            _Value = null;
            ValueType = NodeValueType.Empty;
        }

        public void Assign(XMLNode? node, bool isTree)
        {
            if (node != null)
            {
                _Name = node.Name;
                Tag = node.Tag;
                TagObject = node.TagObject;
                ValueType = node.ValueType;
                _Value = node._Value;
                Attr.Clear();
                for (int i = 0; i < node.Attr.Count; i++)
                {
                    Attr[node.Attr[i].Name].Assign(node.Attr[i]);
                }
                if (isTree)
                {
                    Clear();
                    for (int i = 0; i < node.Count; i++)
                    {
                        Add().Assign(node[i], isTree);
                    }
                }
            }
        }

        public void SortByAttr(string attributeName, bool isReverce = false, bool isNumeric = false)
        {
            bool t = true;
            while (t)
            {
                t = false;
                for (int i = 0; i < _Items.Count - 1; i++)
                {
                    bool c;
                    if (isNumeric)
                    {
                        if (isReverce)
                        {
                            c = _Items[i].Attr[attributeName].Int < _Items[i + 1].Attr[attributeName].Int;
                        }
                        else
                        {
                            c = _Items[i].Attr[attributeName].Int > _Items[i + 1].Attr[attributeName].Int;
                        }
                    }
                    else
                    {
                        if (isReverce)
                        {
                            c = string.Compare(_Items[i].Attr[attributeName].Text, _Items[i + 1].Attr[attributeName].Text) < 0;
                        }
                        else
                        {
                            c = string.Compare(_Items[i].Attr[attributeName].Text, _Items[i + 1].Attr[attributeName].Text) > 0;
                        }
                    }
                    if (c)
                    {
                        t = true;
                        _Items[i + 1].MoveUp();
                    }
                }
            }
        }

        public bool MoveUp()
        {
            bool r = Parent != null;
            if (r)
            {
                int n = ParentIndex;
                r = (n > 0);
                if (r)
                {
                    XMLNode x = Parent!._Items[n];
                    Parent!._Items.Remove(x);
                    Parent!._Items.Insert(n - 1, x);
                }
            }
            return r;
        }

        public bool MoveDown()
        {
            bool r = Parent != null;
            if (r)
            {
                int n = ParentIndex;
                r = (n < (Parent!.Count - 1));
                if (r)
                {
                    XMLNode x = Parent!._Items[n];
                    Parent!._Items.Remove(x);
                    Parent!._Items.Insert(n + 1, x);
                }
            }
            return r;
        }

        public bool MoveTo(int newIndex)
        {
            bool r = Parent != null;
            if (r)
            {
                int n = ParentIndex;
                r = (n != newIndex);
                if (r)
                {
                    XMLNode x = Parent!._Items[n];
                    Parent!._Items.Remove(x);
                    Parent!._Items.Insert(newIndex, x);
                }
            }
            return r;
        }

        public bool MoveToParent(XMLNode? newParent)
        {
            bool r = (newParent != null);
            if (r)
            {
                Parent?._Items.Remove(this);
                Parent = newParent;
                Parent!._Items.Add(this);
            }
            return r;
        }

        public bool PullUp()
        {
            bool r = (Parent != null);
            if (r)
            {
                for (int i = 0; i < _Items.Count; i++)
                {
                    Parent!.InternalAdd(_Items[i]);
                }
                _Items.Clear();
                Parent!.InternalRemove(ParentIndex);
            }
            return r;
        }

        public XMLNode Unbind()
        {
            if (Parent != null)
            {
                Parent.InternalUnbind(this);
                Parent = null;
            }
            return this;
        }

        public void Load(string xml)
        {
            int ln = 1, col = 0;
            bool isError = false;
            string errorMsg = string.Empty, errorTag = string.Empty;
            XMLNode? cd = null;
            try
            {
                Clear();
                Attr.Clear();
                bool isOpen = false, isTag = false, isCloseTag = false, isMark = false, isTagName = false, isClosed = false,
                     isQuote = false, isRemark = false, isHeader = false, isSpace = false, isParam = false, isParamName = false,
                     isParamNeed = false, isParamVal = false, isBody = false, isCR = false, isCData = false;
                string parName = string.Empty, parValue = string.Empty;
                int rmp = 0, level = 0, start = 0, isSubOn = 0, isSubOff = 0;
                for (int i = 0; i < xml.Length; i++)
                {
                    if ((xml[i] == '\n') || isCR)
                    {
                        isCR = false;
                        ln++;
                        col = 1;
                    }
                    else
                    {
                        col++;
                    }
                    switch (xml[i])
                    {
                        case '\t':
                        case '\n':
                        case '\r':
                        case ' ':
                            if (!isQuote && !isRemark && !isCData)
                            {
                                if (isTagName)
                                {
                                    if (isTag && (cd != null))
                                    {
                                        cd.Name = xml[start..i];
                                        isTagName = false;
                                    }
                                    if (isCloseTag && (cd != null))
                                    {
                                        isError = cd.Name != xml[start..i];
                                        if (isError)
                                        {
                                            errorMsg = "Tag not closed";
                                            errorTag = cd.Name;
                                        }
                                        isTagName = false;
                                    }
                                    if (isMark)
                                    {
                                        isHeader = xml[start..i].ToLower() == "xml";
                                        isTagName = false;
                                    }
                                }
                                if (isParam)
                                {
                                    if (isParamName)
                                    {
                                        parName = xml[start..i];
                                        isParamName = false;
                                    }
                                    if (isParamVal)
                                    {
                                        parValue = xml[start..i];
                                        if (isTag && (cd != null))
                                        {
                                            cd.Attr[parName].Text = XMLCharDecode(parValue);
                                        }
                                        isParamVal = false;
                                        isParam = false;
                                    }
                                }
                                if ((isTag || isHeader) && !isCloseTag)
                                {
                                    isSpace = true;
                                }
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            isClosed = false;
                            break;
                        case '!':
                            if (!isCData)
                            {
                                if (isOpen)
                                {
                                    isSubOn = 1;
                                }
                                rmp = (!isQuote && isOpen) ? 1 : 0;
                                isClosed = false;
                            }
                            isSubOff = 0;
                            break;
                        case '"':
                            if (!isRemark && !isBody && !isCData)
                            {
                                if (isParamVal && isQuote)
                                {
                                    parValue = xml[start..i];
                                    if (isTag && (cd != null))
                                    {
                                        cd.Attr[parName].Text = XMLCharDecode(parValue);
                                    }
                                    isParamVal = false;
                                    isParam = false;
                                }
                                if (isParamNeed && !isQuote)
                                {
                                    isParamVal = true;
                                    isParamNeed = false;
                                    start = i + 1;
                                }
                                isQuote = !isQuote;
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            isClosed = false;
                            break;
                        case '-':
                            if (!isQuote && !isCData)
                            {
                                if (isRemark)
                                {
                                    rmp = (rmp == -1) ? -2 : ((rmp == 0) ? -1 : 0);
                                }
                                else
                                {
                                    if (rmp > 0)
                                    {
                                        if (rmp == 2)
                                        {
                                            isRemark = true;
                                        }
                                        else
                                        {
                                            rmp = (rmp == 1) ? 2 : 0;
                                        }
                                    }
                                }
                                if (!isRemark && isParamNeed)
                                {
                                    isParamVal = true;
                                    isParamNeed = false;
                                    start = i;
                                }
                                isClosed = false;
                            }
                            isSubOn = 0;
                            isSubOff = 0;
                            break;
                        case '/':
                            if (!isQuote && !isRemark && !isCData)
                            {
                                if (isParam && isParamVal)
                                {
                                    parValue = xml[start..i];
                                    if (isTag && (cd != null))
                                    {
                                        cd.Attr[parName].Text = XMLCharDecode(parValue);
                                    }
                                }
                                if (isTag)
                                {
                                    isClosed = true;
                                    if (isTagName && (cd != null))
                                    {
                                        cd.Name = xml[start..i];
                                        isTagName = false;
                                    }
                                }
                                if (isOpen)
                                {
                                    isCloseTag = true;
                                }
                                isParam = false;
                                isParamName = false;
                                isParamVal = false;
                                parName = string.Empty;
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            break;
                        case '<':
                            if (!isQuote && !isRemark && !isCData)
                            {
                                if (isBody)
                                {
                                    string body = xml[start..i];
                                    if ((body.Trim().Length > 0) && (cd != null))
                                    {
                                        cd.Text = XMLCharDecode(body);
                                    }
                                }
                                isOpen = true;
                                isBody = false;
                                isSpace = false;
                                isParam = false;
                                isParamName = false;
                                isParamNeed = false;
                                isParamVal = false;
                                parName = string.Empty;
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            isClosed = false;
                            break;
                        case '=':
                            if (!isQuote && !isRemark && !isCData && isParam)
                            {
                                if (isParamName)
                                {
                                    parName = xml[start..i];
                                    isParamName = false;
                                }
                                isParamNeed = true;
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            isClosed = false;
                            break;
                        case '>':
                            if (!isQuote && !isRemark && !isCData)
                            {
                                if (isParam && isParamVal)
                                {
                                    parValue = xml[start..i];
                                    if (isTag && (cd != null))
                                    {
                                        cd.Attr[parName].Text = XMLCharDecode(parValue);
                                    }
                                }
                                if (isTag || isCloseTag)
                                {
                                    if (isTagName && (cd != null))
                                    {
                                        string s = xml[start..i];
                                        if (isCloseTag)
                                        {
                                            isError = (s != cd.Name);
                                            if (isError)
                                            {
                                                errorMsg = "Opening and closing tags do not match";
                                                errorTag = cd.Name;
                                            }
                                        }
                                        else
                                        {
                                            cd.Name = s;
                                        }
                                        isTagName = false;
                                    }
                                    if ((isClosed || isCloseTag) && (cd != null))
                                    {
                                        cd = cd.Parent;
                                        level--;
                                    }
                                    isTag = false;
                                    isCloseTag = false;
                                }
                                if (isMark)
                                {
                                    isMark = false;
                                    isHeader = false;
                                }
                                if (!isClosed)
                                {
                                    isBody = true;
                                    isQuote = false;
                                    start = i + 1;
                                }
                                isClosed = false;
                                isSpace = false;
                                isParam = false;
                                isParamName = false;
                                isParamNeed = false;
                                isParamVal = false;
                                parName = string.Empty;
                            }
                            if (isRemark && (rmp == -2))
                            {
                                isRemark = false;
                            }
                            if (isCData && (isSubOff > 1))
                            {
                                isCData = false;
                                if (cd != null)
                                {
                                    cd.Text = xml[start..(i - 2)];
                                }
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            break;
                        case '?':
                            if (!isQuote && !isRemark && !isCData)
                            {
                                if (isOpen)
                                {
                                    isMark = true;
                                    isOpen = false;
                                    isTagName = true;
                                    start = i + 1;
                                    isClosed = false;
                                }
                                else
                                {
                                    isClosed = true;
                                }
                            }
                            rmp = 0;
                            isSubOn = 0;
                            isSubOff = 0;
                            break;
                        case '[':
                            if (!isCData && (isSubOn > 0))
                            {
                                if (isSubOn == 2)
                                {
                                    string s = xml[start..i];
                                    if (s == "CDATA")
                                    {
                                        isCData = true;
                                        isSubOn = 0;
                                        start = i + 1;
                                    }
                                }
                                if (isOpen && (isSubOn == 1))
                                {
                                    isOpen = false;
                                    isSubOn = 2;
                                    start = i + 1;
                                }
                            }
                            isSubOff = 0;
                            break;
                        case ']':
                            if (isCData)
                            {
                                isSubOff++;
                            }
                            break;
                        default:
                            if (!isQuote && !isRemark && !isCData && (isSubOn == 0))
                            {
                                if (isSpace)
                                {
                                    if (isParam)
                                    {
                                        isParamVal = true;
                                        start = i;
                                    }
                                    else
                                    {
                                        isParam = true;
                                        isParamName = true;
                                        start = i;
                                    }
                                    isSpace = false;
                                }
                                if (isOpen)
                                {
                                    isTag = true;
                                    isTagName = true;
                                    isOpen = false;
                                    start = i;
                                    if (!isCloseTag)
                                    {
                                        cd = (cd != null) ? cd.Add() : this;
                                        level++;
                                    }
                                }
                                if (isParamNeed)
                                {
                                    isParamVal = true;
                                    isParamNeed = false;
                                    start = i;
                                }
                            }
                            isSubOff = 0;
                            rmp = 0;
                            isClosed = false;
                            break;
                    }
                    if (isError)
                    {
                        break;
                    }
                    if (xml[i] == '\r')
                    {
                        isCR = true;
                    }
                }
                if (!isError && ((level != 0) || (cd != null)))
                {
                    isError = true;
                    errorMsg = "Tag not closed";
                    errorTag = (cd != null) ? cd.Name : "-";
                }
                if (isError)
                {
                    Clear();
                    Attr.Clear();
                    throw new XMLParsingException(errorMsg, ln, col, errorTag);
                }
                else
                {
                    Loaded = true;
                }
            }
            catch (Exception ex)
            {
                Clear();
                Attr.Clear();
                throw new XMLParsingException(ex.Message, ln, col, (cd != null) ? cd.Name : "-");
            }
        }

        public void Load(byte[] xml, int encoding = 50001)
        {
            int start = 0;
            LastEncoding = encoding;
            if ((LastEncoding == 50001) && (xml.Length > 1) && (xml.Length % 2 > 0) && (xml[0] == 255) && (xml[1] == 254))
            {
                LastEncoding = 1200;
            }
            if ((LastEncoding == 50001) && (xml.Length > 1) && (xml.Length % 2 > 0) && (xml[0] == 254) && (xml[1] == 255))
            {
                LastEncoding = 1201;
            }
            if (LastEncoding == 50001)
            {
                int a = 0;
                int c = 0;
                for (int i = 0; i < xml.Length; i++)
                {
                    if ((a == 0) && (i > 0) && (xml[i - 1] == 60) && (xml[i] == 63))
                    {
                        a = i + 1;
                    }
                    if ((a > 0) && (xml[i - 1] == 63) && (xml[i] == 62))
                    {
                        c = i - a - 1;
                        break;
                    }
                }
                if (c > 12)
                {
                    string hs = Encoding.ASCII.GetString(xml, a, c).ToLower();
                    int n = hs.IndexOf("encoding=");
                    if ((n > 0) && (hs[..3] == "xml"))
                    {
                        string cp = hs[(n + 9)..].Replace('"', ' ').Trim();
                        if (cp.Contains("utf-8", StringComparison.CurrentCulture))
                        {
                            LastEncoding = 65001;
                        }
                        else
                        {
                            if (cp.Contains("utf-7", StringComparison.CurrentCulture))
                            {
                                LastEncoding = 65000;
                            }
                            else
                            {
                                if (cp.Contains("unicode", StringComparison.CurrentCulture))
                                {
                                    LastEncoding = 1200;
                                }
                                else
                                {
                                    n = cp.IndexOf("windows-");
                                    if (n >= 0)
                                    {
                                        string wcn = cp[(n + 8)..].Trim();
                                        if (wcn.Length > 0)
                                        {
                                            LastEncoding = int.Parse(wcn);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    start = a + c + 2;
                }
            }
            if (LastEncoding == 50001)
            {
                LastEncoding = DefaultEncoding;
            }
            Load(Encoding.GetEncoding(LastEncoding).GetString(xml, start, xml.Length - start).Trim());
        }

        public void Load(Stream xml, int encoding = 50001)
        {
            byte[] b = new byte[xml.Length];
            xml.Seek(0, SeekOrigin.Begin);
            xml.Read(b, 0, b.Length);
            Load(b, encoding);
        }

        public bool LoadFromFile(string fileName, int encoding = 50001)
        {
            bool r = File.Exists(fileName);
            if (r)
            {
                using FileStream f = File.OpenRead(fileName);
                Load(f, encoding);
                f.Close();
            }
            return r;
        }

        public string Save(bool isCompact = false)
        {
            return (isCompact) ? InternalCompactSave() : InternalSave(0, FormattingTabulator);
        }

        public byte[] SaveAs(int encoding = 50001, bool includeBOM = true)
        {
            MemoryStream m = new();
            SaveAs(m, encoding, includeBOM);
            byte[] b = new byte[m.Length];
            m.Position = 0;
            m.Read(b, 0, b.Length);
            m.Close();
            return b;
        }

        public void SaveAs(Stream xml, int encoding = 50001, bool includeBOM = true)
        {
            int enc = (encoding == 50001) ? DefaultEncoding : encoding;
            if (includeBOM)
            {
                xml.Write(GetBOM(enc));
            }
            xml.Write(GetXMLHeader(enc));
            xml.Write(Encoding.GetEncoding(enc).GetBytes(Save()));
        }

        public void SaveToFile(string fileName, int encoding = 50001, bool includeBOM = true)
        {
            using FileStream f = File.Create(fileName);
            SaveAs(f, encoding, includeBOM);
            f.Close();
        }

        public int LastEncoding { get; private set; } = 65001;

        public string FormattingTabulator { get; set; } = "\t";

        public XMLNode? Parent { get; private set; } = null;

        public int ParentIndex => GetParentIndex();

        public XMLNode Root => GetRoot();

        public string RootPath => GetRootPath();

        public bool IsName(string name, bool caseSens = true)
        {
            if (caseSens)
            {
                return _Name == name.Trim();
            }
            else
            {
                return _Name.ToLower() == name.Trim().ToLower();
            }
        }

        public bool IsSchemaName(string name, bool caseSens = true)
        {
            if (caseSens)
            {
                return SchemaName == name.Trim();
            }
            else
            {
                return SchemaName.ToLower() == name.Trim().ToLower();
            }
        }

        public bool IsShortName(string name, bool caseSens = true)
        {
            if (caseSens)
            {
                return ShortName == name.Trim();
            }
            else
            {
                return ShortName.ToLower() == name.Trim().ToLower();
            }
        }

        public bool IsAttr(string attributeName)
        {
            return Attr.Exist(attributeName);
        }

        public XMLNode? this[int index] => GetItem(index);

        public XMLNode? this[string index] => GetItem(index);

        public XMLNodeIsItem IsItem { get; }

        public XMLAttributes Attr { get; } = new XMLAttributes();

        public bool Loaded { get; set; } = false;

        public IEnumerator<XMLNode> GetEnumerator()
        {
            return new XMLNodeEnumerator(this);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class XMLNodeEnumerator : IEnumerator<XMLNode>
        {
            public XMLNodeEnumerator(XMLNode xn) { enumerable = xn; }

            int index = -1;
            readonly XMLNode enumerable;

            public XMLNode Current
            {
                get 
                {
                    return enumerable[index]!; 
                }
            }

            object? IEnumerator.Current
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

        public class XMLNodeIsItem
        {
            public XMLNode? Owner;

            public XMLNode? this[int index] => Owner?.GetIsItem(index);

            public XMLNode? this[string index] => Owner?.GetIsItem(index);
        }

        public class XMLAttribute
        {
            private readonly XMLAttributes? Owner = null;
            private NodeValueType Type = NodeValueType.Empty;

            public XMLAttribute(XMLAttributes owner, string name)
            {
                Owner = owner;
                Name = name;
            }

            public object? Value { get; private set; } = null;

            public string Name { get; } = string.Empty;

            public string Text
            { 
                get 
                {
                    return Type switch
                    {
                        NodeValueType.Text => ((string?)Value) ?? string.Empty,
                        NodeValueType.Int => StorageNode.ConvertToText((int?)Value),
                        NodeValueType.Long => StorageNode.ConvertToText((long?)Value),
                        NodeValueType.Bool => StorageNode.ConvertToText((bool?)Value),
                        NodeValueType.Float => StorageNode.ConvertToText((double?)Value),
                        NodeValueType.Curr => StorageNode.ConvertToText((decimal?)Value),
                        NodeValueType.Time => StorageNode.ConvertToText((DateTime?)Value),
                        _ => string.Empty,
                    };
                }
                set 
                {
                    Type = NodeValueType.Text;
                    Value = value;
                } 
            }

            public int Int 
            { 
                get 
                {
                    return Type switch
                    {
                        NodeValueType.Text => StorageNode.ConvertTextInt((string?)Value),
                        NodeValueType.Int => ((int?)Value) ?? default,
                        NodeValueType.Long => Convert.ToInt32(((long?)Value) ?? default),
                        NodeValueType.Bool => (((bool?)Value) ?? default) ? 1 : 0,
                        NodeValueType.Float => Convert.ToInt32(Math.Round(((double?)Value) ?? default)),
                        NodeValueType.Curr => Convert.ToInt32(Math.Round(((decimal?)Value) ?? default)),
                        NodeValueType.Time => Convert.ToInt32((((DateTime?)Value) ?? default).ToOADate()),
                        _ => 0,
                    };
                }
                set 
                {
                    Type = NodeValueType.Int;
                    Value = value;
                }
            }

            public long Long
            {
                get
                {
                    return Type switch
                    {
                        NodeValueType.Text => StorageNode.ConvertTextLong((string?)Value),
                        NodeValueType.Int => ((int?)Value) ?? default,
                        NodeValueType.Long => ((long?)Value) ?? default,
                        NodeValueType.Bool => (((bool?)Value) ?? default) ? 1 : 0,
                        NodeValueType.Float => Convert.ToInt64(Math.Round(((double?)Value) ?? default)),
                        NodeValueType.Curr => Convert.ToInt64(Math.Round(((decimal?)Value) ?? default)),
                        NodeValueType.Time => Convert.ToInt64((((DateTime?)Value) ?? default).ToOADate()),
                        _ => 0,
                    };
                }
                set
                {
                    Type = NodeValueType.Long;
                    Value = value;
                }
            }

            public bool Bool
            {
                get
                {
                    return Type switch
                    {
                        NodeValueType.Text => StorageNode.ConvertTextBool((string?)Value),
                        NodeValueType.Int => (((int?)Value) ?? default) > 0,
                        NodeValueType.Long => (((long?)Value) ?? default) > 0,
                        NodeValueType.Bool => ((bool?)Value) ?? default,
                        NodeValueType.Float => (((double?)Value) ?? default) > 0,
                        NodeValueType.Curr => (((decimal?)Value) ?? default) > 0,
                        NodeValueType.Time => (((DateTime?)Value) ?? default).ToOADate() > 0,
                        _ => false,
                    };
                }
                set
                {
                    Type = NodeValueType.Bool;
                    Value = value;
                }
            }

            public double Float
            {
                get
                {
                    return Type switch
                    {
                        NodeValueType.Text => StorageNode.ConvertTextFloat((string?)Value),
                        NodeValueType.Int => ((int?)Value) ?? default,
                        NodeValueType.Long => ((long?)Value) ?? default,
                        NodeValueType.Bool => (((bool?)Value) ?? default) ? 1 : 0,
                        NodeValueType.Float => ((double?)Value) ?? default,
                        NodeValueType.Curr => decimal.ToDouble(((decimal?)Value) ?? default),
                        NodeValueType.Time => (((DateTime?)Value) ?? default).ToOADate(),
                        _ => 0,
                    };
                }
                set
                {
                    Type = NodeValueType.Float;
                    Value = value;
                }
            }

            public decimal Curr
            {
                get
                {
                    return Type switch
                    {
                        NodeValueType.Text => StorageNode.ConvertTextCurr((string?)Value),
                        NodeValueType.Int => ((int?)Value) ?? default,
                        NodeValueType.Long => ((long?)Value) ?? default,
                        NodeValueType.Bool => (((bool?)Value) ?? default) ? 1 : 0,
                        NodeValueType.Float => ((decimal?)Value) ?? default,
                        NodeValueType.Curr => ((decimal?)Value) ?? default,
                        NodeValueType.Time => (decimal)(((DateTime?)Value) ?? default).ToOADate(),
                        _ => 0,
                    };
                }
                set
                {
                    Type = NodeValueType.Curr;
                    Value = value;
                }
            }

            public DateTime Time
            {
                get
                {
                    return Type switch
                    {
                        NodeValueType.Text => StorageNode.ConvertTextTime((string?)Value),
                        NodeValueType.Int => DateTime.FromOADate(((int?)Value) ?? default),
                        NodeValueType.Long => DateTime.FromOADate(((long?)Value) ?? default),
                        NodeValueType.Bool => DateTime.FromOADate((((bool?)Value) ?? default) ? 1 : 0),
                        NodeValueType.Float => DateTime.FromOADate(((double?)Value) ?? default),
                        NodeValueType.Curr => DateTime.FromOADate(decimal.ToDouble(((decimal?)Value) ?? default)),
                        NodeValueType.Time => ((DateTime?)Value) ?? default,
                        _ => default,
                    };
                }
                set
                {
                    Type = NodeValueType.Time;
                    Value = value;
                }
            }

            public bool IsSet => (Type != NodeValueType.Empty);

            public void Empty()
            {
                if (Owner != null)
                {
                    Value = null;
                    Type = NodeValueType.Empty; 
                    Owner.ItemRemove(this);
                }
            }

            public void Assign(XMLAttribute attr)
            {
                Value = attr.Value;
                Type = attr.Type;
            }
        }

        public class XMLAttributes : IEnumerable<XMLAttribute>
        {
            private readonly List<XMLAttribute> _Attrs = new();

            public XMLAttribute this[int index] => GetAttribute(index);

            public XMLAttribute this[string index] => GetAttribute(index);

            public int Count => _Attrs.Count;

            public void Clear()
            {
                _Attrs.Clear();
            }

            public bool Exist(string name)
            {
                bool r = false;
                for (int i = 0; i < _Attrs.Count; i++)
                {
                    if (_Attrs[i].IsSet && (_Attrs[i].Name == name))
                    {
                        r = true;
                        break;
                    }
                }
                return r;
            }

            public void ItemRemove(XMLAttribute item)
            {
                _Attrs.Remove(item);
            }

            private XMLAttribute GetAttribute(int index)
            {
                if (index < 0) throw new Exception("Index must be positive");
                if (index >= _Attrs.Count) throw new Exception("Index must be less than count");
                return _Attrs[index];
            }

            private XMLAttribute GetAttribute(string index)
            {
                string s = index.Trim();
                if (String.IsNullOrEmpty(s)) throw new Exception("Index must not be empty");
                int n = -1;
                for (int i = 0; i < _Attrs.Count; i++)
                {
                    if (_Attrs[i].Name == s)
                    {
                        n = i;
                        break;
                    }
                }
                if (n < 0)
                {
                    if (s.Contains('=')) throw new Exception("Index cannot contain character =");
                    if (s.Contains(' ')) throw new Exception("Index cannot contain character space");
                    if (s.Contains('>')) throw new Exception("Index cannot contain character >");
                    if (s.Contains('<')) throw new Exception("Index cannot contain character <");
                    if (s.Contains('"')) throw new Exception("Index cannot contain character \"");
                    if (s.Contains('\'')) throw new Exception("Index cannot contain character '");
                    if ((s[0] >= '0') && (s[0] <= '9')) throw new Exception("Index cannot contain a number in the first character");
                    n = _Attrs.Count;
                    _Attrs.Add(new XMLAttribute(this, s));
                }
                return _Attrs[n];
            }

            public IEnumerator<XMLAttribute> GetEnumerator()
            {
                return new XMLAttributeEnumerator(this);
            }
            
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            class XMLAttributeEnumerator : IEnumerator<XMLAttribute>
            {
                public XMLAttributeEnumerator(XMLAttributes xa) { enumerable = xa; }

                int index = -1;
                readonly XMLAttributes enumerable;

                public XMLAttribute Current
                {
                    get { return enumerable[index]; }
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

        #endregion

        #region Private-Members

        private readonly List<XMLNode> _Items = new();

        protected override string SafeNodeName(string newName)
        {
            string s = newName.Trim();
            s = s.Replace('<', '_').Replace('>', '_').Replace(' ', '_');
            if (s.Length > 0)
            {
                char c = s[0];
                if ((c >= '0') && (c <= '9'))
                {
                    s = string.Concat("_", s.AsSpan(1));
                }
            }
            return (s.Length > 0) ? s : "item";
        }

        public static byte[] GetXMLHeader(int encoding)
        {
            string s = encoding switch
            {
                1200 => "utf-16",
                1201 => "unicodeFFFE",
                1250 => "Windows-1250",
                1251 => "Windows-1251",
                1252 => "Windows-1252",
                1253 => "Windows-1253",
                1254 => "Windows-1254",
                1255 => "Windows-1255",
                1256 => "Windows-1256",
                1257 => "Windows-1257",
                1258 => "Windows-1258",
                1259 => "Windows-1259",
                65000 => "utf-7",
                65001 => "utf-8",
                _ => Encoding.GetEncoding(encoding).EncodingName,
            };
            return Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"" + s + "\"?>\r\n");
        }

        private void InternalAdd(XMLNode item)
        {
            _Items.Add(item);
            item.Parent = this;
        }

        private void InternalRemove(int index)
        {
            if ((index >= 0) && (index < _Items.Count))
            {
                _Items.RemoveAt(index);
            }
        }

        private void InternalUnbind(XMLNode item)
        {
            _Items.Remove(item);
        }

        private string InternalSave(int level, string tab)
        {
            StringBuilder r = new();
            r.Append(CloneString(tab, level) + "<" + Name);
            for (int i = 0; i < Attr.Count; i++)
            {
                if (Attr[i].IsSet)
                {
                    r.Append(" " + Attr[i].Name + "=\"" + XMLCharEncode(Attr[i].Text) + "\"");
                }
            }
            if (_Items.Count == 0)
            {
                r.Append((String.IsNullOrEmpty(Text)) ? "/>\r\n" : ">" + XMLCharEncode(Text) + "</" + Name + ">\r\n");
            }
            else
            {
                r.Append(">" + XMLCharEncode(Text) + "\r\n");
                for (int i = 0; i < _Items.Count; i++)
                {
                    r.Append(_Items[i].InternalSave(level + 1, tab));

                }
                r.Append(CloneString(tab, level) + "</" + Name + ">\r\n");
            }
            return r.ToString();
        }

        private string InternalSaveOld(int level, string tab)
        {
            string r = CloneString(tab, level) + "<" + Name;
            for (int i = 0; i < Attr.Count; i++)
            {
                if (Attr[i].IsSet)
                {
                    r += " " + Attr[i].Name + "=\"" + XMLCharEncode(Attr[i].Text) + "\"";
                }
            }
            if (_Items.Count == 0)
            {
                r += (String.IsNullOrEmpty(Text)) ? "/>\r\n" : ">" + XMLCharEncode(Text) + "</" + Name + ">\r\n";
            }
            else
            {
                r += ">" + XMLCharEncode(Text) + "\r\n";
                for (int i = 0; i < _Items.Count; i++)
                {
                    r += _Items[i].InternalSaveOld(level + 1, tab);

                }
                r += CloneString(tab, level) + "</" + Name + ">\r\n";
            }
            return r;
        }

        private string InternalCompactSave()
        {
            string r = "<" + Name;
            for (int i = 0; i < Attr.Count; i++)
            {
                if (Attr[i].IsSet)
                {
                    r += " " + Attr[i].Name + "=\"" + XMLCharEncode(Attr[i].Text) + "\"";
                }
            }
            if (_Items.Count == 0)
            {
                r += (String.IsNullOrEmpty(Text)) ? "/>" : ">" + XMLCharEncode(Text) + "</" + Name + ">";
            }
            else
            {
                r += ">" + XMLCharEncode(Text);
                for (int i = 0; i < _Items.Count; i++)
                {
                    r += _Items[i].InternalCompactSave();

                }
                r += "</" + Name + ">";
            }
            return r;
        }

        private int GetParentIndex()
        {
            int r = -1;
            if (Parent != null)
            {
                for (int i = 0; i < Parent.Count; i++)
                {
                    if (Parent[i] == this)
                    {
                        r = i;
                        break;
                    }
                }
            }
            return r;
        }

        private XMLNode GetRoot()
        {
            return (Parent != null) ? Parent.GetRoot() : this;
        }

        private string GetRootPath()
        {
            return (Parent != null) ? Parent.GetRootPath() + " " + Name : Name;
        }

        private XMLNode? GetItem(int index)
        {
            XMLNode? r = null;
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

        private XMLNode? GetItem(string index)
        {
            XMLNode? r = null;
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
            if (!String.IsNullOrEmpty(s))
            {
                r = r[s];
            }
            return r;
        }

        private XMLNode? GetIsItem(int index)
        {
            return ((index >= 0) && (index < _Items.Count)) ? _Items[index] : null;
        }

        private XMLNode? GetIsItem(string index)
        {
            XMLNode? r = null;
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
            if ((r != null) && !String.IsNullOrEmpty(s))
            {
                r = r.IsItem[s];
            }
            return r;
        }

        public static string XMLCharEncode(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            string r = string.Empty;
            int n = 0;
            for (int i = 0; i < value.Length; i++)
            {
                string s = string.Empty;
                switch (value[i])
                {
                    case '<':
                        s = "&lt;";
                        break;
                    case '>':
                        s = "&gt;";
                        break;
                    case '&':
                        s = "&amp;";
                        break;
                    case '\'':
                        s = "&apos;";
                        break;
                    case '"':
                        s = "&quot;";
                        break;
                }
                if (!string.IsNullOrEmpty(s))
                {
                    r += ((i - n) > 0) ? value[n..i]  + s : s;
                    n = i + 1;
                }
            }
            if ((value.Length - n) > 0)
            {
                r += value[n..];
            }
            return r;
        }

        public static string XMLCharDecode(string value)
        {
            string r = string.Empty;
            int s = 0, c = 0;
            bool isSymb = false, isCode = false;
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '&':
                        if (!isSymb)
                        {
                            isSymb = true;
                            c = i;
                        }
                        break;
                    case '#':
                        if (isSymb && (c + 1 == i))
                        {
                            isSymb = false;
                            isCode = true;
                        }
                        break;
                    case ';':
                        if (isSymb)
                        {
                            isSymb = false;
                            string n = value.Substring(c + 1, i - c - 1).ToLower();
                            switch (n)
                            {
                                case "quot":
                                    r += value[s..c] + "\"";
                                    s = i + 1;
                                    break;
                                case "apos":
                                    r += value[s..c] + "\'";
                                    s = i + 1;
                                    break;
                                case "amp":
                                    r += value[s..c] + "&";
                                    s = i + 1;
                                    break;
                                case "lt":
                                    r += value[s..c] + "<";
                                    s = i + 1;
                                    break;
                                case "gt":
                                    r += value[s..c] + ">";
                                    s = i + 1;
                                    break;
                            }
                        }
                        break;
                    default:
                        if (isCode)
                        { 
                        }
                        break;
                }
                if (isSymb && (c + 4 < i))
                {
                    isSymb = false;
                }
            }
            r += value[s..];
            return r;
        }

        private static string CloneString(string value, int count)
        {
            string r = string.Empty;
            for (int i = 0; i < count; i++)
            {
                r += value;
            }
            return r;
        }

        #endregion
    }
    
    public class XMLParsingException : Exception
    {
        #region Public-Members

        public int Line { get; } = 0;
        public int Col { get; } = 0;
        public string TagName { get; } = String.Empty;
        public string Reason { get; } = String.Empty;
        public XMLParsingException(string reason, int line, int col, string tagName)
            : base($"XML Parse error: {reason}. Line {line}, Col: {col}, Tag: {tagName}")
        {
            Reason = reason;
            Line = line;
            Col = col;
            TagName = tagName;
        }

        public XMLParsingException()
        {
        }

        public XMLParsingException(string message) : base(message)
        {
        }

        public XMLParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }

}
