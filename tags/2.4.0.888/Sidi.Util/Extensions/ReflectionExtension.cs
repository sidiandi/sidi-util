using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sidi.Extensions
{
    public static class ReflectionExtension
    {
        /// <summary>
        /// Returns the first custom attribute of type attrivute on memberInfo
        /// </summary>
        /// <typeparam name="Attribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <returns>null, if no such attribute is on the memberInfo</returns>
        public static Attribute GetCustomAttribute<Attribute>(this System.Reflection.MemberInfo memberInfo) where Attribute : class
        {
            return memberInfo.GetCustomAttributes(typeof(Attribute), true).FirstOrDefault() as Attribute;
        }

        /// <summary>
        /// Returns custom attributes of type attribute on memberInfo
        /// </summary>
        /// <typeparam name="Attribute"></typeparam>
        /// <param name="memberInfo"></param>
        /// <returns>null, if no such attribute is on the memberInfo</returns>
        public static IEnumerable<Attribute> GetCustomAttributes<Attribute>(this System.Reflection.MemberInfo memberInfo) where Attribute : class
        {
            return memberInfo.GetCustomAttributes(typeof(Attribute), true).OfType<Attribute>();
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
            {
                return ((FieldInfo)memberInfo).FieldType;
            }
            else if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo)memberInfo).PropertyType;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static object GetValue(this MemberInfo memberInfo, object instance)
        {
            if (memberInfo is FieldInfo)
            {
                return ((FieldInfo)memberInfo).GetValue(instance);
            }
            else if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo)memberInfo).GetValue(instance, new object[] { });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void SetValue(this MemberInfo memberInfo, object instance, object value)
        {
            if (memberInfo is FieldInfo)
            {
                ((FieldInfo)memberInfo).SetValue(instance, value);
            }
            else if (memberInfo is PropertyInfo)
            {
                ((PropertyInfo)memberInfo).SetValue(instance, value, new object[] { });
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
