// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace Sidi.Util
{
    /// <summary>
    /// Logs begin and end of its lifetime and the total lifetime
    /// </summary>
    public class StopwatchLog : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StopwatchLog(Action<object> logger, string text, params object[] parameters)
        {
            this.stopwatch = Stopwatch.StartNew();
            this.logger = logger;
            ContextString = String.Format(text, parameters);
            logger(String.Format("begin {0}", ContextString));
        }

        public string ContextString { get; private set; }

        private bool disposed = false;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    stopwatch.Stop();
                    var msg = String.Format(CultureInfo.InvariantCulture, "completed in {0:F3}s: {1}", stopwatch.Elapsed.TotalSeconds, ContextString);
                    logger(msg);
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~StopwatchLog()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }


        Action<object> logger;
        Stopwatch stopwatch;
    }
}

