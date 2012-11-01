using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Visualization
{
    public static class TreeMapExtensions
    {
        public static TypedTreeMap<T> CreateTreeMap<T>(this IEnumerable<T> items)
        {
            return new TypedTreeMap<T>()
            {
                Items = items.ToList()
            };
        }
    }
}
