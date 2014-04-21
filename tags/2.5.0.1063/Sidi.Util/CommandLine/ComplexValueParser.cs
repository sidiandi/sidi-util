using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class ComplexValueParser : IValueParser
    {
        public ComplexValueParser(Type parser)
        {
            this.parser = parser;
        }

        Type parser;

        public Type ValueType
        {
            get
            {
                var p = Activator.CreateInstance(parser);
                return ((IValueContainer)p).ValueType;
            }
        }

        public string Usage
        {
            get
            {
                return Sidi.CommandLine.Usage.Get(parser);
            }
        }

        public string UsageText
        {
            get
            {
                using (var s = new StringWriter())
                {
                    s.Write(Usage);
                    var e = Examples;
                    if (e.Any())
                    {
                        if (!Usage.EndsWith("."))
                        {
                            s.Write(".");
                        }

                        s.Write(" Examples: ");
                        s.Write(e.Select(x => x.Value.Quote()).Join(", "));
                    }
                    return s.ToString();
                }
            }
        }

        public string Name
        {
            get { return ValueType.Name; }
        }

        public string Syntax
        {
            get { return ValueType.Name; }
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
            var valueParser = (IValueContainer) Activator.CreateInstance(parser);
            Parser.SingleSource(valueParser).ParseBraces(args);
            return valueParser.Value;
        }

        public IEnumerable<ExampleAttribute> Examples
        {
            get
            {
                return ExampleAttribute.Get(parser);
            }
        }
    }
}
