using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.CommandLine
{
    [Obsolete("Use the [ArgumentHandler] attribute instead.")]
    public interface IArgumentHandler
    {
        void ProcessArguments(string[] args);
    }
}
