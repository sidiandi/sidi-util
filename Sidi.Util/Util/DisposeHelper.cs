using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    public static class DisposeHelper
    {
        public static void Dispose<T>(ref T x)
        {
            Dispose(x);
            x = default(T);
        }

        public static void Dispose(object x)
        {
            var disposable = x as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
