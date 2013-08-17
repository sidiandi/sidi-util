using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class EnumValueParser : IValueParser
    {
        public EnumValueParser(Type type)
        {
            this.type = type;
        }

        Type type;

        public string Usage
        {
            get { return String.Format("{0} value", type.Name); }
        }

        public string UsageText
        {
            get { return String.Format("{0} enum. One of: {1}", type.Name, Enum.GetValues(type).Cast<object>().Join(", ")); }
        }

        public string Name
        {
            get { return type.Name; }
        }

        public string Syntax
        {
            get
            {
                return Enum.GetValues(type).Cast<object>().Join("|");
            }
        }

        public Application Application
        {
            get { return null; }
        }

        public IEnumerable<string> Categories
        {
            get { return new string[] { }; }
        }

        public object Handle(IList<string> args, bool execute)
        {
            return Enum.Parse(type, args.PopHead(), true);
        }
    }
}
