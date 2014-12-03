using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace Sidi.Util
{
    public class LogScope : IDisposable
    {
        public LogScope(Action<object> logger, string text, params object[] parameters)
        {
            this.logger = logger;
            this.stopwatch = Stopwatch.StartNew();

            this.contextString = String.Format(text, parameters);
            context = log4net.ThreadContext.Stacks["NDC"].Push(contextString);
            logger(String.Format("begin {0}", contextString));
        }

        IDisposable context;

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
                var msg = String.Format(CultureInfo.InvariantCulture, "completed in {0:F3}s: {1}", stopwatch.Elapsed.TotalSeconds, contextString);
                logger(msg);
                context.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~LogScope()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    

        Action<object> logger;
        string contextString;
        Stopwatch stopwatch;
    }
}
