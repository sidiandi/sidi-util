using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    internal class Prefix
    {
        public string Text;
    }

    internal class RelativePrefix : Prefix
    {
    }

    internal class RootRelativePrefix : Prefix
    {
    }

    internal class UncPrefix : Prefix
    {
        public string Server { get; set; }
        public string Share { get; set; }
    }

    internal class LocalDrivePrefix : Prefix
    {
        public string Drive { get; set; }
    }

    internal class DeviceNamespacePrefix : Prefix
    {
    }
}
