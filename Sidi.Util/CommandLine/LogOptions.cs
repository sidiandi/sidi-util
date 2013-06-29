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
using Sidi.IO;

namespace Sidi.CommandLine
{
    [Usage("Adjust logging behavior")]
    public class LogOptions
    {
        public LogOptions(Parser parser)
        {
            this.parser = parser;
        }

        Parser parser;

        [Usage("Log level. Determines the verbosity of logging output.")]
        [Example("off")]
        [Example("error")]
        [Example("warn")]
        [Example("info")]
        [Example("debug")]
        [Example("all")]
        public static log4net.Core.Level ParseLevel(string stringRepresentation)
        {
            var levels = typeof(log4net.Core.Level).GetFields(BindingFlags.Static | BindingFlags.Public);
            var selected = levels.First(i => Parser.IsMatch(stringRepresentation, i.Name));
            return (log4net.Core.Level)selected.GetValue(null);
        }

        [Usage("Determines the verbosity of logging output (off, error, warn, info, debug, all)"), Persistent]
        [Category(Parser.categoryLogging)]
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

        static string logFileAppenderName = "LogFile";
        
        [Usage("Write a log file"), Persistent]
        [Category(Parser.categoryLogging)]
        public bool LogFile
        {
            get
            {
                var hierarchy = (Hierarchy)LogManager.GetRepository();
                return hierarchy.Root.GetAppender(logFileAppenderName) != null;
            }

            set
            {
                ConfigureDefaultLogging();
                var hierarchy = (Hierarchy)LogManager.GetRepository();
                var lf = hierarchy.Root.GetAppender(logFileAppenderName);
                if (value)
                {
                    if (lf == null)
                    {
                        var appType = this.parser.StartupApplication.GetType();
                        var file = Paths.Temp.CatDir(
                                LPath.GetValidFilename(appType.FullName),
                                LPath.GetValidFilename(String.Format("{1}_log_{0}.txt", DateTime.Now.ToString("o"), appType.Name)));
                        file.EnsureParentDirectoryExists();

                        var newLf = new FileAppender()
                        {
                            AppendToFile = false,
                            File = file,
                            Layout = pattern,
                            Name = logFileAppenderName
                        };

                        newLf.ActivateOptions();

                        hierarchy.Root.AddAppender(newLf);
                        
                    }
                }
                else
                {
                    if (lf != null)
                    {
                        hierarchy.Root.RemoveAppender(lf);
                    }
                }
            }
        }

        static log4net.Layout.ILayout pattern = new PatternLayout("%utcdate{ISO8601} [%thread] %level %logger %ndc - %message%newline");
        
        public static void ConfigureDefaultLogging()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            if (hierarchy.Configured)
            {
                return;
            }

            var ca = new ConsoleAppender()
            {
                Target = "Console.Error",
                Layout = pattern,
            };

            hierarchy.Root.AddAppender(ca);
            hierarchy.Configured = true;
        }

        [Usage("Reset log options to defaults")]
        [Category(Parser.categoryLogging)]
        public void LogReset()
        {
            LogLevel = log4net.Core.Level.Info;
            LogFile = false;
        }
    }
}
