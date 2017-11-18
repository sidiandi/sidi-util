using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Linq;

namespace Sidi.CommandLine.GetOptInternal
{
    internal class Logging
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly private GetOpt o;

        public Logging(GetOpt o)
        {
            this.o = o;
        }

        [Usage("More verbose logging. Can be used multiple times.")]
        public void Verbose()
        {
            o.Verbose();
        }
    }
}