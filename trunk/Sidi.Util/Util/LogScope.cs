using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Sidi.Util
{
    public class LogScope : IDisposable
    {
        public LogScope(Action<object> logger, string text, params object[] parameters)
        {
            this.logger = logger;
            this.text = text;
            this.parameters = parameters;
            this.start = DateTime.Now;

            var msg = String.Format(text, parameters);
            msg = String.Format("begin: {0}", msg);
            logger(msg);
        }

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
                var msg = String.Format(text, parameters);
                msg = String.Format(CultureInfo.InvariantCulture, "completed in {0:F3}s: {1}", (DateTime.Now - start).TotalSeconds, msg);
                logger(msg);
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
        string text;
        object[] parameters;
        DateTime start;
    }
}
