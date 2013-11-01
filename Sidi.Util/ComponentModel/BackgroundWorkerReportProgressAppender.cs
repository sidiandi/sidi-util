using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using System.ComponentModel;
using Sidi.Util;
using log4net.Filter;

namespace Sidi.ComponentModel
{
    /// <summary>
    /// Fires a BackgroundWorker.ReportProgress event for every log4net log event made 
    /// in the constructor thread until disposed.
    /// </summary>
    public class BackgroundWorkerReportProgressAppender : AppenderSkeleton, IDisposable
    {
        public BackgroundWorkerReportProgressAppender(BackgroundWorker bgw)
        {
            this.backgroundWorker = bgw;
            originalThreadContextProperty = log4net.ThreadContext.Properties[this.GetType().FullName];
            propertyName = this.GetType().FullName;
            log4net.ThreadContext.Properties[propertyName] = this;
            AddFilter(new Filter(this));

            this.Layout = new log4net.Layout.SimpleLayout();
            this.ActivateOptions();

            var hierarchy = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository());
            hierarchy.Root.AddAppender(this);
        }

        string propertyName;

        class Filter : FilterSkeleton
        {
            public Filter(BackgroundWorkerReportProgressAppender appender)
            {
                this.appender = appender;
            }

            BackgroundWorkerReportProgressAppender appender;

            public override FilterDecision Decide(log4net.Core.LoggingEvent loggingEvent)
            {
                var appenderProperty = log4net.ThreadContext.Properties[appender.propertyName];
                var allow = appenderProperty == appender;
                return allow ? FilterDecision.Accept : FilterDecision.Deny;
            }
        }

        BackgroundWorker backgroundWorker;
        object originalThreadContextProperty = null;
        int logCount = 0;

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            ++logCount;
            backgroundWorker.ReportProgress(logCount, this.RenderLoggingEvent(loggingEvent));
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
                log4net.ThreadContext.Properties[propertyName] = originalThreadContextProperty;
                var hierarchy = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository());
                hierarchy.Root.RemoveAppender(this);
                disposed = true;
            }
        }

        ~BackgroundWorkerReportProgressAppender()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
    }
}
