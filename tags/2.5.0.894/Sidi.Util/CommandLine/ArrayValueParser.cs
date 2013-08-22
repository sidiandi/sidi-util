using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class ArrayValueParser : IValueParser
    {
        public ArrayValueParser(Type type, Parser parser)
        {
            this.type = type;
            this.parser = parser;
        }

        Type type;
        Parser parser;

        Type ElementType
        {
            get
            {
                return type.GetElementType();
            }
        }

        public string Usage
        {
            get { return String.Format("list of {0}, enclosed with '[' and ']' or terminated with '{1}'", ElementType.Name, Parser.ListTerminator); }
        }

        public string UsageText
        {
            get { return Usage; }
        }

        public string Name
        {
            get { return type.Name; }
        }

        public string Syntax
        {
            get { return String.Format("'[' {0} {0} ... ']'", ElementType.Name, ElementType.Name); }
        }

        public ItemSource Source
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> Categories
        {
            get { throw new NotImplementedException(); }
        }

        public object Handle(IList<string> args, bool execute)
        {
            var values = new List<object>();
            if (args.Any() && args.First().Equals("["))
            {
                args.PopHead();
                while (!args.First().Equals("]"))
                {
                    values.Add(parser.ParseValue(args, ElementType));
                }
                args.PopHead();
            }
            else
            {
                while (args.Any())
                {
                    if (args.First().Equals(Parser.ListTerminator))
                    {
                        args.PopHead();
                        break;
                    }
                    values.Add(parser.ParseValue(args, ElementType));
                }
            }
            var array = Array.CreateInstance(ElementType, values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                array.SetValue(values[i], i);
            }
            return array;
        }
    }
}
