using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public interface ITree
    {
        ITree Parent { get; }
        IEnumerable<ITree> Children { get; }
    }
}
