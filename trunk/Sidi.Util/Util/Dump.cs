using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Util
{
    public class Dump
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Write(object x, TextWriter w)
        {
            seen = new HashSet<object>();
            RenderValue(x, w, 0);
        }

        HashSet<object> seen;

        /// <summary>
        /// Maximal tree levels to be output
        /// </summary>
        public int MaxLevel = Int32.MaxValue;

        void WriteTree(object x, TextWriter w, int level)
        {
            var t = x.GetType();
            foreach (var p in t.GetProperties())
            {
                if (p.GetIndexParameters().Any())
                {
                    continue;
                }
                w.Write("{0}: ", p.Name);
                RenderValue(p.GetValue(x, new object[] { }), new IndentWriter(w, Indent, false), level);
            }
        }

        void RenderValue(object value, TextWriter w, int level)
        {
            if (value == null)
            {
                w.WriteLine("null");
                return;
            }

            try
            {
                if (HasAcceptableToStringMethod(value.GetType()))
                {
                    w.WriteLine(value.ToString());
                }
                else
                {
                    w.WriteLine(value.ToString());

                    if (seen.Contains(value))
                    {
                        return;
                    }
                    seen.Add(value);

                    if (level > MaxLevel)
                    {
                        return;
                    }

                    WriteTree(value, w, level + 1);
                    var en = value as System.Collections.IEnumerable;
                    if (en != null)
                    {
                        foreach (var i in en.Cast<object>().Counted())
                        {
                            w.Write("{0}: ", i.Key);
                            RenderValue(i.Value, new IndentWriter(w, Indent, false), level + 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                w.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Prefix for indentation
        /// </summary>
        public string Indent = "  ";

        public static HashSet<Type> acceptableTypes = new HashSet<Type>(new[]
        {
            typeof(System.String),
            typeof(Sidi.IO.Long.Path)
        });

        static bool HasAcceptableToStringMethod(Type t)
        {
            if (acceptableTypes.Contains(t))
            {
                return true;
            }

            var m = t.GetMethod("ToString", new Type[] { });
            return m.DeclaringType != typeof(Object) && t.IsValueType;
        }
    }
}
