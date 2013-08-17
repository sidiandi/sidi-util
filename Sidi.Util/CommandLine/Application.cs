using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sidi.CommandLine
{
    public class Application
    {
        public Application(object instance)
        {
            this.Instance = instance;
        }

        public object Instance { get; private set; }
        public bool? ProgramSpecificPreferences { get; set; }

        /// <summary>
        /// All available actions and options
        /// </summary>
        public IEnumerable<IParserItem> FindItems(Parser parser)
        {
            foreach (MethodInfo i in Instance.GetType().GetMethods())
            {
                if (ValueParser.IsSuitable(i))
                {
                    // yield return new ValueParser(this, application, i);
                }
                else
                {
                    string u = Usage.Get(i);
                    string parameters = String.Join(" ", Array.ConvertAll(i.GetParameters(), new Converter<ParameterInfo, string>(delegate(ParameterInfo pi)
                    {
                        return String.Format("[{1} {0}]", pi.Name, pi.ParameterType.GetInfo());
                    })));
                    if (u != null)
                    {
                        yield return new Action(parser, this, i);
                    }
                }
            }

            foreach (MemberInfo i in Instance.GetType().GetMembers())
            {
                if (SubCommand.IsSubCommand(i))
                {
                    if ((i is FieldInfo || i is PropertyInfo))
                    {
                        var subCommand = new SubCommand(parser, this, i);
                        yield return subCommand;
                    }
                }
                else
                {
                    string u = Usage.Get(i);
                    if ((i is FieldInfo || i is PropertyInfo) && u != null)
                    {
                        yield return new Option(parser, this, i);
                    }
                }
            }
        }

        /// <summary>
        /// All value parsers
        /// </summary>
        public IList<ValueParser> GetValueParsers(Parser parser)
        {
            return Instance.GetType().GetMethods()
                .Where(i => ValueParser.IsSuitable(i))
                .Select(i => new ValueParser(parser, this, i))
                .ToList();
        }
    }
}
