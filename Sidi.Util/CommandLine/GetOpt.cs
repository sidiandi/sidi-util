using Sidi.CommandLine.GetOptInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Layout;
using log4net.Appender;
using log4net.Core;
using Sidi.Extensions;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Option = Sidi.CommandLine.GetOptInternal.Option;

namespace Sidi.CommandLine
{
    /// <summary>
    /// Implements the GNU Program Argument Syntax Conventions
    /// https://www.gnu.org/software/libc/manual/html_node/Argument-Syntax.html#Argument-Syntax
    /// </summary>
    public partial class GetOpt
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
                Name = "getopt",
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

        internal static void AddShortOptions(IEnumerable<GetOptInternal.Option> options)
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

        static void AddShort(IEnumerable<Command> commands)
        {
            var used = new HashSet<string>();
            foreach (var i in commands)
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

        static GetOptInternal.Option FindLongOption(string name, IEnumerable<GetOptInternal.Option> options)
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

        internal static bool HandleLongOption(Args args, IEnumerable<GetOptInternal.Option> options, string prefix)
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

        internal static void HandleInlineOption(Args args, IEnumerable<GetOptInternal.Option> options)
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

        internal static bool HandleOption(Args args, IEnumerable<GetOptInternal.Option> options, string prefix)
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

        internal static bool HandleCommand(Args args, IEnumerable<Command> commands, string prefix)
        {
            if (!commands.Any())
            {
                return false;
            }

            if (!args.HasNext)
            {
                return false;
            }

            var a = args.Next;

            if (!a.StartsWith(prefix))
            {
                return false;
            }

            var name = a.Substring(prefix.Length);

            var command = FindCommand(name, commands);

            if (command == null)
            {
                throw new CommandLineException(String.Format("unknown command: {0}", args.Current));
            }

            args.MoveNext();

            command.Invoke(args);

            return true;
        }

        internal static object ProvideValue(object instance, FieldInfo field)
        { 
            var v = field.GetValue(instance);
            if (v == null)
            {
                v = Activator.CreateInstance(field.FieldType);
                field.SetValue(instance, v);
            }
            return v;
        }

        internal static object ProvideValue(object instance, PropertyInfo property)
        {
            var v = property.GetValue(instance, new object[] { });
            if (v == null)
            {
                v = Activator.CreateInstance(property.PropertyType);
                property.SetValue(instance, v, new object[] { });
            }
            return v;
        }

        private static Command FindCommand(string name, IEnumerable<Command> commands)
        {
            // exact match?
            var o = commands.FirstOrDefault(_ => _.LongOption.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (o != null) return o;

            var abbreviationMatches = commands.Where(_ => IsAbbreviationFor(name, _.LongOption)).ToList();

            if (abbreviationMatches.Count() == 1)
            {
                return abbreviationMatches.First();
            }

            if (abbreviationMatches.Count() == 0)
            {
                throw new CommandLineException(String.Format("command {0} is unknown", name));
            }

            throw new CommandLineException(String.Format("command {0} is not unique. Could be {1}", name, abbreviationMatches.Join(", ")));
        }

        internal static object ParseParameter(Args args, ParameterInfo parameter)
        {
            if (parameter.IsOptional)
            {
                var value = GetParameter(args);
                if (value == null)
                {
                    return null;
                }
                return ParseParameter(value, parameter.ParameterType);
            }

            return ParseParameter(args, parameter.ParameterType);
        }

        internal static object ParseParameter(Args args, Type type)
        {
            if (type.IsArray)
            {
                var p = new List<object>();
                for (; ;)
                {
                    var element = ParseParameter(args, type.GetElementType());
                    if (element == null)
                    {
                        break;
                    }
                    p.Add(element);
                }
                var a = Array.CreateInstance(type.GetElementType(), p.Count);
                for (int i=0; i<p.Count;++i)
                {
                    a.SetValue(p[i], i);
                }
                return a;
            }
            else
            {
                var value = GetParameter(args);
                if (value == null) return null;
                return Parser.ParseValueBuiltIn(value, type);
            }
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
            return parameter.Select(pi => ParseParameter(args, pi)).ToArray();
        }

        internal static object[] GetParameterValues(Args args, Type[] parameter)
        {
            return parameter.Select(pi => ParseParameter(args, pi)).ToArray();
        }

        static internal void Invoke(GetOptInternal.Option option, Args args)
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

        static internal IEnumerable<ParameterInfo> GetNamedParameters(GetOptInternal.Option option)
        {
            var method = option.memberInfo as MethodInfo;
            return method == null ? Enumerable.Empty<ParameterInfo>() : method.GetParameters();
        }

        internal static Type[] GetParameterTypes(GetOptInternal.Option option)
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

        static Type[] GetParameterTypesInternal(GetOptInternal.Option option)
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

        public readonly string commandPrefix = String.Empty;
        public readonly string shortOptionPrefix = "-";
        public readonly string longOptionPrefix = "--";

        internal IEnumerable<GetOptInternal.Option> Options
        {
            get
            {
                if (_options == null)
                {
                    _options = GetOptInternal.Option.Get(modules);
                    AddShortOptions(_options);
                }
                return _options;
            }
        }
        IEnumerable<GetOptInternal.Option> _options;

        internal IEnumerable<Command> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = Command.Get(modules);
                    AddShort(_commands);
                }
                return _commands;
            }
        }
        IEnumerable<Command> _commands;

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
        static bool HandleTwoDashesAsCommandEnd(Args args, IList<string> argList, string longOptionPrefix)
        {
            if (!args.HasNext || !args.Next.Equals(longOptionPrefix))
            {
                return false;
            }

            args.MoveNext();

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
            return Run(new Args(argsArray));
        }

        class MethodArgumentHandler : IArgumentHandlerWithUsage
        {
            private readonly object instance;
            private readonly MethodInfo method;

            public string Usage
            {
                get
                {
                    return GetOptInternal.ShowHelp.ArgumentSyntax(method);
                }
            }

            public MethodArgumentHandler(object instance, MethodInfo method)
            {
                this.instance = instance;
                this.method = method;
            }

            public void ProcessArguments(string[] args)
            {
                var p = GetParameterValues(new Args(args), method.GetParameters());
                log.DebugFormat("invoke argument handler {0} with arguments {1}", method, p.Join(", "));
                method.Invoke(instance, p);
            }

            internal static IArgumentHandlerWithUsage Create(object mainModule)
            {
                var method = mainModule.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(_ => _.GetCustomAttribute<ArgumentHandler>() != null);

                if (method == null) return null;

                return new MethodArgumentHandler(mainModule, method);
            }
        }

        internal interface IArgumentHandlerWithUsage : IArgumentHandler
        {
              string Usage { get; }
        }

        class ArgumentHandlerWrapper : IArgumentHandlerWithUsage
        {
            private readonly IArgumentHandler actualHandler;

            public ArgumentHandlerWrapper(IArgumentHandler actualHandler)
            {
                this.actualHandler = actualHandler;
            }

            public string Usage
            {
                get
                {
                    var processArguments = actualHandler.GetType().GetMethod("ProcessArguments", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string[]) }, null);
                    var u = processArguments.GetCustomAttribute<Usage>();
                    if (u == null)
                    {
                        return "[argument]...";
                    }
                    return u.Description;
                }
            }

            public void ProcessArguments(string[] args)
            {
                actualHandler.ProcessArguments(args);
            }
        }

        class EmptyArgumentHandler : IArgumentHandlerWithUsage
        {
            public string Usage => String.Empty;

            public void ProcessArguments(string[] args)
            {
                foreach (var i in args)
                {
                    log.WarnFormat("Argument {0} is ignored.", i);
                }
            }
        }

        internal IArgumentHandlerWithUsage GetArgumentHandler()
        {
            var argumentHandler = MainModule as IArgumentHandler;
            if (argumentHandler != null) return new ArgumentHandlerWrapper(argumentHandler);

            // look for method with ArgumentHandler attribute to handle arguments
            var argumentHandlerWithUsage = MethodArgumentHandler.Create(MainModule);
            if (argumentHandlerWithUsage != null) return argumentHandlerWithUsage;

            return new EmptyArgumentHandler();
        }

        internal int Run(Args args)
        {
            try
            {
                var argList = new List<string>();

                for (; args.HasNext;)
                {
                    if (HandleTwoDashes(args, argList, longOptionPrefix)) continue;
                    if (HandleLongOption(args, Options, longOptionPrefix)) continue;
                    if (HandleOption(args, Options, shortOptionPrefix)) continue;
                    if (HandleCommand(args, Commands, commandPrefix)) continue;
                    if (HandleNormalArgument(args, argList)) continue;
                }

                if (argList.Any())
                {
                    var argumentHandler = GetArgumentHandler();
                    log.InfoFormat("arguments: {0}", argList.Join(", "));
                    argumentHandler.ProcessArguments(argList.ToArray());
                }

                return 0;
            }
            catch (Sidi.CommandLine.CommandLineException cle)
            {
                using (var message = new StringWriter())
                {
                    message.WriteLine(cle.Message);
                    new GetOptInternal.ShowHelp(this).PrintHelp(message);
                    ShowMessage(message);
                }
                return -1;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return -1;
            }
        }

        static string ToString(Action<TextWriter> printer)
        {
            using (var w = new StringWriter())
            {
                printer(w);
                return w.ToString();
            }
        }

        internal void RunCommand(Args args)
        {
            try
            {
                var argList = new List<string>();
                bool commandDidNotConsumeAll = false;

                for (; args.HasNext;)
                {
                    if (HandleTwoDashesAsCommandEnd(args, argList, longOptionPrefix))
                    {
                        commandDidNotConsumeAll = true;
                        break;
                    };
                    if (HandleLongOption(args, Options, longOptionPrefix)) continue;
                    if (HandleOption(args, Options, shortOptionPrefix)) continue;
                    if (HandleCommand(args, Commands, commandPrefix)) continue;
                    if (HandleNormalArgument(args, argList)) continue;
                }

                if (argList.Any())
                {
                    var argumentHandler = GetArgumentHandler();
                    log.InfoFormat("arguments: {0}", argList.Join(", "));
                    argumentHandler.ProcessArguments(argList.ToArray());
                }

                if (!commandDidNotConsumeAll)
                {
                    Environment.Exit(0);
                }
            }
            catch (Sidi.CommandLine.CommandLineException cle)
            {
                using (var message = new StringWriter())
                {
                    message.WriteLine(cle.Message);
                    new GetOptInternal.ShowHelp(this).PrintHelp(message);
                    ShowMessage(message);
                }
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Environment.Exit(1);
            }
        }

        static bool IsConsole()
        {
            try
            {
                var t = Console.Title;
                return true;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }

        public void ShowMessage(object message)
        {
            var text = message.ToString();
            if (!IsConsole())
            {
                MessageBox.Show(text, this.ProgramName);
            }
            else
            {
                Console.Out.WriteLine(text);
            }
        }

        public string ProgramName
        {
            get
            {
                if (_ProgramName == null)
                {
                    return Process.GetCurrentProcess().ProcessName;
                }
                return _ProgramName;
            }

            set
            {
                _ProgramName = value;
            }
        }
        string _ProgramName;
    }
}
