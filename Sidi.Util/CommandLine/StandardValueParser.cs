using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class StandardValueParser : IValueParser
    {
        public StandardValueParser(Type type)
        {
            this.ValueType = type;

            var parse = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            if (parse != null)
            {
                goto ok;
            }

            var ctor = type.GetConstructor(new Type[] { typeof(string) });
            if (ctor != null)
            {
                goto ok;
            }

            throw new CommandLineException(String.Format(@"Type {0} is not supported as a command line argument, because it does not have one of these methods:
public {0}(string)
public static {0} Parse(string)", type.Name));
        ok: ;
        }

        public Type ValueType { get; private set; }

        public string Usage
        {
            get { return String.Format("{0} value", ValueType.Name); }
        }

        public string UsageText
        {
            get { return String.Format("{0} value", ValueType.Name); }
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

        public Uri ReferenceUri
        {
            get
            {
                return new Uri(String.Format(@"http://msdn.microsoft.com/library/{0}.aspx", ValueType.FullName));
            }
        }

        public object Handle(IList<string> args, bool execute)
        {
            var stringRepresentation = args.PopHead();

            var parse = ValueType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            if (parse != null)
            {
                return parse.Invoke(null, new object[] { stringRepresentation });
            }

            var ctor = ValueType.GetConstructor(new Type[] { typeof(string) });
            if (ctor != null)
            {
                return ctor.Invoke(new object[] { stringRepresentation });
            }

            throw new InvalidOperationException();
        }


        public IEnumerable<ExampleAttribute> Examples
        {
            get { return Enumerable.Empty<ExampleAttribute>(); }
        }
    }
}
