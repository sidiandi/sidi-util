using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.CommandLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class ValueParserAttribute : System.Attribute 
    {
        public ValueParserAttribute(Type type)
        {
            ValueParserType = type;
        }

        public Type ValueParserType { get; private set; }
    }
}
