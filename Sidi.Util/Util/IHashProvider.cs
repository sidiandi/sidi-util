using Sidi.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public interface IHashProvider
    {
        Hash Get(IFileSystemInfo file);
        Hash Get(Stream stream);
    }
}
