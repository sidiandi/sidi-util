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
    internal class Command
    {
        static public Command Create(object instance, MemberInfo memberInfo)
        {
            var attribute = memberInfo.GetCustomAttribute<Sidi.CommandLine.SubCommandAttribute>();
            if (attribute == null)
            {
                return null;
            }
            return new Command(instance, memberInfo);
        }

        Command(object instance, MemberInfo memberInfo)
        {
            this.instance = instance;
            this.memberInfo = memberInfo;
            this.LongOption = Option.CSharpIdentifierToLongOption(memberInfo.Name);
            this.usage = Usage.Get(ModuleType);
        }

        Type ModuleType
        {
            get
            {
                if (memberInfo is FieldInfo)
                {
                    return ((FieldInfo)memberInfo).FieldType;
                }

                if (memberInfo is PropertyInfo)
                {
                    return ((PropertyInfo)memberInfo).PropertyType;
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        public readonly object instance;
        public readonly MemberInfo memberInfo;
        public readonly string usage;

        public string LongOption { get; set; }

        public string ShortOption { get; set; }

        public override string ToString()
        {
            return LongOption;
        }

        public static IEnumerable<Command> Get(object module)
        {
            return module.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(m => Command.Create(module, m))
                .Where(_ => _ != null)
                .ToList();
        }

        internal static IEnumerable<Command> Get(IEnumerable<object> modules)
        {
            var commands = modules.SelectMany(Get).ToList();
            return commands;
        }
    }
}
