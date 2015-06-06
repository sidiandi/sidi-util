// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sidi.Util;
using Sidi.Extensions;
using System.Reflection;
using System.Collections;
using System.Windows;
using System.Linq.Expressions;

namespace Sidi.Util
{
    public class Dumper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dumper Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new Dumper();
                }
                return s_instance;
            }
            set
            {
                s_instance = value;
            }
        }
        static Dumper s_instance;

        public string ToString(object x)
        {
            using (var w = new StringWriter())
            {
                Write(w, x);
                return w.ToString();
            }
        }

        public string ToString(Expression<Func<object>> getObjectToDump)
        {
            using (var w = new StringWriter())
            {
                Write(w, getObjectToDump);
                return w.ToString();
            }
        }

        public void Write(TextWriter output, object x)
        {
            if (output == null)
            {
                output = Console.Out;
            }

            seen = new HashSet<object>();
            RenderValue(x, output, 0);
        }

        public void Write(TextWriter w, Expression<Func<object>> getObjectToDump)
        {
            var name = Dumper.GuessVariableName(getObjectToDump);
            w.Write("{0} = ", name);
            var objectToDump = getObjectToDump.Compile()();
            Write(w, objectToDump);
        }

        HashSet<object> seen;

        /// <summary>
        /// Maximal tree levels to be output
        /// </summary>
        public int MaxLevel = 1;

        public int MaxEnumElements = 100;

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
                try
                {
                    var value = p.GetValue(x, new object[] { });
                    RenderValue(value, new IndentWriter(w, Indent, false), level);
                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException)
                    {
                        ex = ex.InnerException;
                    }
                    w.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
                }
            }
        }

        static object GetValue(PropertyInfo p, object x, object[] arguments)
        {
            try
            {
                return p.GetValue(x, arguments);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        void RenderValue(object value, TextWriter w, int level)
        {
            if (value == null)
            {
                w.WriteLine("(null)");
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

                    if (level >= MaxLevel)
                    {
                        return;
                    }

                    WriteTree(value, w, level + 1);

                    if (value is IEnumerable<byte>)
                    {
                        var e = (IEnumerable<byte>)value;
                        HexDump.Write(e.Take(1024), w);
                    }
                    else if (value is IDictionary)
                    {
                        var dictionary = (IDictionary)value;
                        foreach (var key in dictionary.Keys.Cast<object>().OrderBy(_ => _))
                        {
                            w.Write("[{0}] ", key);
                            RenderValue(dictionary[key], new IndentWriter(w, Indent, false), level + 1);
                        }
                    }
                    else if (value is System.Collections.IEnumerable)
                    {
                        var en = (IEnumerable) value;
                        foreach (var i in en.Cast<object>().Counted().Take(MaxEnumElements))
                        {
                            w.Write("[{0}] ", i.Key);
                            RenderValue(i.Value, new IndentWriter(w, Indent, false), level + 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                w.WriteLine("[{0}]", ex.GetType());
            }
        }

        /// <summary>
        /// Prefix for indentation
        /// </summary>
        public string Indent = "  ";

        static HashSet<Type> acceptableTypes = new HashSet<Type>(new[]
        {
            typeof(System.String),
            typeof(Sidi.IO.LPath)
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

        public static string GuessVariableName(System.Linq.Expressions.Expression e)
        {
            if (e is LambdaExpression)
            {
                e = ((LambdaExpression)e).Body;
            }

            if (e is MemberExpression)
            {
                var m = ((MemberExpression)e).Member;
                return m.Name;
            }
            else if (e is UnaryExpression)
            {
                return GuessVariableName(((UnaryExpression)e).Operand);
            }
            else
            {
                return e.ToString();
            }
        }
    }
}
