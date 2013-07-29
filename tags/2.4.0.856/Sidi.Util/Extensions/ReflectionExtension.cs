using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Extensions
{
    public static class ReflectionExtension
    {
        public static Attribute GetCustomAttribute<Attribute>(this System.Reflection.MemberInfo memberInfo) where Attribute : class
        {
            return memberInfo.GetCustomAttributes(typeof(Attribute), true).FirstOrDefault() as Attribute;
        }
    }
}
