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
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Appender;
using log4net.Layout;

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

    public interface CommandLineHandler2
    {
        void BeforeParse(IList<string> args, Parser parser);
        void UnknownArgument(IList<string> args, Parser parser);
    }

    [Serializable]
    public class CommandLineException : Exception
    {
        public CommandLineException(string reason, Exception innerException = null)
            : base(reason, innerException)
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
        public InvalidParameterException(ParameterInfo parameter, Exception inner)
        : base(parameter.ToString(), inner)
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
        public const string categoryLogging = "Logging";
        public const string categoryUsage = "Usage";

        List<ItemSource> m_itemSources = new List<ItemSource>();
        public Dictionary<Type, string[]> Prefix = new Dictionary<Type, string[]>();
        static CultureInfo cultureInfo;

        public static CultureInfo CultureInfo { get { return cultureInfo; } }

        public List<ItemSource> ItemSources
        {
            get { return m_itemSources; }
            set { m_itemSources = value; }
        }

        List<ItemSource> builtInSources;

        public List<IParserItem> SubParsers = new List<IParserItem>();

        public ItemSource MainSource
        {
            get
            {
                return ItemSources.First();
            }
        }

        public ItemSource StartupSource
        {
            get
            {
                if (Parent == null)
                {
                    return MainSource;
                }
                else
                {
                    return Parent.StartupSource;
                }
            }
        }

        static Parser()
        {
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;
        }

        public Parser(params object[] sources)
        : this()
        {
            if (sources.Any())
            {
                ItemSources.AddRange(sources.Select(x => new ItemSource(x)));
                AddDefaultUserInterface();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000: DisposeObjectsBeforeLosingScope")]
        public void AddDefaultUserInterface()
        {
            ItemSources.Add(new ItemSource(new ShowUserInterface(this)));
            ItemSources.Add(new ItemSource(new ShowHelp(this)));
            LogOptions = new LogOptions(this);
            ItemSources.Add(new ItemSource(LogOptions) { ProgramSpecificPreferences = true });
            ItemSources.Add(new ItemSource(new ShowWebServer(this)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000: DisposeObjectsBeforeLosingScope")]
        public void AddDefaultUserInterfaceForSubCommands()
        {
            ItemSources.Add(new ItemSource(new ShowUserInterface(this)));
            ItemSources.Add(new ItemSource(new ShowHelp(this)));
            if (!this.Parent.ItemSources.Any(x => x.Instance is WebServer))
            {
                ItemSources.Add(new ItemSource(new ShowWebServer(this)));
            }
        }

        public LogOptions LogOptions { get; private set; }

        internal Parser()
        {
            var dumper = new Dump() { MaxLevel = 1 };
            ProcessResult = result => { };

            Prefix = new Dictionary<Type, string[]>();
            Prefix[typeof(Option)] = new string[] { "--", "-", "/", String.Empty };
            
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;
            builtInSources = new object[] { new BasicValueParsers() }
                .Select(x => new ItemSource(x)).ToList();
        }

        public Parser Parent { get; set; }

        public static int Run(object itemSource, string[] args)
        {
            Parser parser = new Parser(itemSource);
            return parser.Run(args);
        }

        /// <summary>
        /// Loads preferences, executes args, stores preferences
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int Run(string[] args)
        {
            var parser = this;
            try
            {
                parser.LoadPreferences();
                parser.Parse(args);
                parser.StorePreferences();
                return 0;
            }
            catch (CommandLineException exception)
            {
                log.Error(exception);
                Console.WriteLine(exception.Message);
                Console.WriteLine("Type \"{0}\" to get usage information.", parser.ApplicationName);
                return 1;
            }
            catch (TargetInvocationException exception)
            {
                log.Error(exception.InnerException);
                Console.Error.WriteLine("Error: " + exception.InnerException.Message);
                return 1;
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

            while (args.Count > 0)
            {
                ParseSingleCommand(args);
            }
        }

        public void ParseSingleCommand(IList<string> args)
        {
            foreach (var i in ItemSources.Select(x => x.Instance).OfType<CommandLineHandler>())
            {
                i.BeforeParse(args);
            }

            foreach (var i in ItemSources.Select(x => x.Instance).OfType<CommandLineHandler2>())
            {
                i.BeforeParse(args, this);
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
                    var key = GetPreferencesKey(o);
                    var valueName = o.Name;
                    log.DebugFormat("Load persistent option {0} from {1}\\{2}", o, key, valueName);
                    var value = Registry.GetValue(key, valueName, null);
                    if (value != null)
                    {
                        var valueString = value.ToString();
                        if (o.IsPassword)
                        {
                            valueString = valueString.Decrypt(preferencesPassword);
                        }
                        o.Handle(new string[] { valueString }.ToList(), true);
                    }

                    foreach (var s in SubCommands)
                    {
                        s.LoadPreferences();
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
                    var valueString = value.ToString();
                    if (o.IsPassword)
                    {
                        valueString = valueString.Encrypt(preferencesPassword);
                    }

                    if (valueString != null)
                    {
                        var key = GetPreferencesKey(o);
                        var valueName = o.Name;
                        log.DebugFormat("Store persistent option {0} = {1} in {2}\\{3}", o.Name, o.DisplayValue, key, valueName);
                        Registry.SetValue(key, valueName, valueString);
                    }
                }
            }

            foreach (var s in SubCommands)
            {
                s.StorePreferences();
            }
        }

        public static IEnumerable<string> GetNameParts(Type type)
        {
            return type.FullName.Split('.');
        }

        public bool? ApplicationSpecificPreferences;

        public string GetPreferencesKey(Option o)
        {
            IEnumerable<string> parts = new[] { PreferencesKey };
            bool applicationSpecific;
            if (ApplicationSpecificPreferences != null)
            {
                applicationSpecific = ApplicationSpecificPreferences.Value;
                goto done;
            }

            if (o.Source.ProgramSpecificPreferences != null)
            {
                applicationSpecific = o.Source.ProgramSpecificPreferences.Value;
                goto done;
            }

            applicationSpecific = o.GetPersistentAttribute().ApplicationSpecific;
            
            done:

            if (applicationSpecific)
            {
                parts = parts.Concat(GetNameParts(StartupSource.Instance.GetType()));
            }
            parts = parts.Concat(GetNameParts(o.Source.Instance.GetType()));

            return CatReg(parts.ToArray());
        }

        string CatReg(params string[] parts)
        {
            return parts.Join(@"\");
        }

        /// <summary>
        /// Registry key for LoadPreferences and StorePreferences. 
        /// Default is HKEY_CURRENT_USER\Software\[your company]\[your product]
        /// </summary>
        public string PreferencesKey
        {
            get
            {
                if (m_PreferencesKey == null)
                {
                    var k = CatReg(Registry.CurrentUser.ToString(), "Software");
                    var applicationType = MainSource.Instance.GetType();
                    var company = applicationType.GetAssemblyAttribute<AssemblyCompanyAttribute>().Company;
                    var product = applicationType.GetAssemblyAttribute<AssemblyProductAttribute>().Product;
                    k = CatReg(k, company, product, Profile);
                    m_PreferencesKey = k;
                }
                return m_PreferencesKey;
            }

            set
            {
                m_PreferencesKey = value;
            }
        }
        string m_PreferencesKey;

        public string Profile
        {
            get
            {
                if (!String.IsNullOrEmpty(m_profile))
                {
                    return m_profile;
                }

                if (this.Parent == null)
                {
                    return defaultProfile;
                }
                else
                {
                    return Parent.Profile;
                }
            }

            set
            {
                m_profile = value;
            }
        }
        string m_profile;
        const string defaultProfile = "default";

        bool HandleUnknown(IList<string> args)
        {
            foreach (var h in ItemSources.Select(x => x.Instance).OfType<CommandLineHandler>())
            {
                int c = args.Count;
                h.UnknownArgument(args);
                return args.Count != c;
            }

            foreach (var h in ItemSources.Select(x => x.Instance).OfType<CommandLineHandler2>())
            {
                int c = args.Count;
                h.UnknownArgument(args, this);
                return args.Count != c;
            }

            return false;
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

        string[] GetPrefixes(IParserItem item)
        {
            string[] prefix;
            if (!Prefix.TryGetValue(item.GetType(), out prefix))
            {
                prefix = new string[] { String.Empty };
            }
            return prefix;
        }

        public bool IsExactMatch(string userInput, IParserItem item)
        {
            string name;
            if (!DetectPrefix(userInput, GetPrefixes(item), out name))
            {
                return false;
            }

            return IsExactMatch(name, item.Name);
        }

        public bool IsMatch(string userInput, IParserItem item)
        {
            string name;
            if (!DetectPrefix(userInput, GetPrefixes(item), out name))
            {
                return false;
            }
            return IsMatch(name, item.Name);
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
        /// Detects and removes an option prefix from text
        /// </summary>
        /// <param name="text">Text with prefix</param>
        /// <param name="prefixes">array of prefixes to be searched for</param>
        /// <param name="trimmedText">returns the text without the prefix</param>
        /// <returns>True, if prefix was found</returns>
        bool DetectPrefix(string text, string[] prefixes, out string trimmedText)
        {
            foreach (var p in prefixes)
            {
                if (text.StartsWith(p))
                {
                    trimmedText = text.Substring(p.Length);
                    return true;
                }
            }
            trimmedText = text;
            return false;
        }

        public IParserItem LookupParserItem(string name, IEnumerable<IParserItem> parserItems)
        {
            var exact = parserItems.FirstOrDefault(x => IsExactMatch(name, x));
            if (exact != null)
            {
                return exact;
            }

            var hits = parserItems.Where(x => IsMatch(name, x)).ToList();
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
            return LookupParserItem(name, Items);
        }

        Dictionary<Type, IValueParser> usedValueParsers = new Dictionary<Type, IValueParser>();

        public IValueParser CreateValueParser(Type type)
        {
                if (type.IsEnum)
                {
                    return new EnumValueParser(type);
                }
                else if (type.IsArray)
                {
                    return new ArrayValueParser(type, this);
                }

                return new StandardValueParser(type);
        }
        
        public IValueParser GetValueParser(Type type)
        {
            IValueParser p;
            if (!usedValueParsers.TryGetValue(type, out p))
            {
                p = AvailableValueParsers.FirstOrDefault(x => x.ValueType.Equals(type));
                if (p != null) goto found;
                p = CreateValueParser(type);
found:
                usedValueParsers[type] = p;
            }
            return p;
        }

        public T ParseValue<T>(IList<string> args)
        {
            return (T)ParseValue(args, typeof(T));
        }

        public T ParseValue<T>(string text)
        {
            var args = new List<string>(){ text };
            return (T)ParseValue(args, typeof(T));
        }

        public object ParseValue(IList<string> args, Type type)
        {
            var originalArgs = new List<string>(args);
            try
            {
                var vp = GetValueParser(type);
                if (vp == null)
                {
                    var value = ParseValueBuiltIn(args[0], type);
                    args.RemoveAt(0);
                    return value;
                }
                return vp.Handle(args, this.execute);
            }
            catch (Exception ex)
            {
                throw new CommandLineException(String.Format("Cannot interpret argument(s) \"{0}\" as value of type {1}", originalArgs.Join(" "), type), ex);
            }
        }
        
        public static object ParseValueBuiltIn(string stringRepresentation, Type type)
        {
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

        public bool HandleParserItem(IList<string> args)
        {
            var parserItem = LookupParserItem(args[0]);
            if (parserItem == null)
            {
                return false;
            }

            args.PopHead();
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

        IList<IParserItem> items;

        public IList<IParserItem> Items
        {
            get
            {
                if (items == null)
                {
                    items = new List<IParserItem>(FindItems());
                }
                return items;
            }
        }

        /// <summary>
        /// All available actions and options
        /// </summary>
        IEnumerable<IParserItem> FindItems()
        {
            var items = SubParsers
                .Concat(
                    ItemSources.Concat(builtInSources)
                    .SelectMany(x => x.FindItems(this)))
                    .ToList();

            if (!items.Any())
            {
                throw new Exception();
            }

            return items;
        }

        List<IValueParser> availableValueParsers;

        /// <summary>
        /// All value parsers
        /// </summary>
        public IList<IValueParser> AvailableValueParsers
        {
            get
            {
                if (availableValueParsers == null)
                {
                    availableValueParsers = ItemSources.Concat(builtInSources)
                        .SelectMany(a => a.GetValueParsers(this))
                        .ToList();
                }
                return availableValueParsers;
            }
        }

        /// <summary>
        /// Used value parsers
        /// </summary>
        public IEnumerable<IValueParser> UsedValueParsers
        {
            get
            {
                return usedValueParsers.Values;
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
                return StartupSource.Instance.GetType().Name;
            }
        }

        public string VersionInfo
        {
            get
            {
                using (var w = new StringWriter())
                {
                    Assembly assembly = MainSource.Instance.GetType().Assembly;

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
                var app = StartupSource.Instance;
                var appType = app.GetType();

                using (var i = new StringWriter())
                {
                    i.WriteLine(
                    String.Format("{0} - {1}",
                        ApplicationName,
                        Usage.Get(appType))
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
            WriteUsageShort(Console.Out);
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
            w.WriteLine();
            w.WriteLine(String.Format("Syntax: {0} command [parameters] command [parameters] ...", ApplicationName));
            w.WriteLine("All commands can be abbreviated, as long as they are distinct. Example: ShowFile can be abbreviated as sf");
            WriteUsageByCategory(w, Items);
        }

        /// <summary>
        /// Writes usage information to a TextWriter
        /// </summary>
        /// <param name="w">Receives the usage message.</param>
        public void WriteUsageShort(TextWriter w)
        {
            w.WriteLine(Info);
            w.WriteLine();
            w.WriteLine(String.Format("Syntax: {0} command [parameters] command [parameters] ...", ApplicationName));
            WriteUsageByCategory(w, Items.Where(x => IncludeInShortUsage(x)));
        }

        static bool IncludeInShortUsage(IParserItem item)
        {
            return !item.Source.Instance.GetType().Namespace.Equals("Sidi.CommandLine") ||
                item.Name.Equals("Usage");
        }

        public IList<KeyValuePair<string, IList<IParserItem>>>
            GroupByCategory(IEnumerable<IParserItem> items)
        {
            return items
                .Where(x => !(x is StaticMethodValueParser))
                .SelectMany(i => i.Categories.Select(x => new { Category = x, Item = i }))
                .GroupBy(x => x.Category)
                .OrderBy(x => x.Key)
                .Select(x => new KeyValuePair<string, IList<IParserItem>>(
                    x.Key,
                    x.Select(y => y.Item).OrderBy(y => y.Name).ToList()))
                .ToList();
        }

        public void WriteUsageByCategory(TextWriter w, IEnumerable<IParserItem> items)
        {
            var g = GroupByCategory(items);

            foreach (var catItems in g)
            {
                if (catItems.Value.Any())
                {
                    if (!String.IsNullOrEmpty(catItems.Key))
                    {
                        w.WriteLine();
                        w.WriteLine(catItems.Key);
                    }

                    foreach (var i in catItems.Value)
                    {
                        w.WriteLine();
                        w.WriteLine(i.UsageText.Indent(indent).Wrap(maxColumns));
                    }
                }
            }
        }

        public void PrintSampleScript(TextWriter w)
        {
            string applicationName = ItemSources.Last().GetType().Name;
            Console.WriteLine(
                String.Format("{0} - {1}",
                applicationName,
                Usage.Get(MainSource.Instance.GetType()))
                );

            Assembly assembly = MainSource.Instance.GetType().Assembly;

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

            foreach (var source in ItemSources)
            {
                w.WriteLine("Actions");
                w.WriteLine();

                foreach (MethodInfo i in source.Instance.GetType().GetMethods())
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

                foreach (MemberInfo i in source.Instance.GetType().GetMembers())
                {
                    if (i.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fieldInfo = (FieldInfo)i;
                        object defaultValue = i.GetValue(source.Instance);
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
                        object defaultValue = i.GetValue(source.Instance);
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

        public static Parser SingleSource(object instance)
        {
            return new Parser()
            {
                ItemSources = new List<ItemSource>(){ new ItemSource(instance) }
            };
        }

        public void ParseBraces(IList<string> args)
        {
            var braceOpen = "(";
            var braceClose = ")";
            if (args[0].StartsWith(braceOpen))
            {
                args[0] = args[0].Substring(braceOpen.Length);
                if (args[0].Length == 0)
                {
                    args.RemoveAt(0);
                }
                for (int i = 0; i < args.Count; ++i)
                {
                    if (args[i].EndsWith(braceClose))
                    {
                        args[i] = args[i].Substring(0, args[i].Length - braceOpen.Length);
                        args.Insert(i + 1, braceClose);
                        if (args[i].Length == 0)
                        {
                            args.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
            else
            {
                ParseSingleCommand(args);
                return;
            }

            while (args.Any())
            {
                if (args.First().Equals(braceClose))
                {
                    args.PopHead();
                    break;
                }
                ParseSingleCommand(args);
            }
        }
    }
}
