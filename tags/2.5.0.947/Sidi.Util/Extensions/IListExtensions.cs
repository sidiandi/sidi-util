using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Extensions
{
    public static class IListExtensions
    {
        public static T Pop<T>(this IList<T> list)
        {
            var last = list.Count-1;
            var r = list[last];
            list.RemoveAt(last);
            return r;
        }
    }
}
