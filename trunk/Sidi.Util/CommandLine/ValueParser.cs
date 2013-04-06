using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Sidi.CommandLine
{
    public class ValueParser : IParserItem
    {
        public ValueParser(Parser parser, object application, MethodInfo method)
        {
            Application = application;
            MethodInfo = method;
            this.parser = parser;
        }

        public static bool IsSuitable(MethodInfo m)
        {
            var p = m.GetParameters();
            return m.Name.StartsWith("Parse")
                && p.Length == 1
                && p[0].ParameterType.Equals(typeof(string));
        }

        Parser parser;

        // must be like static bool Parse(string stringRepresentation, out <Type> result)
        public MethodInfo MethodInfo { private set; get; }

        public object Application { private set; get; }

        public string Usage
        {
            get { return String.Empty; }
        }

        public string UsageText
        {
            get { return String.Format("Can parse {0} values", ValueType); }
        }

        public string Syntax
        {
            get { return String.Format("Can parse {0} values", ValueType); }
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
            var result = this.MethodInfo.Invoke(null, new object[] { args[0] });
            args.RemoveAt(0);
            return result;
        }
    }
}
