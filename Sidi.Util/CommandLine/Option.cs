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

        public object Handle(IList<string> args, bool execute)
        {
            if (args.Count < 1)
            {
                throw new CommandLineException("value missing for option {0}".F(this.Name));
            }

            if (MemberInfo is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)MemberInfo;
                object v = Parser.ParseValue(args[0], fi.FieldType);
                if (execute) fi.SetValue(Application, v);
                args.RemoveAt(0);
                if (IsPassword)
                {
                    return null;
                }
                else
                {
                    var r = fi.GetValue(Application);
                    log.InfoFormat("Option {0} = {1}", fi.Name, r);
                    return null;
                }
            }
            else if (MemberInfo is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)MemberInfo;
                object v = Parser.ParseValue(args[0], pi.PropertyType);
                if (execute) pi.SetValue(Application, v, new object[] { });
                args.RemoveAt(0);
                if (IsPassword)
                {
                    return null;
                }
                else
                {
                    var r = pi.GetValue(Application, new object[] { });
                    log.InfoFormat("Option {0} = {1}", pi.Name, r);
                    return null;
                }
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
                    "default: {0}".F(IsPassword ? "-- hidden --" : GetValue()).Indent(indent),
                    Usage.Indent(indent)
                    );
            }
        }

        public bool IsPersistent
        {
            get
            {
                return MemberInfo.GetCustomAttributes(typeof(PersistentAttribute), true).Any();
            }
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
