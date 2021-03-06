﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

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
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException(String.Format("{0} is not defined for assembly {1}", typeof(T).ToString(), a), ex);
            }
        }

        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
