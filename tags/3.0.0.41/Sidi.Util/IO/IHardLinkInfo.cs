using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public interface IHardLinkInfo
    {
        int FileLinkCount { get; }
        long FileIndex { get; }
        IList<LPath> HardLinks { get; }
    }
}
