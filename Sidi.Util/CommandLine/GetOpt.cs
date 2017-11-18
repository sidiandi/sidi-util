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
            o.modules.Add(new Logging(o));
            o.modules.Add(new GetOptInternal.ShowHelp(o));
            return o.Run(args);
        }

        static IEnumerable<GetOptOption> GetOptions(object module)
        {
            return module.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(m => GetOptOption.Create(module, m))
                .Where(_ => _ != null)
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

        internal static bool HandleLongOption(Args args, IEnumerable<GetOptOption> options)
        {
            var a = args.Current;
            var prefix = "--";
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

            var option = options.FirstOrDefault(_ => _.LongOption.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (option == null)
            {
                throw new CommandLineException(String.Format("unknown option: {0}", a));
            }

            args.MoveNext();

            Invoke(option, args);
            return true;
        }

        internal static bool HandleOption(Args args, IEnumerable<GetOptOption> options)
        {
            var a = args.Current;
            var prefix = "-";
            if (!a.StartsWith(prefix))
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
                throw new CommandLineException(String.Format("unknown option: {0}", a));
            }

            // Multiple options may follow a hyphen delimiter in a single token if the options do not take arguments. Thus, ‘-abc’ is equivalent to ‘-a -b -c’. 
            if (!GetParameterTypes(option).Any() && args.InlineParameter != null)
            {
                args.args[args.i] = prefix + args.InlineParameter;
                args.InlineParameter = null;
            }
            else
            {
                args.MoveNext();
            }

            Invoke(option, args);
            return true;
        }

        internal class Args : IEnumerator<string>
        {
            public Args(string[] args)
            {
                this.args = args;
                this.i = 0;
            }
            public readonly string[] args;
            public int i;

            public string Current { get { return args[i]; } }

            object IEnumerator.Current => args[i];

            public void Dispose()
            {
            }

            public bool HasNext => i < args.Length;

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
                i = 0;
            }
        }

        static IEnumerable<string> GetParameters(Args args)
        {
            if (args.InlineParameter != null)
            {
                yield return args.InlineParameter;
                args.InlineParameter = null;
            }
            for (;args.HasNext;)
            {
                if (!args.Current.StartsWith("-") || args.Current.Equals("-"))
                {
                    var a = args.Current;
                    args.MoveNext();
                    yield return a;
                }
                else
                {
                    break;
                }
            }
            for (; ; )
            {
                yield return null;
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

        static internal void Invoke(GetOptOption option, Args args)
        {
            var method = option.memberInfo as MethodInfo;
            if (method != null)
            {
                var p = GetParameters(args).Zip(method.GetParameters(), ParseParameter).ToArray();
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
                    p = GetParameters(args).Zip(new[] { property.PropertyType }, ParseParameter).First();
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

        public int Run(string[] argsArray)
        {
            try
            {
                var args = new Args(argsArray);
                var argList = new List<string>();

                for (; args.HasNext;)
                {
                    if (HandleLongOption(args, Options)) continue;
                    if (HandleOption(args, Options)) continue;
                    argList.Add(args.Current);
                    args.MoveNext();
                }

                var argumentHandler = MainModule as Sidi.CommandLine.IArgumentHandler;
                if (argumentHandler == null)
                {
                    throw new CommandLineException("{0} must implement IArgumentHandler to handle command line arguments.");
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
