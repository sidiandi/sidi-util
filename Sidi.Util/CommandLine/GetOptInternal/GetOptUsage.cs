using Sidi.Extensions;
using System;
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
            PrintHelp(Console.Out);
            System.Environment.Exit(0);
        }

        public void PrintHelp2Column(TextWriter w)
        {
            w.WriteLine("Usage: {0} [OPTION]", o.modules.First().GetType().Name);
            w.WriteLine(Usage.Get(o.modules.First().GetType()));
            w.WriteLine();

            foreach (var option in o.Options)
            {
                PrintColumns(w, new[] { 40, 40 }, new[] { Syntax(o, option), option.usage });
            }
        }

        public void PrintHelp(TextWriter w)
        {
            w.WriteLine("Usage: {0} [OPTION]", o.modules.First().GetType().Name);
            w.WriteLine(Usage.Get(o.modules.First().GetType()));
            w.WriteLine();

            foreach (var option in o.Options)
            {
                w.WriteLine("{0} : {1}", Syntax(o, option), option.usage);
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
                if (option.ShortOption != null)
                {
                    if (needSep) { w.Write(", "); }
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
                    if (needSep) { w.Write(", "); }
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
            w.WriteLine("{0} {1}", mainModule.GetType().Name, mainModule.GetType().Assembly.GetName().Version);
        }

        [Usage("Show version information and exit.")]
        public void Version()
        {
            PrintVersion(Console.Out);
            System.Environment.Exit(0);
        }
    }
}