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

        public Action(Parser parser, ItemSource source, MethodInfo method)
        {
            Source = source;
            MethodInfo = method;
            this.parser = parser;
            foreach (var i in Parameters)
            {
            }
        }

        Parser parser;
        public MethodInfo MethodInfo { private set; get; }
        public ItemSource Source { private set; get; }
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

        public class Parameter
        {
            public IValueParser ValueParser;
            public System.Reflection.ParameterInfo ParameterInfo;
        }

        public Parameter[] Parameters
        {
            get
            {
                return MethodInfo.GetParameters().Select(x => new Parameter()
                {
                    ValueParser = parser.GetValueParser(x.ParameterType),
                    ParameterInfo = x,
                }).ToArray();
            }
        }

        public string Syntax
        {
            get
            {
                string parameters = Parameters
                    .Select(pi => pi.ValueParser.Syntax)
                    .Join(" ");
                return String.Format("{0} {1}", Name, parameters);
            }
        }

        static string FormatParameter(ParameterInfo pi)
        {
            if (pi.ParameterType.IsArray)
            {
                return String.Format("[{0}: '[' {1} {1} ... ']' ]", pi.Name, pi.ParameterType.GetElementType().GetInfo());
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

        public object Handle(IList<string> args, bool execute)
        {
            var parameterValues = MethodInfo.GetParameters()
                .Select(x =>
                    {
                        log.DebugFormat("Read parameter {0} from {1}", x, args.Join(" "));
                        object value = null;

                        var emptyArgs = !args.Any();

                        if (emptyArgs && x.DefaultValue != null)
                        {
                            value = x.DefaultValue;
                            log.DebugFormat("Parameter {0} = {1} (default value)", x, value);
                            return value;
                        }

                        try
                        {
                            value = parser.ParseValue(args, x.ParameterType);
                        }
                        catch (Exception ex)
                        {
                            if (emptyArgs)
                            {
                                throw new CommandLineException("Error in command\r\n{0}\r\nParameter {1} is missing.".F(this.UsageText, x, args.Join(" ")), ex);
                            }
                            else
                            {
                                throw new CommandLineException("Error in command\r\n{0}\r\nCannot read parameter {1} from arguments {2}".F(this.UsageText, x, args.Join(" ")), ex);
                            }
                        }
                        log.DebugFormat("Parameter {0} = {1}", x, value);
                        return value;
                    })
                .ToArray();

            using (new LogScope(log.Info, "Action {0}({1})", Name, parameterValues.Join(", ")))
            {
                if (execute)
                {
                    return MethodInfo.Invoke(Source.Instance, parameterValues);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
