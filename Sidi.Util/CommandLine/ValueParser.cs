using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using Sidi.Extensions;
using Sidi.Util;

namespace Sidi.CommandLine
{
    public interface IValueParser : IParserItem
    {
        Type ValueType { get; }
        IEnumerable<ExampleAttribute> Examples { get; }
    }
}
