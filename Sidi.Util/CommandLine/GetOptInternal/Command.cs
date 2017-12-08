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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            this.usage = Sidi.CommandLine.Usage.Get(ModuleType);
        }

        Type ModuleType
        {
            get
            {
                Type t = null;
                if (memberInfo is FieldInfo)
                {
                    t = ((FieldInfo)memberInfo).FieldType;
                }

                if (memberInfo is PropertyInfo)
                {
                    t = ((PropertyInfo)memberInfo).PropertyType;
                }

                t = HandleLazy(t);

                if (t != null) return t;

                throw new ArgumentOutOfRangeException();
            }
        }

        Type HandleLazy(Type moduleType)
        {
            if (moduleType.IsGenericType && moduleType.Name.StartsWith("Lazy"))
            {
                return moduleType.GetGenericArguments()[0];
            }

            return moduleType;
        }

        internal void Invoke(Args args)
        {
            log.InfoFormat("command {0}", this);
            var commandModule = GetModule();
            var getOpt = new GetOpt();
            getOpt.modules.Add(commandModule);
            getOpt.AddDefaultModules();
            getOpt.ProgramName = getOpt.ProgramName + " " + LongOption;
            getOpt.RunCommand(args);
        }

        object GetModule()
        {
            object module = null;
            if (memberInfo is FieldInfo)
            {
                var field = (FieldInfo)memberInfo;
                module = GetOpt.ProvideValue(instance, field);
            }
            else if (memberInfo is PropertyInfo)
            {
                var property = (PropertyInfo)memberInfo;
                module = GetOpt.ProvideValue(instance, property);
            }

            module = HandleLazyValue(module);

            if (module != null) return module;

            throw new ArgumentOutOfRangeException("command.memberInfo", memberInfo, "Cannot handle this memberInfo");
        }

        static object HandleLazyValue(object x)
        {
            if (x == null) return null;

            var moduleType = x.GetType();

            if (moduleType.IsGenericType && moduleType.Name.StartsWith("Lazy"))
            {
                return moduleType.GetProperty("Value").GetValue(x);
            }

            return x;
        }

        readonly object instance;
        readonly MemberInfo memberInfo;
        readonly string usage;

        public string Usage { get { return usage;  } }

        public string LongOption { get; set; }

        public string ShortOption { get; set; }

        public override string ToString()
        {
            return LongOption;
        }

        static IEnumerable<Command> Get(object module)
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
