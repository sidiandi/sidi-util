using Sidi.CommandLine.GetOptInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Layout;
using log4net.Appender;
using log4net.Core;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    /// <summary>
    /// Implements the GNU Program Argument Syntax Conventions
    /// https://www.gnu.org/software/libc/manual/html_node/Argument-Syntax.html#Argument-Syntax
    /// </summary>
    public class GetOpt
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly IList<object> modules;

        public static void ConfigureDefaultLogging()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            if (hierarchy.Configured)
            {
                return;
            }

            var pattern = new PatternLayout
            {
                ConversionPattern = "%utcdate{ISO8601} [%thread] %level %logger %ndc - %message%newline",
            };
            pattern.ActivateOptions();

            var ca = new ConsoleAppender
            {
                Target = "Console.Out",
                Name ="getopt",
                Layout = pattern,
                Threshold = Level.All
            };

            hierarchy.Root.AddAppender(ca);
            hierarchy.Configured = true;
        }

        Level[] levels = new[] { Level.Off, Level.Error, Level.Warn, Level.Info, Level.All };
        int verbosity = 0;

        internal void Verbose()
        {
            verbosity = Math.Min(levels.Length - 1, verbosity + 1);

            ConfigureDefaultLogging();
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Level = levels[verbosity];
            log.InfoFormat("Log level: {0}", hierarchy.Root.Level);
        }

        public GetOpt()
        {
            this.modules = new List<object>();
        }

        public static int Run(object module, string[] args)
        {
            var o = new GetOpt();
            o.modules.Add(module);
            o.AddDefaultModules();
            return o.Run(args);
        }

        /// <summary>
        /// Adds default modules for logging and help
        /// </summary>
        public void AddDefaultModules()
        {
            modules.Add(new Logging(this));
            modules.Add(new GetOptInternal.ShowHelp(this));
        }

        static IEnumerable<GetOptOption> GetOptions(object module)
        {
            return module.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(m => GetOptOption.Create(module, m))
                .Where(_ => _ != null)
                .Where(_ => !_.memberInfo.Name.Equals("ProcessArguments"))
                .ToList();
        }
        internal static IEnumerable<GetOptOption> GetOptions(IEnumerable<object> modules)
        {
            var options = modules.SelectMany(GetOptions).ToList();
            AddShortOptions(options);
            return options;
        }

        static void AddShortOptions(IEnumerable<GetOptOption> options)
        {
            var used = new HashSet<string>();
            foreach (var i in options)
            {
                var shortName = i.LongOption.Substring(0, 1).ToLower();
                if (!used.Contains(shortName))
                {
                    i.ShortOption = shortName;
                    used.Add(shortName);
                }
            }
        }

        internal static bool IsAbbreviationFor(string abbreviation, string candidate)
        {
            return IsAbbreviationFor(abbreviation, candidate, 0, 0);
        }

        static bool IsAbbreviationFor(string a, string b, int ia, int ib)
        {
            if (ia >= a.Length)
            {
                return true;
            }

            if (ib >= b.Length)
            {
                return false;
            }

            if (Char.ToLower(a[ia]) == Char.ToLower(b[ib]))
            {
                for (int i = ib + 1; i <= b.Length; ++i)
                {
                    if (IsAbbreviationFor(a, b, ia + 1, i))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            { 
                return false;
            }
        }

        static GetOptOption FindLongOption(string name, IEnumerable<GetOptOption> options)
        {
            // exact match?
            var o = options.FirstOrDefault(_ => _.LongOption.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (o != null) return o;

            var abbreviationMatches = options.Where(_ => IsAbbreviationFor(name, _.LongOption)).ToList();

            if (abbreviationMatches.Count() == 1)
            {
                return abbreviationMatches.First();
            }

            if (abbreviationMatches.Count() == 0)
            {
                throw new CommandLineException(String.Format("parameter {0} is unknown", name));
            }

            throw new CommandLineException(String.Format("parameter {0} is not unique. Could be {1}", name, abbreviationMatches.Join(", ")));
        }

        internal static bool HandleLongOption(Args args, IEnumerable<GetOptOption> options, string prefix)
        {
            if (!args.HasNext)
            {
                return false;
            }

            var a = args.Next;
            if (!a.StartsWith(prefix))
            {
                return false;
            }

            var p = a.Split(new[] { '=' }, 2);
            var name = p[0].Substring(prefix.Length);
            if (p.Length == 2)
            {
                args.InlineParameter = p[1];
            }

            var option = FindLongOption(name, options);

            if (option == null)
            {
                throw new CommandLineException(String.Format("unknown option: {0}", a));
            }

            args.MoveNext();

            Invoke(option, args);
            return true;
        }

        internal static void HandleInlineOption(Args args, IEnumerable<GetOptOption> options)
        {
            if (args.InlineParameter == null)
            {
                throw new ArgumentOutOfRangeException("args");
            }

            var a = args.InlineParameter;
            args.InlineParameter = null;

            var name = a.Substring(0, 1);
            args.InlineParameter = a.Substring(1);
            if (String.IsNullOrEmpty(args.InlineParameter))
            {
                args.InlineParameter = null;
            }

            var option = options.FirstOrDefault(_ => string.Equals(_.ShortOption, name, StringComparison.InvariantCultureIgnoreCase));

            if (option == null)
            {
                throw new CommandLineException(String.Format("unknown option: {0}", args.Current));
            }

            Invoke(option, args);

            if (args.InlineParameter != null)
            {
                if (GetParameterTypes(option).Any())
                {
                    throw new CommandLineException("inline parameter not consumed.");
                }
                else
                {
                    HandleInlineOption(args, options);
                }
            }
        }

        internal static bool HandleOption(Args args, IEnumerable<GetOptOption> options, string prefix)
        {
            if (!args.HasNext)
            {
                return false;
            }

            var a = args.Next;

            if (!a.StartsWith(prefix) || a.Length == 1)
            {
                return false;
            }

            var name = a.Substring(prefix.Length, 1);
            args.InlineParameter = a.Substring(prefix.Length + 1);
            if (String.IsNullOrEmpty(args.InlineParameter))
            {
                args.InlineParameter = null;
            }

            var option = options.FirstOrDefault(_ => string.Equals(_.ShortOption, name, StringComparison.InvariantCultureIgnoreCase));

            if (option == null)
            {
                throw new CommandLineException(String.Format("unknown option: {0}", args.Current));
            }

            args.MoveNext();

            Invoke(option, args);

            if (args.InlineParameter != null)
            {
                if (GetParameterTypes(option).Any())
                {
                    throw new CommandLineException("inline parameter not consumed.");
                }
                else
                {
                    HandleInlineOption(args, options);
                    return true;
                }
            }

            return true;
        }

        internal class Args : IEnumerator<string>
        {
            public Args(string[] args)
            {
                this.args = args;
                this.i = -1;
            }
            public readonly string[] args;
            public int i;

            public string Current { get { return args[i]; } }

            public string Next { get { return args[i+1]; } }

            object IEnumerator.Current => args[i];

            public void Dispose()
            {
            }

            public bool HasNext => i < args.Length-1;

            public string InlineParameter { get; set; }

            public bool MoveNext()
            {
                if (HasNext)
                {
                    ++i;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                i = -1;
            }
        }

        internal static object ParseParameter(string value, ParameterInfo parameter)
        {
            if (value == null && parameter.IsOptional)
            {
                return null;
            }
            return ParseParameter(value, parameter.ParameterType);
        }

        internal static object ParseParameter(string value, Type type)
        {
            return Parser.ParseValueBuiltIn(value, type);
        }

        internal static string GetParameter(Args args)
        {
            if (args.InlineParameter != null)
            {
                var p = args.InlineParameter;
                args.InlineParameter = null;
                return p;
            }

            return args.MoveNext() ? args.Current : null;
        }

        internal static object[] GetParameterValues(Args args, ParameterInfo[] parameter)
        {
            return parameter.Select(pi => ParseParameter(GetParameter(args), pi)).ToArray();
        }

        internal static object[] GetParameterValues(Args args, Type[] parameter)
        {
            return parameter.Select(pi => ParseParameter(GetParameter(args), pi)).ToArray();
        }

        static internal void Invoke(GetOptOption option, Args args)
        {
            var method = option.memberInfo as MethodInfo;
            if (method != null)
            {
                var p = GetParameterValues(args, method.GetParameters());
                log.DebugFormat("invoke option {0} with arguments {1}", option, p.Join(", "));
                method.Invoke(option.instance, p);
            }

            var property = option.memberInfo as PropertyInfo;
            if (property != null)
            {

                object p;
                if (property.PropertyType.Equals(typeof(bool)))
                {
                    p = true;
                }
                else
                {
                    p = GetParameterValues(args, new[] { property.PropertyType })[0];
                }
                log.DebugFormat("invoke option {0} with argument {1}", option, p);
                property.SetValue(option.instance, p);
            }
        }

        static internal IEnumerable<ParameterInfo> GetNamedParameters(GetOptOption option)
        {
            var method = option.memberInfo as MethodInfo;
            return method == null ? Enumerable.Empty<ParameterInfo>() : method.GetParameters();
        }

        internal static Type[] GetParameterTypes(GetOptOption option)
        {
            var p = GetParameterTypesInternal(option);
            if (p.Length == 1 && p[0].Equals(typeof(Boolean)))
            {
                return new Type[] { };
            }
            else
            {
                return p;
            }
        }

        static Type[] GetParameterTypesInternal(GetOptOption option)
        {
            var method = option.memberInfo as MethodInfo;
            if (method != null)
            {
                return method.GetParameters().Select(_ => _.ParameterType).ToArray();
            }

            var property = option.memberInfo as PropertyInfo;
            if (property != null)
            {
                return new[] { property.PropertyType };
            }

            var field = option.memberInfo as FieldInfo;
            if (field != null)
            {
                return new[] { field.FieldType };
            }

            throw new ArgumentOutOfRangeException("option");
        }

        IEnumerable<GetOptOption> _options;
        public readonly string shortOptionPrefix = "-";
        public readonly string longOptionPrefix = "--";

        internal IEnumerable<GetOptOption> Options
        {
            get
            {
                if (_options == null)
                {
                    _options = GetOptions(modules);
                }
                return _options;
            }
        }

        public object MainModule { get { return modules.First(); } }

        static bool HandleTwoDashes(Args args, IList<string> argList, string longOptionPrefix)
        {
            if (!args.HasNext || !args.Next.Equals(longOptionPrefix))
            {
                return false;
            }

            args.MoveNext();

            for (; args.MoveNext();)
            {
                argList.Add(args.Current);
            }
            return true;
        }

        static bool HandleNormalArgument(Args args, IList<string> argList)
        {
            if (!args.HasNext) return false;

            args.MoveNext();
            argList.Add(args.Current);
            return true;
        }

        public int Run(string[] argsArray)
        {
            try
            {
                var args = new Args(argsArray);
                var argList = new List<string>();

                for (; args.HasNext;)
                {
                    if (HandleTwoDashes(args, argList, longOptionPrefix)) continue;
                    if (HandleLongOption(args, Options, longOptionPrefix)) continue;
                    if (HandleOption(args, Options, shortOptionPrefix)) continue;
                    if (HandleNormalArgument(args, argList)) continue;
                }

                var argumentHandler = MainModule as Sidi.CommandLine.IArgumentHandler;
                if (argumentHandler == null)
                {
                }
                else
                {
                    log.InfoFormat("Handling arguments: {0}", argList.Join(", "));
                    argumentHandler.ProcessArguments(argList.ToArray());
                }

                return 0;
            }
            catch (Sidi.CommandLine.CommandLineException cle)
            {
                Console.WriteLine(cle.Message);
                new GetOptInternal.ShowHelp(this).PrintHelp(Console.Out);
                return -1;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return -1;
            }
        }
    }
}
