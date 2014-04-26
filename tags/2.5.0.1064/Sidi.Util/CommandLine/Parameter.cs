using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.CommandLine
{
    public class Parameter
    {
        public IValueParser ValueParser;
        public System.Reflection.ParameterInfo ParameterInfo;
    }
}
