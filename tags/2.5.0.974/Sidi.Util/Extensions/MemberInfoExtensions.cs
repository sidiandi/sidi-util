using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sidi.Extensions
{
    public static class MemberInfoExtensions
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

        /// <summary>
        /// Set the value of a field or property
        /// </summary>
        /// <param name="member"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetValue(this MemberInfo member, object target, object value)
        {
            if (member is FieldInfo)
            {
                ((FieldInfo)member).SetValue(target, value);
            }
            else if (member is PropertyInfo)
            {
                ((PropertyInfo)member).SetValue(target, value, new object[] { });
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Get the type of a field or property
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is FieldInfo)
            {
                return ((FieldInfo)member).FieldType;
            }
            else if (member is PropertyInfo)
            {
                return ((PropertyInfo)member).PropertyType;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Get the value of a field or property
        /// </summary>
        /// <param name="member"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static object GetValue(this MemberInfo member, object item)
        {
            if (member is FieldInfo)
            {
                return ((FieldInfo)member).GetValue(item);
            }
            else if (member is PropertyInfo)
            {
                return ((PropertyInfo)member).GetValue(item, new object[] { });
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
