using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sidi.Util;

namespace Sidi.IO.Long.Extensions
{
    public static class PathExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Path Long(this string x)
        {
            return new Path(x);
        }
    }
}
