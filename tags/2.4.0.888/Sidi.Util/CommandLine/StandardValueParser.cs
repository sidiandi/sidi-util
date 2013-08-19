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
            this.type = type;

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

            throw new InvalidCastException(type.ToString() + " is not supported");
        ok: ;
        }

        Type type;

        public string Usage
        {
            get { return String.Format("{0} value", type.Name); }
        }

        public string UsageText
        {
            get { return String.Format("{0} value", type.Name); }
        }

        public string Name
        {
            get { return type.Name; }
        }

        public string Syntax
        {
            get { return type.Name; }
        }

        public Application Application
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
                return new Uri(String.Format(@"http://msdn.microsoft.com/library/{0}.aspx", type.FullName));
            }
        }

        public object Handle(IList<string> args, bool execute)
        {
            var stringRepresentation = args.PopHead();

            var parse = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            if (parse != null)
            {
                return parse.Invoke(null, new object[] { stringRepresentation });
            }

            var ctor = type.GetConstructor(new Type[] { typeof(string) });
            if (ctor != null)
            {
                return ctor.Invoke(new object[] { stringRepresentation });
            }

            throw new InvalidCastException(type.ToString() + " is not supported");
        }
    }
}
