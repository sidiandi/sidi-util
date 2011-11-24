using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Sidi.IMAP
{
    public class ArgParser
    {
        public ArgParser(string argString)
        : this(argString, null, null)
        {
        }

        public ArgParser(string argString, TextReader input, TextWriter output)
        {
            this.input = input;
            this.output = output;
            this.argString = argString;
        }

        const string argSep = " ";
        
        public TextReader input;
        TextWriter output;
        string argString;
        int begin = 0;

        public string Rest
        {
            get
            {
                return argString.Substring(begin);
            }
        }
            
        public object GetArgument(ParameterInfo p)
        {
            return GetArgument(p.ParameterType);
        }

        public bool NextLine()
        {
            argString = input.ReadLine();
            begin = 0;
            return argString != null;
        }

        public object GetArgument(Type type)
        {
            if (type == typeof(ArgParser))
            {
                return this;
            }
            if (type == typeof(string))
            {
                if (argString[begin] == '"')
                {
                    return GetQuotedString();
                }
                else if (argString[begin] == '{')
                {
                    ++begin;
                    var end = Index("}");
                    var count = Int32.Parse(argString.Substring(begin, end - begin));
                    CommandContinuationRequest();
                    var b = new byte[count];
                    ((StreamReader)input).BaseStream.Read(b, 0, count);
                    argString = input.ReadLine();
                    begin = 0;
                    return System.Text.ASCIIEncoding.ASCII.GetString(b);
                }
                else
                {
                    return GetAtom();
                }
            }
            else if (type == typeof(object[]))
            {
                object o = Get();
                if (!(o is object[]))
                {
                    o = new object[] { o };
                }
                return o;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object Get()
        {
            if (IsNext("\""))
            {
                return GetQuotedString();
            }
            else if (IsNext("("))
            {
                return GetList();
            }
            else
            {
                return GetAtom();
            }
        }

        const string quote = "\"";

        Regex atom = new Regex(@"[^\(\)\{\ \}\[\]]+");

        public void ArgSep()
        {
            Consume(argSep);
        }

        public object[] GetList()
        {
            Consume("(");
            var elements = new List<object>();
            while (!ConsumeIfNext(")"))
            {
                elements.Add(Get());
                ConsumeIfNext(argSep);
            }
            return elements.ToArray();
        }

        public string GetQuotedString()
        {
            if (!ConsumeIfNext(quote))
            {
                throw new ParseException(this);
            }

            var end = Index(quote);
            var s = Part(begin, end);
            begin = end + quote.Length;
            return s;
        }

        public int GetNumber()
        {
            return Int32.Parse(Get(new Regex(@"\d+")));
        }

        public int GetNumber(int length)
        {
            return Int32.Parse(Get(new Regex(String.Format(@"\d{{{0}}}", length))));
        }

        public UInt32 GetUInt32()
        {
            return System.UInt32.Parse(Get(new Regex(@"\d+")));
        }

        public string Get(Regex re)
        {
            var m = re.Match(argString, begin);
            if (!m.Success || m.Index != begin)
            {
                throw new ParseException(this);
            }
            begin += m.Length;
            return m.Groups[0].Value;
        }

        public bool IsNext(Regex re)
        {
            var m = re.Match(argString, begin);
            return (m.Success && m.Index == begin);
        }

        public bool IsNext(string n)
        {
            if (begin <= argString.Length - n.Length)
            {
                if (argString.Substring(begin, n.Length).Equals(n, comparisonType))
                {
                    return true;
                }
            }
            return false;
        }

        public void Consume(string next)
        {
            if (!ConsumeIfNext(next))
            {
                throw new ParseException(this);
            }
        }

        public class ParseException : Exception
        {
            public ParseException(ArgParser p)
            {
                ArgString = p.argString;
                Index = p.begin;
            }

            public string ArgString;
            public int Index;

            public override string ToString()
            {
                return String.Format("{0}, {1}", ArgString, ArgString.Substring(Index));
            }
        }

        int Index(string x)
        {
            var i = argString.IndexOf(x, begin, comparisonType);
            if (i<0) { i = argString.Length; }
            return i;
        }

        string Part(int begin, int end)
        {
            return argString.Substring(begin, end - begin);
        }

        StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase;

        public bool ConsumeIfNext(string n)
        {
            if (begin <= argString.Length - n.Length)
            {
                if (argString.Substring(begin, n.Length).Equals(n, comparisonType))
                {
                    begin += n.Length;
                    return true;
                }
            }
            return false;
        }

        public bool IsEnd
        {
            get
            {
                return !(begin < argString.Length);
            }
        }

        public string GetAtom()
        {
            var m = atom.Match(argString, begin);
            if (m.Success)
            {
                begin += m.Length;
                return m.Groups[0].Value;
            }
            else
            {
                throw new ParseException(this);
            }
        }

        void CommandContinuationRequest()
        {
            output.WriteLine("+");
        }

        static Regex optionalWhitespace = new Regex(@"\s*");

        internal void SkipSpace()
        {
            Get(optionalWhitespace);
        }
    }
}
