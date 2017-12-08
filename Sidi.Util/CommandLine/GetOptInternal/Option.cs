using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sidi.CommandLine.GetOptInternal
{
    internal class Option
    {
        static public Option Create(object instance, MemberInfo memberInfo)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            var usage = Sidi.CommandLine.Usage.Get(memberInfo);
            if (usage == null)
            {
                return null;
            }

            if (IsStatic(memberInfo))
            {
                throw new CommandLineException("[Usage] attribute cannot be applied to static members.");
            }

            return new Option(instance, memberInfo, usage);
        }

        static bool IsStatic(MemberInfo m)
        {
            if (m is FieldInfo)
            {
                var f = (FieldInfo)m;
                return f.IsStatic;
            }
            if (m is PropertyInfo)
            {
                return true;
            }
            if (m is MethodInfo)
            {
                var method = (MethodInfo)m;
                return method.IsStatic;
            }
            throw new ArgumentOutOfRangeException("m", m, "Cannot handle this type");
        }

        Option(object instance, MemberInfo memberInfo, string usage)
        {
            this.instance = instance;
            this.memberInfo = memberInfo;
            this.usage = usage;
            this.LongOption = CSharpIdentifierToLongOption(memberInfo.Name);
        }

        static internal string CSharpIdentifierToLongOption(string csharpIdentifier)
        {
            var o = new StringWriter();
            int i = 0;
            for (; i < csharpIdentifier.Length && i < 1; ++i)
            {
                o.Write(char.ToLower(csharpIdentifier[i]));
            }
            for (; i<csharpIdentifier.Length;++i)
            {
                if (char.IsLower(csharpIdentifier[i-1]) && char.IsUpper(csharpIdentifier[i]))
                {
                    o.Write("-");
                }
                o.Write(char.ToLower(csharpIdentifier[i]));
            }
            return o.ToString();
        }

        public readonly object instance;
        public readonly MemberInfo memberInfo;
        public readonly string usage;

        public string LongOption { get; set; }

        public string ShortOption { get; set; }

        public override string ToString()
        {
            return "--" + LongOption;
        }

        public static IEnumerable<Option> Get(object module)
        {
            return module.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(m => Option.Create(module, m))
                .Where(_ => _ != null)
                .Where(_ => !_.memberInfo.Name.Equals("ProcessArguments"))
                .ToList();
        }
        public static IEnumerable<Option> Get(IEnumerable<object> modules)
        {
            var options = modules.SelectMany(Get).ToList();
            return options;
        }
    }
}
