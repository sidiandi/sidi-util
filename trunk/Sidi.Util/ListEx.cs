using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Util
{
    public static class ListEx
    {
        public static bool Pop<T>(this IList<T> list, out T result)
        {
            if (list.Count == 0)
            {
                result = default(T);
                return false;
            }
            else
            {
                result = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return true;
            }
        }
    }
}
