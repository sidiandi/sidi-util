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
                        instance = memberInfo.GetValue(Application.Instance);
                    }
                    else
                    {
                        // auto create instance
                        log.DebugFormat("auto-create of {0} instance", memberInfo);
                        instance =  Activator.CreateInstance(memberInfo.GetMemberType());
                        memberInfo.SetValue(Application.Instance, instance);
                        instanceCreated = true;
                    }

                    // create parser
                    _parser = new Parser()
                    {
                        Parent = this.parentParser
                    };
                    _parser.Applications.Add(new Application(instance));

                    if (instanceCreated && needLoadPreferences)
                    {
                        LoadPreferences();
                    }
                }
                return _parser;
            }
        }
        Parser _parser;

        public SubCommand(Parser parser, Application application, MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
            this.Application = application;
            this.parentParser = parser;
        }

        bool IsMemberInitialized
        {
            get
            {
                return memberInfo.GetValue(Application.Instance) != null;
            }
        }

        public void StorePreferences()
        {
            if (IsMemberInitialized)
            {
                Parser.StorePreferences();
            }
        }

        public void LoadPreferences()
        {
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

                throw new InvalidDataException("No usage text for {0}".F(memberInfo));
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
            get { throw new NotImplementedException(); }
        }

        public Application Application { get; private set; }

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
