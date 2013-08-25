using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using Sidi.Extensions;
using Sidi.Util;

namespace Sidi.CommandLine
{
    public interface IValueParser : IParserItem
    {
    }

    public class ValueParser : IValueParser
    {
        public ValueParser(Parser parser, ItemSource source, MethodInfo method)
        {
            Source = source;
            MethodInfo = method;
            this.parser = parser;
        }

        public static bool IsSuitable(MethodInfo m)
        {
            var p = m.GetParameters();
            return m.Name.StartsWith("Parse")
                && m.IsStatic
                && p.Length == 1
                && (p[0].ParameterType.Equals(typeof(string)) || p[0].ParameterType.Equals(typeof(IList<string>)));
        }

        Parser parser;

        // must be like static bool Parse(string stringRepresentation, out <Type> result)
        public MethodInfo MethodInfo { private set; get; }

        public ItemSource Source { private set; get; }

        public string Usage
        {
            get { return Sidi.CommandLine.Usage.Get(MethodInfo); }
        }

        public IEnumerable<ExampleAttribute> Examples
        {
            get
            {
                return ExampleAttribute.Get(MethodInfo);
            }
        }

        public string UsageText
        {
            get
            {
                var s = new StringWriter();
                s.Write(Usage);
                var e = Examples;
                if (e.Any())
                {
                    s.Write(". Examples: ");
                    s.Write(e.Select(x => x.Value.Quote()).Join(", "));
                }
                return s.ToString();
            }
        }

        public string Syntax
        {
            get { return ValueType.Name; }
        }

        public string Name
        {
            get { return ValueType.Name; }
        }

        public IEnumerable<string> Categories
        {
            get
            {
                var c = MethodInfo.GetCustomAttributes(typeof(CategoryAttribute), true)
                    .Select(x => ((CategoryAttribute)x).Category);
                return c.Any() ? c : new string[] { String.Empty };
            }
        }

        public Type ValueType
        {
            get
            {
                return MethodInfo.ReturnType;
            }
        }

        public object Handle(IList<string> args, bool execute)
        {
            object result = null;
            if (this.MethodInfo.GetParameters()[0].ParameterType.Equals(typeof(IList<string>)))
            {
                try
                {
                    result = this.MethodInfo.Invoke(null, new object[] { args });
                }
                catch
                {
                    var p = Tokenizer.ToList(args[0]);
                    result = this.MethodInfo.Invoke(null, new object[] { p });
                    args.PopHead();
                }
            }
            else
            {
                result = this.MethodInfo.Invoke(null, new object[] { args[0] });
                args.PopHead();
            }
            return result;
        }
    }
}
