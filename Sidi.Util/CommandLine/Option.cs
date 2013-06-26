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
    public class Option : IParserItem
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Option(Parser parser, object application, MemberInfo memberInfo)
        {
            Application = application;
            MemberInfo = memberInfo;
            this.parser = parser;
            parser.GetValueParser(Type);
        }

        Parser parser;

        public object Application { get; private set; }
        public MemberInfo MemberInfo { get; private set; }

        public string Name { get { return MemberInfo.Name; } }
        public string Usage { get { return Sidi.CommandLine.Usage.Get(MemberInfo); } }

        public override string ToString()
        {
            return Name;
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
                MemberInfo i = MemberInfo;
                return String.Format(Parser.CultureInfo, "{0} [{1}]", Name, Type.GetInfo());
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
                throw new InvalidOperationException(i.MemberType.ToString());
            }
        }

        public IValueParser ValueParser
        {
            get
            {
                return parser.GetValueParser(Type);
            }
        }

        public void PrintScriptFileSample(TextWriter w)
        {
            w.Write("# "); w.WriteLine(Usage);
            w.Write("# "); w.WriteLine(Syntax);
            w.WriteLine();
        }

        public object Handle(IList<string> args, bool execute)
        {
            if (args.Count < 1)
            {
                throw new CommandLineException("value missing for option {0}".F(this.Name));
            }

            if (MemberInfo is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)MemberInfo;
                object v = parser.ParseValue(args, fi.FieldType);
                if (execute) fi.SetValue(Application, v);
                log.InfoFormat("Option {0} = {1}", fi.Name, DisplayValue);
                return null;
            }
            else if (MemberInfo is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)MemberInfo;
                object v = parser.ParseValue(args, pi.PropertyType);
                if (execute) pi.SetValue(Application, v, new object[] { });
                log.InfoFormat("Option {0} = {1}", pi.Name, DisplayValue);
                return null;
            }
            else
            {
                throw new CommandLineException("option is of invalid type: {0}".F(MemberInfo));
            }
        }

        public string DisplayValue
        {
            get
            {
                return IsPassword ? "-- hidden --" : GetValue().SafeToString();
            }
        }

        public string UsageText
        {
            get
            {
                string indent = "  ";
                using (var w = new StringWriter())
                {
                    w.WriteLine(@"{0} [{1}]", Name, Type.GetInfo());
                    w.Write("{0}default: {1}", indent, DisplayValue);
                    if (this.IsPersistent)
                    {
                        w.Write(" (persistent)");
                    }
                    w.WriteLine();
                    w.WriteLine(Usage.Indent(indent));
                    return w.ToString();
                }
            }
        }

        public bool IsPersistent
        {
            get
            {
                return MemberInfo.GetCustomAttributes(typeof(PersistentAttribute), true).Any();
            }
        }

        public PersistentAttribute GetPersistentAttribute()
        {
            return MemberInfo.GetCustomAttribute<PersistentAttribute>();
        }

        public bool IsPassword
        {
            get
            {
                return MemberInfo.GetCustomAttributes(typeof(PasswordAttribute), true).Any();
            }
        }
    }

}
