using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sidi.CommandLine
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=true)]
    public class ExampleAttribute : System.Attribute
    {
        public string Value { private set; get; }

        public ExampleAttribute(string stringRepresentation)
        {
            this.Value = stringRepresentation;
        }

        public static IEnumerable<ExampleAttribute> Get(ICustomAttributeProvider cap)
        {
            return cap.GetCustomAttributes(typeof(ExampleAttribute), true).Cast<ExampleAttribute>();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
