﻿using Sidi.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sidi.CommandLine.GetOptInternal
{
    internal class ShowHelp
    {
        readonly private GetOpt o;

        public ShowHelp(GetOpt o)
        {
            this.o = o;
        }

        [Usage("Show this message and exit.")]
        public void Help()
        {
            using (var w = new StringWriter())
            {
                PrintHelp(w);
                o.ShowMessage(w);
            }
            System.Environment.Exit(0);
        }

        static string GetIArgumentHandlerUsage(GetOpt o)
        {
            if (o.MainModule is IArgumentHandler)
            {
                var processArguments = o.MainModule.GetType().GetMethod("ProcessArguments", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string[]) }, null);
                var u = processArguments.GetCustomAttribute<Usage>();
                if (u == null)
                {
                    return "[argument]...";
                }
                return u.Description;
            }
            else
            {
                return String.Empty;
            }
        }

        public void PrintHelp(TextWriter w)
        {
            w.WriteLine("Usage: " + NotEmpty
            (
                o.ProgramName,
                "[option]...",
                this.o.Commands.Any() ? "<command> " : null,
                GetIArgumentHandlerUsage(this.o)
            ).Join(" "));
            w.WriteLine(Usage.Get(o.MainModule.GetType()));

            if (o.Options.Any())
            {
                w.WriteLine();
                w.WriteLine("Options:");
                foreach (var option in o.Options)
                {
                    w.WriteLine("{0} : {1}", Syntax(o, option), option.usage);
                }
            }

            if (o.Commands.Any())
            {
                w.WriteLine();
                w.WriteLine("Commands:");
                foreach (var command in o.Commands)
                {
                    w.WriteLine("{0} : {1}", Syntax(o, command), command.usage);
                }
            }
        }

        static string Syntax(GetOpt o, GetOptOption option)
        {
            if (GetOpt.GetNamedParameters(option).Count() > 1)
            {
                return SyntaxForMethodsWithMultipleParameters(o, option);
            }

            using (var w = new StringWriter())
            {
                bool needSep = false;
                const string sep = " | ";

                if (option.ShortOption != null)
                {
                    if (needSep) { w.Write(sep); }
                    w.Write(o.shortOptionPrefix);
                    w.Write(option.ShortOption);
                    var p = GetOpt.GetParameterTypes(option);
                    if (p.Any())
                    {
                        w.Write(p.Select(Syntax).Join(" "));
                    }
                    needSep = true;
                }
                if (option.LongOption != null)
                {
                    if (needSep) { w.Write(sep); }
                    w.Write(o.longOptionPrefix);
                    w.Write(option.LongOption);
                    var p = GetOpt.GetParameterTypes(option);
                    if (p.Any())
                    {
                        w.Write("=");
                        w.Write(p.Select(Syntax).Join(" "));
                    }
                    needSep = true;
                }
                return w.ToString();
            }
        }

        static IEnumerable<string> NotEmpty(params string[] p)
        {
            return p.Where(_ => !String.IsNullOrEmpty(_));
        }

        static string Syntax(GetOpt o, Command command)
        {
            return NotEmpty(command.ShortOption, command.LongOption).Join(" | ");
        }

        static string SyntaxForMethodsWithMultipleParameters(GetOpt o, GetOptOption option)
        {
            using (var w = new StringWriter())
            {
                bool needSep = false;
                if (option.ShortOption != null)
                {
                    if (needSep) { w.Write(", "); }
                    w.Write(o.shortOptionPrefix);
                    w.Write(option.ShortOption);
                    var p = GetOpt.GetNamedParameters(option);
                    w.Write(" ");
                    w.Write(p.Select(Syntax).Join(" "));
                    needSep = true;
                }
                if (option.LongOption != null)
                {
                    if (needSep) { w.Write(", "); }
                    w.Write(o.longOptionPrefix);
                    w.Write(option.LongOption);
                    var p = GetOpt.GetNamedParameters(option);
                    if (p.Any())
                    {
                        w.Write("");
                        w.Write(p.Select(Syntax).Join(" "));
                    }
                    needSep = true;
                }
                return w.ToString();
            }
        }

        static string Syntax(Type type)
        {
            return String.Format("<{0}>", type.Name);
        }

        static string Syntax(ParameterInfo parameterInfo)
        {
            return String.Format("<{0}: {1}>", parameterInfo.Name, parameterInfo.ParameterType.Name);
        }

        static void PrintColumns(TextWriter w, int[] width, string[] text)
        {
            for (int row = 0; ;++row )
            {
                for (int c = 0; c < width.Length; ++c)
                {
                    if (row*width[c] < text[c].Length)
                    {
                        goto print;
                    }
                }
                return;
print:

                for (int c = 0; c < width.Length; ++c)
                {
                    w.Write(GetPadddedSubString(text[c], row * width[c], width[c]));
                }
                w.WriteLine();
            }
        }

        static string GetPadddedSubString(string text, int offset, int length)
        {
            if (offset + length <= text.Length)
            {
                return text.Substring(offset, length);
            }

            if (offset < text.Length)
            {
                return text.Substring(offset) + new String(' ', length - (text.Length - offset));
            }

            return new String(' ', length);
        }

        internal void PrintVersion(TextWriter w)
        {
            var mainModule = o.MainModule;
            w.WriteLine("{0} {1}", this.o.ProgramName, mainModule.GetType().Assembly.GetName().Version);
        }

        [Usage("Show version information and exit.")]
        public void Version()
        {
            using (var w = new StringWriter())
            {
                PrintVersion(w);
                this.o.ShowMessage(w);
            }
            System.Environment.Exit(0);
        }
    }
}