// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Reflection;
using System.ComponentModel;
using Sidi.Util;
using System.IO;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class Action : IParserItem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Action(object application, MethodInfo method)
        {
            Application = application;
            MethodInfo = method;
        }

        public MethodInfo MethodInfo { private set; get; }
        public object Application { private set; get; }
        public string Name { get { return MethodInfo.Name; } }
        public string Usage { get { return Sidi.CommandLine.Usage.Get(MethodInfo); } }

        public IEnumerable<string> Categories
        {
            get
            {
                var c = MethodInfo.GetCustomAttributes(typeof(CategoryAttribute), true)
                    .Select(x => ((CategoryAttribute)x).Category);
                return c.Any() ? c : new string[] { String.Empty };
            }
        }

        public string Syntax
        {
            get
            {
                MethodInfo i = MethodInfo;
                string parameters = i.GetParameters()
                    .Select(pi => FormatParameter(pi))
                    .Join(" ");
                return String.Format("{0} {1}", i.Name, parameters);
            }
        }

        static string FormatParameter(ParameterInfo pi)
        {
            if (pi.ParameterType.IsArray)
            {
                return String.Format("[{0}: list of {1}, termintated by ';']", pi.Name, pi.ParameterType.GetElementType().GetInfo());
            }
            else
            {
                return String.Format("[{0}: {1}]", pi.Name, pi.ParameterType.GetInfo());
            }
        }

        /// <summary>
        /// Formatted usage information with parameters
        /// </summary>
        public string UsageText
        {
            get
            {
                string parameters = MethodInfo.GetParameters().Select(pi => FormatParameter(pi)).Join(" ");

                return String.Format("{0} {1}\r\n{2}",
                    Name,
                    parameters,
                    Usage.Indent("  "));
            }
        }

        public void PrintScriptFileSample(TextWriter w)
        {
            w.Write("# "); w.WriteLine(Usage);
            w.Write("# "); w.WriteLine(Syntax);
            w.WriteLine();
        }

        object GetParameter(ParameterInfo parameter, IList<string> list)
        {
            var type = parameter.ParameterType;

            try
            {
                if (type.IsArray)
                {
                    var elements = new List<object>();
                    for (; list.Any(); )
                    {
                        if (list.First().Equals(Parser.ListTerminator))
                        {
                            list.RemoveAt(0);
                            break;
                        }
                        else
                        {
                            elements.Add(Parser.ParseValue(list.First(), type.GetElementType()));
                            list.RemoveAt(0);
                        }
                    }

                    var a = Array.CreateInstance(type.GetElementType(), elements.Count);
                    foreach (var i in elements.Counted())
                    {
                        a.SetValue(i.Value, i.Key);
                    }
                    return a;
                }
                else
                {
                    var r = Parser.ParseValue(list.First(), type);
                    list.RemoveAt(0);
                    return r;
                }
            }
            catch
            {
                throw new InvalidParameterException(parameter);
            }
        }

        public object Handle(IList<string> args, bool execute)
        {
            var parameters = MethodInfo.GetParameters();
            object[] parameterValues;

            try
            {
                parameterValues = parameters.Select(p => GetParameter(p, args)).ToArray();
            }
            catch (InvalidParameterException ipe)
            {
                var parameter = ipe.Parameter;
                if (args.Any())
                {
                    throw new CommandLineException("Parameter {0} could not be read from argument list {1}\r\n{2}".F(
                        FormatParameter(parameter),
                        args.Select(i => i.Quote()).Join(" "),
                        this.UsageText
                        ));
                }
                else
                {
                    throw new CommandLineException("Parameter {0} is missing.\r\n{1}".F(
                        FormatParameter(parameter), this.UsageText));
                }
            }

            log.InfoFormat("Action {0}({1})", Name, parameterValues.Join(", "));
            if (execute)
            {
                return MethodInfo.Invoke(Application, parameterValues);
            }
            else
            {
                return null;
            }
        }
    }

}
