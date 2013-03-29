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
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Globalization;
using Sidi.Util;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Sidi.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Sidi.CommandLine
{
    /// <summary>
    /// Extension methods for Type
    /// </summary>
    public static class TypeExtensions
    {
        public static string GetInfo(this Type type)
        {
            if (type.IsEnum)
            {
                return "enum {0} ({1})".F(
                    type.Name,
                    Enum.GetValues(type).Cast<object>().Join(", "));
            }
            else
            {
                return type.Name;
            }
        }
    }

    public interface CommandLineHandler
    {
        void BeforeParse(IList<string> args);
        void UnknownArgument(IList<string> args);
    }

    [Serializable]
    public class CommandLineException : Exception
    {
        public CommandLineException(string reason)
            : base(reason)
        {
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException(ParameterInfo parameter)
        {
            this.Parameter = parameter;
        }

        public ParameterInfo Parameter { get; private set; }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Parameter", Parameter);
        }
    }

    /// <summary>
    /// Command line parser
    /// </summary>
    /// Helps to transform your class into a command line application with attributes.
    /// You only have to decorate your class with [Usage("...")] attributes, and Parser will 
    /// do the rest for you.
    /// 
    /// Supported argument types are: bool,int, double, string, DirectoryInfo,
    /// FileSystemInfo, DateTime, TimeSpan and enums.
    /// You can use System.ComponentModel.CategoryAttribute to sort your actions and
    /// options into categories to make the usage message more readable.
    public class Parser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string ListTerminator = ";";
        public const string categoryUserInterface = "User Interface";
        
        List<object> m_applications = new List<object>();
        static string[] optionPrefix = new string[] { "--", "-", "/" };
        static CultureInfo cultureInfo;

        public static CultureInfo CultureInfo { get { return cultureInfo; } }

        public List<object> Applications
        {
            get { return m_applications; }
        }

        public List<IParserItem> SubParsers = new List<IParserItem>();

        public object MainApplication
        {
            get
            {
                return Applications.Last();
            }
        }

        static Parser()
        {
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;
        }

        public Parser(params object[] applications)
        : this()
        {
            AddDefaultUserInterface();
            Applications.AddRange(applications);
        }

        void AddDefaultUserInterface()
        {
            Applications.Add(new ShowUserInterface(this));
            Applications.Add(new ShowHelp(this));
            Applications.Add(new ShowWebServer(this));
        }

        internal Parser()
        {
            var dumper = new Dump() { MaxLevel = 1 };
            ProcessResult = result => { };
            
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;
        }

        public static void Run(object application, string[] args)
        {
            Parser parser = new Parser(application);    
            try
            {
                parser.LoadPreferences();
                parser.Parse(args);
                parser.StorePreferences();
            }
            catch (CommandLineException exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("Type \"{0}\" to get usage information.", parser.ApplicationName);
            }
            catch (TargetInvocationException exception)
            {
                log.Error(exception.InnerException);
                Console.WriteLine();
                Console.WriteLine("Error: " + exception.InnerException.Message);
            }
        }

        public void Check(string[] args)
        {
            Check(args.ToList());
        }

        /// <summary>
        /// Checks if the arguments are syntactically correct, but does not execute 
        /// anything. Throws the same exceptions as Parse when the arguments contain
        /// an error.
        /// </summary>
        /// <param name="a_args"></param>
        public void Check(IList<string> a_args)
        {
            execute = false;
            try
            {
                Parse(a_args);
            }
            catch
            {
                throw;
            }
            finally
            {
                execute = true;
            }
        }

        /// <summary>
        /// Checks if the arguments are syntactically correct, but does not execute 
        /// anything. Throws the same exceptions as Parse when the arguments contain
        /// an error.
        /// </summary>
        /// <param name="a_args"></param>
        public void CheckSingleCommand(IList<string> a_args)
        {
            execute = false;
            try
            {
                ParseSingleCommand(a_args);
            }
            catch
            {
                throw;
            }
            finally
            {
                execute = true;
            }
        }

        bool execute = true;

        public void Parse(string[] args)
        {
            Parse(args.ToList());
        }

        public void Parse(IList<string> args)
        {
            if (args.Count == 0)
            {
                ShowUsage();
                return;
            }

            log.InfoFormat("Arguments: {0}", args.Join(" "));

            while (args.Count > 0)
            {
                ParseSingleCommand(args);
            }
        }

        public void ParseSingleCommand(IList<string> args)
        {
            foreach (var i in Applications)
            {
                if (i is CommandLineHandler)
                {
                    CommandLineHandler h = (CommandLineHandler)i;
                    h.BeforeParse(args);
                }
            }

            if (args.Count == 0)
            {
                return;
            }

            if (HandleParserItem(args))
            {
                return;
            }

            if (HandleUnknown(args))
            {
                return;
            }

            throw new CommandLineException("Argument " + args[0] + " is unknown.");
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void LoadPreferences()
        {
            foreach (var o in Options.Where(x => x.IsPersistent))
            {
                try
                {
                    var value = Registry.GetValue(GetPreferencesKey(o), o.Name, null);
                    if (value != null)
                    {
                        var valueString = value.ToString();
                        log.InfoFormat("Restore persistent option {0}", o);
                        if (o.IsPassword)
                        {
                            valueString = valueString.Decrypt(preferencesPassword);
                        }
                        o.Handle(new string[] { valueString }.ToList(), true);
                    }
                }
                catch (Exception ex)
                {
                    log.Warn(String.Format("Could not load preferences for {0}", o), ex);
                }
            }
        }

        /// <summary>
        /// It is not safe to use an internal password
        /// to store secrets in the registry, but better than nothing, however.
        /// </summary>
        static string preferencesPassword = "^69@KE3i3%VKAxAd";

        public void StorePreferences()
        {
            foreach (var o in Options.Where(x => x.IsPersistent))
            {
                var value = o.GetValue();
                if (value != null)
                {
                    var v = value.ToString();
                    if (o.IsPassword)
                    {
                        v = v.Encrypt(preferencesPassword);
                    }

                    if (v != null)
                    {
                        log.InfoFormat("Store persistent option {0} = {1}", o.Name, o.DisplayValue);
                        Registry.SetValue(GetPreferencesKey(o), o.Name, v);
                    }
                }
            }

            foreach (var s in SubCommands.Where(x => x.IsInstanciated))
            {
                var p = new Parser();
                p.Applications.Add(s.CommandInstance);
                p.StorePreferences();
            }
        }

        /// <summary>
        /// The registry key for an option is HKEY_CURRENT_USER\Software\[Company]\[Product]\[Full application class name]
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string GetPreferencesKey(Option o)
        {
            return GetPreferencesKey(o.Application.GetType());
        }

        string CatReg(params string[] parts)
        {
            return parts.Join(@"\");
        }

        public string GetPreferencesKey(Type at)
        {
            var k = PreferencesKey;
            if (k == null)
            {
                k = CatReg(Registry.CurrentUser.ToString(), "Software");
                var company = GetAssemblyAttribute<AssemblyCompanyAttribute>(at).Company;
                var product = GetAssemblyAttribute<AssemblyProductAttribute>(at).Product;
                k = CatReg(k, company, product, at.FullName);
            }
            k = CatReg(k, at.FullName);
            return k;
        }

        T GetAssemblyAttribute<T>(Type t)
        {
            var a = t.Assembly;
            try
            {
                return ((T)a.GetCustomAttributes(typeof(T), false).First());
            }
            catch (Exception)
            {
                throw new Exception(String.Format("{0} is not defined for assembly {1}", typeof(T).ToString(), a));
            }
        }

        /// <summary>
        /// Registry key for LoadPreferences and StorePreferences. 
        /// Default is HKEY_CURRENT_USER\Software\[your company]\[your product]
        /// </summary>
        public string PreferencesKey { get; set; }

        bool HandleUnknown(IList<string> args)
        {
            foreach (var i in Applications)
            {
                if (i is CommandLineHandler)
                {
                    CommandLineHandler h = (CommandLineHandler)i;
                    int c = args.Count;
                    h.UnknownArgument(args);
                    return args.Count != c;
                }
            }
            return false;
        }

        string NextArg(IList<string> args)
        {
            string a = args[0];
            args.RemoveAt(0);
            return a;
        }

        void Error(string msg, object o)
        {
            Error(String.Format(msg, o));
        }

        void Error(string msg)
        {
            Console.WriteLine(msg);
            Console.WriteLine();
            ShowUsage();
        }

        public bool IsExactMatch(string userInput, string memberName)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(userInput, memberName) == 0;
        }

        public static bool IsMatch(string userInput, string memberName)
        {
            return IsMatch(userInput, memberName, 0, 0);
        }

        static bool IsMatch(string a, string b, int ia, int ib)
        {
            if (ia >= a.Length)
            {
                return true;
            }

            if (ib >= b.Length)
            {
                return false;
            }

            if (Char.ToLower(a[ia]) != Char.ToLower(b[ib]))
            {
                return false;
            }

            var nc = ib + 1;
            for (; nc < b.Length && !Char.IsUpper(b[nc]); ++nc)
            {
            }
            if (IsMatch(a, b, ia + 1, nc))
            {
                return true;
            }

            return IsMatch(a, b, ia + 1, ib + 1);
        }

        /// <summary>
        /// Detects and removes an option prefix from name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool DetectOptionPrefix(ref string name)
        {
            foreach (var p in optionPrefix)
            {
                if (name.StartsWith(p))
                {
                    name = name.Substring(p.Length);
                    return true;
                }
            }
            return false;
        }

        IParserItem LookupParserItem(string name, IEnumerable<IParserItem> parserItems)
        {
            if (DetectOptionPrefix(ref name))
            {
                parserItems = parserItems.OfType<Option>().Cast<IParserItem>();
            }

            var exact = parserItems.FirstOrDefault(x => IsExactMatch(name, x.Name));
            if (exact != null)
            {
                return exact;
            }

            var hits = parserItems.Where(x => IsMatch(name, x.Name)).ToList();
            if (hits.Count == 1)
            {
                return hits.First();
            }
            else if (hits.Count == 0)
            {
                return null;
            }
            else
            {
                throw new CommandLineException(
                    String.Format(
                        "Argument {0} is ambiguous. Possible arguments are: {1}",
                        name,
                        hits.Select(x => x.Name).Aggregate((x, y) => x + ", " + y)
                        ));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Specifies the parser item. Abbreviations allowed.</param>
        /// <returns></returns>
        public IParserItem LookupParserItem(string name)
        {
            IEnumerable<IParserItem> parserItems = Items.ToList();
            return LookupParserItem(name, parserItems);
        }

        public object ParseValue(IList<string> args, Type type)
        {
            var vp = ValueParsers.FirstOrDefault(x => x.ValueType.Equals(type));
            if (vp == null)
            {
                var value = ParseValueBuiltIn(args[0], type);
                args.RemoveAt(0);
                return value;
            }
            return vp.Handle(args, this.execute);
        }
        
        public static object ParseValueBuiltIn(string stringRepresentation, Type type)
        {
            if (type == typeof(bool))
            {
                return bool.Parse(stringRepresentation);
            }
            if (type == typeof(int))
            {
                return int.Parse(stringRepresentation, cultureInfo);
            }
            else if (type == typeof(double))
            {
                return double.Parse(stringRepresentation, cultureInfo);
            }
            else if (type == typeof(string))
            {
                return stringRepresentation;
            }
            else if (type == typeof(DirectoryInfo))
            {
                return new DirectoryInfo(stringRepresentation);
            }
            else if (type == typeof(FileSystemInfo))
            {
                return Sidi.IO.FileUtil.GetFileSystemInfo(stringRepresentation);
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.Parse(stringRepresentation, cultureInfo);
            }
            else if (type == typeof(TimeSpan))
            {
                return DateTime.Parse(stringRepresentation, cultureInfo).TimeOfDay;
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, stringRepresentation);
            }

            var parse = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            if (parse != null)
            {
                return parse.Invoke(null, new object[] { stringRepresentation });
            }

            var ctor = type.GetConstructor(new Type[] { typeof(string) });
            if (ctor != null)
            {
                return ctor.Invoke(new object[] { stringRepresentation });
            }

            throw new InvalidCastException(type.ToString() + " is not supported");
        }

        bool HandleParserItem(IList<string> args)
        {
            var parserItem = LookupParserItem(args[0]);
            if (parserItem == null)
            {
                return false;
            }

            NextArg(args);
            var result = parserItem.Handle(args, execute);
            ProcessResult(result);
            return true;
        }

        Action<object> ProcessResult;

        /// <summary>
        /// deprecated
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public Action GetAction(string actionName)
        {
            return (Action) LookupParserItem(actionName, Items.Where(x => x is Action));
        }

        /// <summary>
        /// All available actions and options
        /// </summary>
        public IEnumerable<IParserItem> Items
        {
            get
            {
                foreach (var s in SubParsers)
                {
                    yield return s;
                }
                
                foreach (var application in Applications)
                {
                    foreach (MethodInfo i in application.GetType().GetMethods())
                    {
                        string u = Usage.Get(i);
                        string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
                        {
                            return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.GetInfo());
                        })));
                        if (u != null)
                        {
                            yield return new Action(this, application, i);
                        }

                        if (ValueParser.IsSuitable(i))
                        {
                            yield return new ValueParser(this, application, i);
                        }
                    }

                    foreach (MemberInfo i in application.GetType().GetMembers())
                    {
                        if (SubCommand.IsSubCommand(i))
                        {
                            if ((i is FieldInfo || i is PropertyInfo))
                            {
                                yield return new SubCommand(application, i);
                            }
                        }
                        else
                        {
                            string u = Usage.Get(i);
                            if ((i is FieldInfo || i is PropertyInfo) && u != null)
                            {
                                yield return new Option(this, application, i);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// All value parsers
        /// </summary>
        public IEnumerable<ValueParser> ValueParsers
        {
            get
            {
                return Items.OfType<ValueParser>();
            }
        }

        /// <summary>
        /// All actions
        /// </summary>
        public IEnumerable<Action> Actions
        {
            get
            {
                return Items.OfType<Action>();
            }
        }

        /// <summary>
        /// All options
        /// </summary>
        public IEnumerable<Option> Options
        {
            get
            {
                return Items.OfType<Option>();
            }
        }

        /// <summary>
        /// All options
        /// </summary>
        public IEnumerable<SubCommand> SubCommands
        {
            get
            {
                return Items.OfType<SubCommand>();
            }
        }

        public string ApplicationName
        {
            get
            {
                Assembly a = Assembly.GetEntryAssembly();
                if (a != null)
                {
                    return a.GetName().Name;
                }
                return MainApplication.GetType().Name;
            }
        }

        public string VersionInfo
        {
            get
            {
                using (var w = new StringWriter())
                {
                    w.WriteLine(
                        String.Format("{0} - {1}",
                        ApplicationName,
                        Usage.Get(MainApplication.GetType()))
                        );

                    Assembly assembly = MainApplication.GetType().Assembly;

                    List<string> infos = new List<string>();

                    infos.Add(String.Format("Version {0}", assembly.GetName().Version));

                    object[] a = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
                    a = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    if (a.Length > 0)
                    {
                        infos.Add(((AssemblyCopyrightAttribute)a[0]).Copyright);
                    }

                    w.WriteLine(infos.Join(", "));
                    return w.ToString();
                }
            }
        }

        public string Info
        {
            get
            {
                var app = Applications.Last();
                var appType = app.GetType();

                using (var i = new StringWriter())
                {
                    i.WriteLine(
                    String.Format("{0} - {1}",
                        ApplicationName,
                        Usage.Get(app.GetType()))
                    );

                    Assembly assembly = appType.Assembly;

                    List<string> infos = new List<string>();

                    infos.Add(String.Format("Version {0}", assembly.GetName().Version));

                    object[] a = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
                    a = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    if (a.Length > 0)
                    {
                        infos.Add(((AssemblyCopyrightAttribute)a[0]).Copyright);
                    }

                    i.Write(infos.Join(", "));
                    return i.ToString();
                }
            }
        }

        /// <summary>
        /// Writes usage information to the console.
        /// </summary>
        public void ShowUsage()
        {
            WriteUsage(Console.Out);
        }

        static string indent = "  ";
        static int maxColumns = 60;

        public IEnumerable<string> Categories
        {
            get
            {
                var categories =
                    Items.SelectMany(x => x.Categories)
                    .Distinct().ToList();
                categories.Sort();
                return categories;
            }
        }

        /// <summary>
        /// Writes usage information to a TextWriter
        /// </summary>
        /// <param name="w">Receives the usage message.</param>
        public void WriteUsage(TextWriter w)
        {
            w.WriteLine(Info);
            w.WriteLine(String.Format("Usage: {0} option1 value option2 value action [parameters]", ApplicationName));

            var categories = Categories;

            foreach (var category in categories)
            {
                var categoryText = category + " ";

                var actions = Actions.Where(x => x.Categories.Contains(category));

                if (actions.Any())
                {
                    w.WriteLine();
                    w.WriteLine("{0}Actions", categoryText);

                    foreach (Action a in actions)
                    {
                        w.WriteLine();
                        w.WriteLine(a.UsageText.Indent(indent).Wrap(maxColumns));
                    }
                }

                var options = Options.Where(x => x.Categories.Contains(category));

                if (options.Any())
                {
                    w.WriteLine();
                    w.WriteLine("{0}Options", categoryText);

                    foreach (Option i in options)
                    {
                        w.WriteLine();
                        w.WriteLine(i.UsageText.Indent(indent).Wrap(maxColumns));
                    }
                }

                var subCommands = SubCommands.Where(x => x.Categories.Contains(category));

                if (subCommands.Any())
                {
                    w.WriteLine();
                    w.WriteLine("{0}Subcommands", categoryText);

                    foreach (SubCommand i in subCommands)
                    {
                        w.WriteLine();
                        w.WriteLine(i.UsageText.Indent(indent).Wrap(maxColumns));
                    }
                }
            }
        }

        public void PrintSampleScript(TextWriter w)
        {
            string applicationName = Applications.Last().GetType().Name;
            Console.WriteLine(
                String.Format("{0} - {1}",
                applicationName,
                Usage.Get(MainApplication.GetType()))
                );

            Assembly assembly = MainApplication.GetType().Assembly;

            List<string> infos = new List<string>();


            infos.Add(String.Format("Version {0}", assembly.GetName().Version));

            object[] a = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
            a = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (a.Length > 0)
            {
                infos.Add(((AssemblyCopyrightAttribute)a[0]).Copyright);
            }

            w.WriteLine(infos.Join(", "));
            w.WriteLine();

            foreach (var m_application in Applications)
            {
                w.WriteLine("Actions");
                w.WriteLine();

                foreach (MethodInfo i in m_application.GetType().GetMethods())
                {
                    string u = Usage.Get(i);
                    string parameters = i.GetParameters()
                        .Select(pi => String.Format("[{1} {0}]", pi.Name, pi.ParameterType.GetInfo()))
                        .Join(" ");

                    if (u != null)
                    {
                        w.WriteLine(String.Format("  {0} {2}\r\n    {1}\r\n", i.Name, u, parameters));
                    }
                }

                w.WriteLine();
                w.WriteLine("Options");
                w.WriteLine();

                foreach (MemberInfo i in m_application.GetType().GetMembers())
                {
                    if (i.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fieldInfo = (FieldInfo)i;
                        object defaultValue = fieldInfo.GetValue(m_application);
                        string u = Usage.Get(i);
                        if (u != null)
                        {
                            w.WriteLine(String.Format(
                                cultureInfo,
                                "  {0}\r\n    Type: {1}, default: {3}\r\n    {2}\r\n",
                                i.Name,
                                fieldInfo.FieldType.GetInfo(),
                                u,
                                defaultValue));
                        }
                    }
                    else if (i.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo propertyInfo = (PropertyInfo)i;
                        object defaultValue = propertyInfo.GetValue(m_application, new object[] { });
                        string u = Usage.Get(i);
                        if (u != null)
                        {
                            w.WriteLine(String.Format(
                                cultureInfo,
                                "  {0}\r\n    Type: {1}, default: {3}\r\n    {2}\r\n",
                                i.Name,
                                propertyInfo.PropertyType.GetInfo(),
                                u,
                                defaultValue));
                        }
                    }
                }
            }
        }

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = NativeMethods.CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}
