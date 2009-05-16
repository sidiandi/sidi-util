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

namespace Sidi.CommandLine
{
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

    public class Action
    {
        public MethodInfo MethodInfo;
        public string Name { get { return MethodInfo.Name; } }
        public string Usage { get { return Sidi.CommandLine.Usage.Get(MethodInfo); } }
        public string Syntax
        {
            get
            {
                MethodInfo i = MethodInfo;
                string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
                {
                    return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.Name);
                })));
                return String.Format("{0} {1}", i.Name, parameters);
            }
        }
        
        public void PrintScriptFileSample(TextWriter w)
        {
            w.Write("# "); w.WriteLine(Usage);
            w.Write("# "); w.WriteLine(Syntax);
            w.WriteLine();
        }
    }

    public class Option
    {
        public MemberInfo MemberInfo;
        public string Name { get { return MemberInfo.Name; } }
        public string Usage { get { return Sidi.CommandLine.Usage.Get(MemberInfo); } }
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
                        "--{0} [{1}]",
                        i.Name,
                        fieldInfo.FieldType.Name
                        );
                }
                else if (i.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)i;
                    return String.Format(
                        Parser.CultureInfo,
                        "--{0} [{1}]",
                        i.Name,
                        propertyInfo.PropertyType.Name
                        );
                }
                throw new InvalidDataException(i.GetType().ToString());
            }
        }

        public void PrintScriptFileSample(TextWriter w)
        {
            w.Write("# "); w.WriteLine(Usage);
            w.Write("# "); w.WriteLine(Syntax);
            w.WriteLine();
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
    public class Parser
    {
        object m_application;
        static string[] optionPrefix = new string[] { "--", "-", "/" };
        static CultureInfo cultureInfo;

        public static CultureInfo CultureInfo { get { return cultureInfo; } }

        public Object Application
        {
            get { return m_application; }
        }

        static Parser()
        {
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;
        }

        public Parser(object application)
        {
            cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            cultureInfo.DateTimeFormat = dtfi;
            m_application = application;
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

                while (args.Count > 0)
                {
                    if (Application is CommandLineHandler)
                    {
                        CommandLineHandler h = (CommandLineHandler)Application;
                        h.BeforeParse(args);
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
            if (Application is CommandLineHandler)
            {
                CommandLineHandler h = (CommandLineHandler)Application;
                int c = args.Count;
                h.UnknownArgument(args);
                return args.Count != c;
            }
            else
            {
                return false;
            }
        }

        bool HandleOption()
        {
            string option = null;
            foreach (string op in optionPrefix)
            {
                if (args[0].StartsWith(op))
                {
                    option = args[0].Substring(op.Length);
                    break;
                }
            }

            if (option == null)
            {
                return false;
            }

            MemberInfo member = GetOption(option);
            if (member == null)
            {
                return false;
            }
            NextArg();
            if (member is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)member;
                object v = ParseValue(NextArg(), fi.FieldType);
                fi.SetValue(m_application, v);
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)member;
                object v = ParseValue(NextArg(), pi.PropertyType);
                pi.SetValue(m_application, v, new object[] { });
            }
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

        MemberInfo FuzzyMatch(IEnumerable<MemberInfo> members, string name)
        {
            name = name.ToLower();
            IEnumerable<MemberInfo> accessibleMembers = members.Where(x => Usage.Get(x) != null);

            foreach (MemberInfo i in accessibleMembers)
            {
                if (i.Name.ToLower() == name)
                {
                    return i;
                }
            }

            IEnumerable<MemberInfo> hits = accessibleMembers.Where(i => i.Name.ToLower().StartsWith(name));

            if (hits.Any())
            {
                if (hits.Count() > 1)
                {
                    throw new CommandLineException(
                        String.Format(
                            "Argument {0} is ambiguous. Possible arguments are: {1}", 
                            name,
                            hits.Select(x => x.Name).Aggregate((x,y) => x + ", " + y)
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

        MemberInfo GetOption(string name)
        {
            MemberInfo m = AppType.GetProperty(name);
            if (m != null) return m;
            m = AppType.GetField(name);
            if (m != null) return m;

            m = FuzzyMatch(AppType.GetProperties(), name);
            if (m != null) return m;

            m = FuzzyMatch(AppType.GetFields(), name);
            if (m != null) return m;

            return null;
        }

        Type AppType
        {
            get { return m_application.GetType(); }
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
            throw new InvalidCastException(type.ToString() + " is not supported");
        }

        MethodInfo GetAction(string actionName)
        {
            return (MethodInfo)FuzzyMatch(AppType.GetMethods(), actionName);
        }

        bool HandleAction()
        {
            string actionName = args[0];
            MethodInfo action = GetAction(actionName);
            if (action == null)
            {
                return false;
            }

            NextArg();
            ParameterInfo[] parameters = action.GetParameters();
            object[] parameterValues = new object[parameters.Length];

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(List<string>))
            {
                parameterValues[0] = args;
            }
            else
            {
                if (args.Count < parameters.Length)
                {
                    throw new CommandLineException(String.Format("Not enough parameters for action \"{0}\". {1} parameters are required, but only {2} are supplied.",
                        action.Name,
                        parameters.Length,
                        args.Count));
                }

                for (int i = 0; i < parameters.Length; ++i)
                {
                    string a = NextArg();
                    parameterValues[i] = ParseValue(a, parameters[i].ParameterType);
                }
            }
            action.Invoke(m_application, parameterValues);
            return true;
        }

        public IEnumerable<Action> Actions
        {
            get
            {
                foreach (MethodInfo i in m_application.GetType().GetMethods())
                {
                    string u = Usage.Get(i);
                    string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
                    {
                        return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.Name);
                    })));
                    if (u != null)
                    {
                        Action a = new Action();
                        a.MethodInfo = i;
                        yield return a;
                    }
                }
            }
        }

        public IEnumerable<Option> Options
        {
            get
            {
                foreach (MemberInfo i in Application.GetType().GetMembers())
                {
                    string u = Usage.Get(i);
                    if ((i is FieldInfo || i is PropertyInfo) && u != null)
                    {
                        Option o = new Option();
                        o.MemberInfo = i;
                        yield return o;
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
                return m_application.GetType().Name;
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
                    Usage.Get(m_application.GetType()))
                    );

                Assembly assembly = AppType.Assembly;

                List<string> infos = new List<string>();

                infos.Add(String.Format("Version {0}", assembly.GetName().Version));

                object[] a = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
                a = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (a.Length > 0)
                {
                    infos.Add(((AssemblyCopyrightAttribute)a[0]).Copyright);
                }

                w.WriteLine(String.Join(", ", infos.ToArray()));
                return w.ToString();
            }
        }

        public string Info
        {
            get
            {
                StringWriter i = new StringWriter();
                i.WriteLine(
                String.Format("{0} - {1}",
                    ApplicationName,
                    Usage.Get(m_application.GetType()))
                );

                Assembly assembly = AppType.Assembly;

                List<string> infos = new List<string>();

                infos.Add(String.Format("Version {0}", assembly.GetName().Version));

                object[] a = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
                a = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (a.Length > 0)
                {
                    infos.Add(((AssemblyCopyrightAttribute)a[0]).Copyright);
                }

                i.Write(String.Join(", ", infos.ToArray()));
                return i.ToString();
            }
        }

        public void ShowUsage()
        {
            Console.WriteLine(Info);
            Console.WriteLine(String.Format("Usage: {0} --option1 value --option2 value action [parameters]", ApplicationName));
            Console.WriteLine();
            Console.WriteLine("Actions");
            Console.WriteLine();

            foreach (MethodInfo i in m_application.GetType().GetMethods())
            {
                string u = Usage.Get(i);
                string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
                {
                    return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.Name);
                })));
                if (u != null)
                {
                    Console.WriteLine(String.Format("  {0} {2}\r\n    {1}\r\n", i.Name, u, parameters));
                }
            }

            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine();

            foreach (MemberInfo i in m_application.GetType().GetMembers())
            {
                if (i.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldInfo = (FieldInfo)i;
                    object defaultValue = fieldInfo.GetValue(m_application);
                    string u = Usage.Get(i);
                    if (u != null)
                    {
                        Console.WriteLine(String.Format(
                            cultureInfo,
                            "  --{0}\r\n    Type: {1}, default: {3}\r\n    {2}\r\n",
                            i.Name,
                            fieldInfo.FieldType.Name,
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
                        Console.WriteLine(String.Format(
                            cultureInfo,
                            "  --{0}\r\n    Type: {1}, default: {3}\r\n    {2}\r\n",
                            i.Name,
                            propertyInfo.PropertyType.Name,
                            u,
                            defaultValue));
                    }
                }
            }
        }

        public void PrintSampleScript(TextWriter w)
        {
            string applicationName = m_application.GetType().Name;
            Console.WriteLine(
                String.Format("{0} - {1}",
                applicationName,
                Usage.Get(m_application.GetType()))
                );

            Assembly assembly = AppType.Assembly;

            List<string> infos = new List<string>();


            infos.Add(String.Format("Version {0}", assembly.GetName().Version));

            object[] a = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
            a = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (a.Length > 0)
            {
                infos.Add(((AssemblyCopyrightAttribute)a[0]).Copyright);
            }

            w.WriteLine(String.Join(", ", infos.ToArray()));
            w.WriteLine();
            w.WriteLine("Actions");
            w.WriteLine();

            foreach (MethodInfo i in m_application.GetType().GetMethods())
            {
                string u = Usage.Get(i);
                string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
                {
                    return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.Name);
                })));
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
                            "  --{0}\r\n    Type: {1}, default: {3}\r\n    {2}\r\n",
                            i.Name,
                            fieldInfo.FieldType.Name,
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
                            "  --{0}\r\n    Type: {1}, default: {3}\r\n    {2}\r\n",
                            i.Name,
                            propertyInfo.PropertyType.Name,
                            u,
                            defaultValue));
                    }
                }
            }
        }

    }
}
