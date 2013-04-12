using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Repository.Hierarchy;
using log4net;
using System.ComponentModel;
using System.Reflection;
using log4net.Layout;
using log4net.Appender;

namespace Sidi.CommandLine
{
    [Usage("Adjust logging behavior")]
    public class LogOptions
    {
        [Usage("off, error, warn, info, debug, all")]
        public static log4net.Core.Level ParseLevel(string stringRepresentation)
        {
            var levels = typeof(log4net.Core.Level).GetFields(BindingFlags.Static | BindingFlags.Public);
            var selected = levels.First(i => Parser.IsMatch(stringRepresentation, i.Name));
            return (log4net.Core.Level)selected.GetValue(null);
        }

        [Usage("Log level (off, error, warn, info, debug, all)"), Persistent]
        [Category(Parser.categoryUserInterface)]
        public log4net.Core.Level LogLevel
        {
            get
            {
                var hierarchy = (Hierarchy)LogManager.GetRepository();
                return hierarchy.Root.Level;
            }

            set
            {
                ConfigureDefaultLogging();
                var hierarchy = (Hierarchy)LogManager.GetRepository();
                hierarchy.Root.Level = value;
            }
        }

        public static void ConfigureDefaultLogging()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            if (hierarchy.Configured)
            {
                return;
            }

            var pattern = new PatternLayout("%utcdate{ISO8601} [%thread] %level %logger %ndc - %message%newline");

            var ca = new ConsoleAppender()
            {
                Target = "Console.Error",
                Layout = pattern,
            };

            hierarchy.Root.AddAppender(ca);
            hierarchy.Configured = true;
        }


    }
}
