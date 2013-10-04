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
            this.ValueType = type;
        }

        public Type ValueType { get; private set; }

        public string Usage
        {
            get { return String.Format("{0} value", ValueType.Name); }
        }

        public string UsageText
        {
            get { return String.Format("{0} enum. One of: {1}", ValueType.Name, Enum.GetValues(ValueType).Cast<object>().Join(", ")); }
        }

        public string Name
        {
            get { return ValueType.Name; }
        }

        public string Syntax
        {
            get
            {
                return Enum.GetValues(ValueType).Cast<object>().Join("|");
            }
        }

        public ItemSource Source
        {
            get { return null; }
        }

        public IEnumerable<string> Categories
        {
            get { return new string[] { }; }
        }

        public object Handle(IList<string> args, bool execute)
        {
            return Enum.Parse(ValueType, args.PopHead(), true);
        }

        public IEnumerable<ExampleAttribute> Examples
        {
            // todo
            get { return Enumerable.Empty<ExampleAttribute>(); }
        }
    }
}
