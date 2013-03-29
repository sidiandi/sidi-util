using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.CommandLine
{
    [Usage("Logging options")]
    public class LoggingOptions
    {
        [Usage("Logging level"), Persistent]
        public log4net.Core.Level Level { get; set; }

        [Usage("Choose Info log level")]
        public void Debug()
        {
            Level = log4net.Core.Level.Debug;
        }
        [Usage("Choose Info log level")]
        public void Info()
        {
            Level = log4net.Core.Level.Info;
        }
        [Usage("Choose Info log level")]
        public void Warn()
        {
            Level = log4net.Core.Level.Warn;
        }
        [Usage("Choose Info log level")]
        public void Error()
        {
            Level = log4net.Core.Level.Error;
        }
    }
}
