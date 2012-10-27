using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Cache
{
    public interface ICache<Key, Value>
    {
        bool Contains(Key key);
        void Clear();
        void Reset(Key key);
        Value this[Key key] { get; }
    }
}
