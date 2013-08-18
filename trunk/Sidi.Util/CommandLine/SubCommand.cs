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
using System.IO;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class SubCommand : IParserItem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        bool loadPreferencesOnInstanciation = false;
        Parser parentParser;
        Parser _parser;

        public SubCommand(Parser parent, Application application, MemberInfo memberInfo)
        {
            parentParser = parent;
            Application = application;
            MemberInfo = memberInfo;
        }

        public void LoadPreferences()
        {
            var c = GetValue();
            if (c != null)
            {
                Parser.LoadPreferences();
            }
            else
            {
                loadPreferencesOnInstanciation = true;
            }
        }

        public void StorePreferences()
        {
            if (GetValue() != null)
            {
                Parser.StorePreferences();
            }
        }

        public Application CommandApplication
        {
            get
            {
                if (_CommandApplication == null)
                {
                    _CommandApplication = new Application(CommandInstance);
                    var persistent = MemberInfo.GetCustomAttribute<PersistentAttribute>();
                    if (persistent != null)
                    {
                        _CommandApplication.ProgramSpecificPreferences = persistent.ApplicationSpecific;
                    }
                }
                return _CommandApplication;
            }
        }
        Application _CommandApplication;

        Parser Parser
        {
            get
            {
                if (_parser == null)
                {
                    _parser = new Parser() { Parent = parentParser };
                    _parser.Applications.Add(CommandApplication);

                    var instance = CommandApplication.Instance;
                    if (!(instance is ShowHelp))
                    {
                        _parser.Applications.Add(new Application(new ShowHelp(_parser)));
                    }

                    if (!(instance is ShowUserInterface))
                    {
                        _parser.Applications.Add(new Application(new ShowUserInterface(_parser)));
                    }

                    if (!(instance is ShowWebServer))
                    {
                        _parser.Applications.Add(new Application(new ShowWebServer(_parser)));
                    }

                    if (loadPreferencesOnInstanciation)
                    {
                        LoadPreferences();
                    }
                }

                return _parser;
            }
        }

        public Application Application { get; set; }

        public MemberInfo MemberInfo { get; private set; }

        public string Name { get { return MemberInfo.Name; } }
        
        public string Usage
        {
            get
            {
                var u = Sidi.CommandLine.Usage.Get(MemberInfo);
                if (u != null)
                {
                    return u;
                }
                u = Sidi.CommandLine.Usage.Get(Type);
                if (u != null)
                {
                    return u;
                }
                return String.Empty;
            }
        }

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
                return this.Name + " [subcommands...] ;";
            }
        }

        public string UsageText
        {
            get
            {
                string indent = "  ";
                return String.Format(
                    Parser.CultureInfo,
                    @"{0}
{1}",
                    Syntax,
                    Usage.Indent(indent)
                    );
            }
        }

        object GetValue()
        {
            MemberInfo i = MemberInfo;
            if (i.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)i;
                return fieldInfo.GetValue(Application.Instance);
            }
            else if (i.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = (PropertyInfo)i;
                return propertyInfo.GetValue(Application.Instance, new object[] { });
            }
            throw new InvalidDataException(i.MemberType.ToString());
        }

        Type Type
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
                throw new InvalidOperationException(i.MemberType.ToString());
            }
        }

        public void PrintScriptFileSample(TextWriter w)
        {
            w.Write("# "); w.WriteLine(Usage);
            w.Write("# "); w.WriteLine(Syntax);
            w.WriteLine();
        }

        public override string ToString()
        {
            return String.Format("Subcommand {0}", Name);
        }

        public object Handle(IList<string> args, bool execute)
        {
            log.InfoFormat("{0}", this);
            if (args.Any())
            {
                while (args.Any())
                {
                    if (args.First() == Parser.ListTerminator)
                    {
                        args.RemoveAt(0);
                        break;
                    }

                    if (execute)
                    {
                        Parser.ParseSingleCommand(args);
                    }
                    else
                    {
                        Parser.CheckSingleCommand(args);
                    }
                }
            }
            else
            {
                Parser.ShowUsage();
            }
            return null;
        }

        object CommandInstance
        {
            get
            {
                var ci = GetValue();
                if (ci != null)
                {
                    return ci;
                }
                
                // try to instanciate automatically
                // if value is null and property or field is 
                // writable

                if (MemberInfo is FieldInfo)
                {
                    var fi = (FieldInfo)MemberInfo;
                    ci = Activator.CreateInstance(fi.FieldType);
                    fi.SetValue(Application.Instance, ci);
                    goto created;
                }

                if (MemberInfo is PropertyInfo)
                {
                    var pi = (PropertyInfo)MemberInfo;
                    if (pi.CanWrite)
                    {
                        ci = Activator.CreateInstance(pi.PropertyType);
                        pi.SetValue(Application.Instance, ci, new object[] { });
                        log.DebugFormat("Created subcommand {0}.{1}", pi.DeclaringType.FullName, pi.Name);
                        goto created;
                    }
                }

                throw new InvalidDataException("CommandInstance cannot be null");

                created:

                log.DebugFormat("Created subcommand {0}.{1}", MemberInfo.DeclaringType.FullName, MemberInfo.Name);
                return ci;
            }
        }

        public static bool IsSubCommand(MemberInfo mi)
        {
            return mi.GetCustomAttribute<SubCommandAttribute>() != null;
        }
    }

}
