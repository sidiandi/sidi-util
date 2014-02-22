using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Reflection;

namespace Sidi.Extensions
{
    public static class TypeExtensions
    {
        public static T GetAssemblyAttribute<T>(this Type t)
        {
            var a = t.Assembly;
            try
            {
                return ((T)a.GetCustomAttributes(typeof(T), false).First());
            }
            catch (Exception)
            {
                throw new Exception(String.Format("{0} is not defined for assembly {1}", typeof(T).ToString(), a));
            }
        }
    }
}
