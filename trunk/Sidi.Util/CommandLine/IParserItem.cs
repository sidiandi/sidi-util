using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.CommandLine
{
    public interface IParserItem
    {
        string Usage { get; }
        string UsageText { get; }
        string Name { get; }
        object Application { get; }
        IEnumerable<string> Categories { get; }
        void Handle(IList<string> args, bool execute);
    }
}
