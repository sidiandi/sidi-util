using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace Sidi.Util
{
    /// <summary>
    /// Adds a nested diagnostic context to log4net for its lifetime
    /// </summary>
    public class LogScope : StopwatchLog
    {
        public LogScope(Action<object> logger, string text, params object[] parameters)
            : base(logger, text, parameters)
        {
            context = log4net.ThreadContext.Stacks["NDC"].Push(ContextString);
        }

        IDisposable context;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }

        // Use C# destructor syntax for finalization code.
        ~LogScope()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    }
}
