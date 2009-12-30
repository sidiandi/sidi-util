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
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Globalization;
using Sidi.Util;
using System.ComponentModel;

namespace Sidi.CommandLine
{
    public static class TypeEx
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

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class Usage : System.Attribute
    {
        private string m_description = "";

        public string Description { get { return m_description; } }

        public Usage(string description)
        {
            m_description = description;
        }

        public static string Get(ICustomAttributeProvider cap)
        {
            Usage usage = (Usage)cap.GetCustomAttributes(typeof(Usage), true).FirstOrDefault();
            if (usage != null)
            {
                return usage.Description;
            }

            System.ComponentModel.DescriptionAttribute description = (System.ComponentModel.DescriptionAttribute)
                cap.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true).FirstOrDefault();
            if (description != null)
            {
                return description.Description;
            }

            return null;
        }
    }

    public interface IParserItem
    {
        string UsageText { get; }
        string Name { get; }
    }
    
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
                    .Select(pi => String.Format("[{1} {0}]", pi.Name, pi.ParameterType.GetInfo()))
                    .Join(" ");
                return String.Format("{0} {1}", i.Name, parameters);
            }
        }

        public string UsageText
        {
            get
            {
                string parameters = MethodInfo.GetParameters().Select(pi =>
                {
                    return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.GetInfo());
                }).Join(" ");

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

        public void Handle(List<string> args)
        {
            var parameters = MethodInfo.GetParameters();
            object[] parameterValues;

            if (parameters.Length > 0 && parameters[0].ParameterType == typeof(List<string>))
            {
                parameterValues = new object[] { args };
            }
            else
            {
                if (args.Count < parameters.Length)
                {
                    throw new CommandLineException(String.Format("Not enough parameters for action \"{0}\". {1} parameters are required, but only {2} are supplied.\r\n\r\n{3}\r\n",
                        Name,
                        parameters.Length,
                        args.Count,
                        UsageText));
                }

                parameterValues = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    parameterValues[i] = Parser.ParseValue(args[i], parameters[i].ParameterType);
                }
                args.RemoveRange(0, parameters.Length);
            }

            log.InfoFormat("Action {0}({1})", Name, parameterValues.Join(", "));
            MethodInfo.Invoke(Application, parameterValues);
        }
    }

    public class Option : IParserItem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Option(object application, MemberInfo memberInfo)
        {
            Application = application;
            MemberInfo = memberInfo;
        }
        
        public object Application { get; private set; }
        public MemberInfo MemberInfo { get; private set; }

        public string Name { get { return MemberInfo.Name; } }
        public string Usage { get { return Sidi.CommandLine.Usage.Get(MemberInfo); } }

        public IEnumerable<string> Categories
        {
            get
            {
                var c = MemberInfo.GetCustomAttributes(typeof(CategoryAttribute), true)
                    .Select(x => ((CategoryAttribute)x).Category);
                return c.Any() ? c : new string[] { String.Empty };
            }
        }

        public string Syntax
        {
            get
            {
                MemberInfo i = MemberInfo;
                if (i.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldInfo = (FieldInfo)i;
                    return String.Format(
                        Parser.CultureInfo,
                        "{0} [{1}]",
                        i.Name,
                        fieldInfo.FieldType.GetInfo()
                        );
                }
                else if (i.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)i;
                    return String.Format(
                        Parser.CultureInfo,
                        "{0} [{1}]",
                        i.Name,
                        propertyInfo.PropertyType.GetInfo()
                        );
                }
                throw new InvalidDataException(i.GetType().ToString());
            }
        }

        public object GetValue()
        {
            MemberInfo i = MemberInfo;
            if (i.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)i;
                return fieldInfo.GetValue(Application);
            }
            else if (i.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = (PropertyInfo)i;
                return propertyInfo.GetValue(Application, new object[] { });
            }
            throw new InvalidDataException(i.MemberType.ToString());
        }

        public Type Type
        {
            get
            {
                MemberInfo i = MemberInfo;
                if (i.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldInfo = (FieldInfo)i;
                    return fieldInfo.FieldType;
                }
                else if (i.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)i;
                    return propertyInfo.PropertyType;
                }
                throw new InvalidDataException(i.MemberType.ToString());
            }
        }

        public void PrintScriptFileSample(TextWriter w)
        {
            w.Write("# "); w.WriteLine(Usage);
            w.Write("# "); w.WriteLine(Syntax);
            w.WriteLine();
        }

        public void Handle(IList<string> args)
        {
            if (MemberInfo is FieldInfo)
            {
                FieldInfo fi = (FieldInfo) MemberInfo;
                object v = Parser.ParseValue(args[0], fi.FieldType);
                fi.SetValue(Application, v);
                args.RemoveAt(0);
                log.InfoFormat("Option {0} = {1}", fi.Name, fi.GetValue(Application));
            }
            else if (MemberInfo is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)MemberInfo;
                object v = Parser.ParseValue(args[0], pi.PropertyType);
                pi.SetValue(Application, v, new object[] { });
                args.RemoveAt(0);
                log.InfoFormat("Option {0} = {1}", pi.Name, pi.GetValue(Application, new object[] { }));
            }
            else
            {
                throw new CommandLineException("option is of invalid type: {0}".F(MemberInfo));
            }
        
        }

        public string UsageText
        {
            get
            {
                string indent = "  ";
                return String.Format(
                    Parser.CultureInfo,
                    "{0} [{1}]\r\n{2}\r\n{3}",
                    Name,
                    Type.GetInfo(),
                    "default: {0}".F(GetValue()).Indent(indent),
                    Usage.Indent(indent)
                    );
            }
        }
    }

    public class CommandLineException : Exception
    {
        public CommandLineException(string reason)
            : base(reason)
        {
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

        List<object> m_applications = new List<object>();
        static string[] optionPrefix = new string[] { "--", "-", "/" };
        static CultureInfo cultureInfo;

        public static CultureInfo CultureInfo { get { return cultureInfo; } }

        public List<object> Applications
        {
            get { return m_applications; }
        }

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

        public class ShowHelp
        {
            Parser parser;

            public ShowHelp(Parser parser)
            {
                this.parser = parser;
            }

            [Usage("Shows help for all options and actions that match searchString")]
            public void Help(string searchString)
            {
                foreach (var i in parser.Items)
                {
                    if (Regex.IsMatch(i.UsageText, searchString))
                    {
                        Console.WriteLine();
                        Console.WriteLine(i.UsageText);
                    }
                }
            }
        }

        public Parser(object application)
            : this()
        {
            Applications.Add(application);
        }

        public Parser()
        {
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;

            Applications.Add(new ShowHelp(this));
        }

        public static void Run(object application, string[] args)
        {
            Parser parser = new Parser(application);
            try
            {
                parser.Parse(args);
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

        List<string> args;

        public void Parse(string[] a_args)
        {
            args = new List<string>(a_args);

            if (args.Count == 0)
            {
                ShowUsage();
                // ShowGui();
                return;
            }

            log.InfoFormat("Arguments: {0}", args.Join(" "));

            while (args.Count > 0)
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
                    break;
                }

                if (HandleOption())
                {
                    continue;
                }

                if (HandleAction())
                {
                    continue;
                }

                if (HandleUnknown())
                {
                    continue;
                }

                throw new CommandLineException("Argument " + args[0] + " is unknown.");
            }
        }

        bool HandleUnknown()
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

        bool HandleOption()
        {
            string optionName = args[0];
            foreach (string op in optionPrefix)
            {
                if (args[0].StartsWith(op))
                {
                    optionName = args[0].Substring(op.Length);
                    break;
                }
            }

            if (optionName == null)
            {
                return false;
            }

            var option = GetOption(optionName);
            if (option == null)
            {
                return false;
            }
            NextArg();
            option.Handle(args);
            return true;
        }

        string NextArg()
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

        public bool IsMatch(string userInput, string memberName)
        {
            var m = memberName.GetEnumerator();
            foreach (var u in userInput)
            {
                if (!m.MoveNext())
                {
                    return false;
                }

                if (Char.ToLower(u) == Char.ToLower(m.Current))
                {
                    continue;
                }

                while (Char.IsLower(m.Current))
                {
                    if (!m.MoveNext())
                    {
                        return false;
                    }
                }

                if (Char.ToLower(u) != Char.ToLower(m.Current))
                {
                    return false;
                }
            }
            return true;
        }

        MemberInfo FuzzyMatch(IEnumerable<MemberInfo> members, string name)
        {
            name = name.ToLower();
            IEnumerable<MemberInfo> accessibleMembers = members.Where(x => Usage.Get(x) != null);

            var exact = accessibleMembers.FirstOrDefault(x => IsExactMatch(name, x.Name));
            if (exact != null)
            {
                return exact;
            }

            IEnumerable<MemberInfo> hits = accessibleMembers.Where(i => IsMatch(name, i.Name));

            if (hits.Any())
            {
                if (hits.Count() > 1)
                {
                    throw new CommandLineException(
                        String.Format(
                            "Argument {0} is ambiguous. Possible arguments are: {1}",
                            name,
                            hits.Select(x => x.Name).Aggregate((x, y) => x + ", " + y)
                            )
                    );
                }
                else
                {
                    return hits.First();
                }
            }
            else
            {
                return null;
            }
        }

        Option GetOption(string name)
        {
            foreach (var i in Applications)
            {
                var AppType = i.GetType();
                MemberInfo m = AppType.GetProperty(name);
                if (m != null) return new Option(i, m);
                m = AppType.GetField(name);
                if (m != null) return new Option(i, m);
            }

            foreach (var i in Applications)
            {
                var AppType = i.GetType();
                var m = FuzzyMatch(AppType.GetProperties(), name);
                if (m != null) return new Option(i, m);

                m = FuzzyMatch(AppType.GetFields(), name);
                if (m != null) return new Option(i, m);
            }

            return null;
        }

        public static object ParseValue(string stringRepresentation, Type type)
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
            throw new InvalidCastException(type.ToString() + " is not supported");
        }

        public Action GetAction(string actionName)
        {
            foreach (var i in Applications)
            {
                var m = (MethodInfo)FuzzyMatch(i.GetType().GetMethods(), actionName);
                if (m != null)
                {
                    return new Action(i, m);
                }
            }
            return null;
        }

        bool HandleAction()
        {
            string actionName = args[0];
            var action = GetAction(actionName);
            if (action == null)
            {
                return false;
            }

            NextArg();
            action.Handle(args);
            return true;
        }

        public IEnumerable<IParserItem> Items
        {
            get
            {
                return Actions.Cast<IParserItem>().Concat(Options.Cast<IParserItem>()).OrderBy(x => x.Name);
            }
        }

        public IEnumerable<Action> Actions
        {
            get
            {
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
                            yield return new Action(application, i);
                        }
                    }
                }
            }
        }

        public IEnumerable<Option> Options
        {
            get
            {
                foreach (var application in Applications)
                {
                    foreach (MemberInfo i in application.GetType().GetMembers())
                    {
                        string u = Usage.Get(i);
                        if ((i is FieldInfo || i is PropertyInfo) && u != null)
                        {
                            yield return new Option(application, i);
                        }
                    }
                }
            }
        }

        string ApplicationName
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
                StringWriter w = new StringWriter();
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

        public string Info
        {
            get
            {
                var app = Applications.Last();
                var appType = app.GetType();

                StringWriter i = new StringWriter();
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

        /// <summary>
        /// Writes usage information to the console.
        /// </summary>
        public void ShowUsage()
        {
            WriteUsage(Console.Out);
        }

        static string indent = "  ";
        static int maxColumns = 60;

        /// <summary>
        /// Writes usage information to a TextWriter
        /// </summary>
        /// <param name="w">Receives the usage message.</param>
        public void WriteUsage(TextWriter w)
        {
            w.WriteLine(Info);
            w.WriteLine(String.Format("Usage: {0} option1 value option2 value action [parameters]", ApplicationName));

            var categories =
                Actions.SelectMany(x => x.Categories)
                .Concat(Options.SelectMany(x => x.Categories))
                .Distinct().ToList();
            categories.Sort();

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
    }
}
