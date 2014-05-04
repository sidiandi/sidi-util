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

        MemberInfo memberInfo;
        Parser parentParser;
        bool needLoadPreferences = false;

        bool IsInitialized
        {
            get
            {
                return _parser != null;
            }
        }

        public Parser Parser
        {
            get
            {
                if (_parser == null)
                {
                    // get member instance
                    object instance = null;
                    bool instanceCreated = false;

                    if (IsMemberInitialized)
                    {
                        instance = memberInfo.GetValue(Source.Instance);
                    }
                    else
                    {
                        // auto create instance
                        log.DebugFormat("auto-create of {0} instance", memberInfo);
                        instance =  Activator.CreateInstance(memberInfo.GetMemberType());
                        memberInfo.SetValue(Source.Instance, instance);
                        instanceCreated = true;
                    }

                    // create parser
                    _parser = new Parser()
                    {
                        Parent = this.parentParser,
                        Profile = parentParser.Profile,
                    };
                    _parser.ItemSources.Add(new ItemSource(instance));
                    _parser.AddDefaultUserInterfaceForSubCommands();
                    var persistent = this.memberInfo.GetCustomAttribute<PersistentAttribute>();
                    if (persistent != null && persistent.Global)
                    {
                        _parser.PreferencesKey = RegistryExtension.Cat(
                            this.memberInfo.GetMemberType().Assembly.GetRegistryKeyUserSoftware(),
                            _parser.Profile,
                            this.Name);
                    }
                    else
                    {
                        _parser.PreferencesKey = parentParser.PreferencesKey + "\\" + this.Name;
                    }                    
                    if (instanceCreated && needLoadPreferences)
                    {
                        LoadPreferences();
                    }
                }
                return _parser;
            }
        }
        Parser _parser;

        public SubCommand(Parser parser, ItemSource source, MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
            this.Source = source;
            this.parentParser = parser;
        }

        bool IsMemberInitialized
        {
            get
            {
                return memberInfo.GetValue(Source.Instance) != null;
            }
        }

        public bool IsPersistent
        {
            get
            {
                return memberInfo.GetCustomAttribute<PersistentAttribute>() != null;
            }
        }

        public void StorePreferences()
        {
            if (!IsPersistent) { return; }

            if (IsMemberInitialized)
            {
                Parser.StorePreferences();
            }
        }

        public void ClearPreferences()
        {
            if (!IsPersistent) { return; }

            if (IsMemberInitialized)
            {
                Parser.ClearPreferences();
            }
        }

        public void LoadPreferences()
        {
            if (!IsPersistent) { return; }

            if (IsMemberInitialized)
            {
                Parser.LoadPreferences();
            }
            else
            {
                needLoadPreferences = true;
            }
        }

        public static bool IsSubCommand(MemberInfo member)
        {
            return (member is FieldInfo || member is PropertyInfo) &&
                member.GetCustomAttribute<SubCommandAttribute>() != null;
        }

        public string Usage
        {
            get
            {
                var usage = Sidi.CommandLine.Usage.Get(memberInfo);
                if (usage != null)
                {
                    return usage;
                }

                usage = Sidi.CommandLine.Usage.Get(memberInfo.GetMemberType());
                if (usage != null)
                {
                    return usage;
                }

                return "No usage text for {0}".F(memberInfo);
            }
        }

        public string UsageText
        {
            get
            {
                return String.Format("{0} [subcommands...] {1}\r\n{2}",
                    Name,
                    Parser.ListTerminator,
                    Usage.Indent("  "));
            }
        }

        public string Name
        {
            get
            {
                return memberInfo.Name;
            }
        }

        public string Syntax
        {
            get
            {
                return String.Format("{0} [subcommands...] {1}",
                    Name, Parser.ListTerminator);
            }
        }

        public ItemSource Source { get; private set; }

        public IEnumerable<string> Categories
        {
            get
            {
                var a = memberInfo.GetCustomAttributes<CategoryAttribute>().Select(x => x.Category);
                return a.Any() ? a : new[] { String.Empty };
            }
        }

        public object Handle(IList<string> args, bool execute)
        {
            if (!args.Any())
            {
                Parser.ShowUsage();
                return null;
            }

            for (; args.Any(); )
            {
                if (args.First().Equals(Parser.ListTerminator))
                {
                    args.PopHead();
                    break;
                }
                Parser.ParseSingleCommand(args);
            }
            return null;
        }
    }
}
