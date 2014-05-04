using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sidi.Extensions
{
    public static class AssemblyExtensions
    {
        public static T GetCustomAttribute<T>(this Assembly a)
        {
            return (T) a.GetCustomAttributes(typeof(T), false).First();
        }
    }
}
