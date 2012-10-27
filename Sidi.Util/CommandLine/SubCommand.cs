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

        public SubCommand(object application, MemberInfo memberInfo)
        {
            Application = application;
            MemberInfo = memberInfo;
        }

        public object Application { get; private set; }
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
                throw new InvalidOperationException(i.GetType().ToString());
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
            var parser = new Parser(CommandInstance);
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
                        parser.ParseSingleCommand(args);
                    }
                    else
                    {
                        parser.CheckSingleCommand(args);
                    }
                }
            }
            else
            {
                parser.ShowUsage();
            }
            return null;
        }

        public object CommandInstance
        {
            get
            {
                var i = GetValue();
                if (i == null)
                {
                    if (MemberInfo is FieldInfo)
                    {
                        var fi = (FieldInfo)MemberInfo;
                        i = Activator.CreateInstance(fi.FieldType);
                        fi.SetValue(Application, i);
                    }
                    else if (MemberInfo is PropertyInfo)
                    {
                        var pi = (PropertyInfo)MemberInfo;
                        i = Activator.CreateInstance(pi.PropertyType);

                        pi.SetValue(Application, i, new object[] { });
                    }
                }
                return i;
            }
        }

        public string UsageText
        {
            get
            {
                string indent = "  ";
                return String.Format(
                    Parser.CultureInfo,
                    "{0}\r\n{1}",
                    Name,
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

        public static bool IsSubCommand(MemberInfo mi)
        {
            return mi.GetCustomAttributes(typeof(SubCommandAttribute), true).Any();
        }
    }

}
