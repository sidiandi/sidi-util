using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Extensions
{
    public static class MethodBaseExtensions
    {
        public static string GetFullName(this MethodBase method)
        {
            return new[] { method.DeclaringType.FullName, method.Name }.Join(".");
        }
    }
}
