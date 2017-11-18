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
    internal class GetOptOption
    {
        static public GetOptOption Create(object instance, MemberInfo memberInfo)
        {
            var usage = Sidi.CommandLine.Usage.Get(memberInfo);
            if (usage == null)
            {
                return null;
            }
            return new GetOptOption(instance, memberInfo, usage);
        }

        GetOptOption(object instance, MemberInfo memberInfo, string usage)
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
    }
}
