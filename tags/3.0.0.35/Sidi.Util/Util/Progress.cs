using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.Util
{
    public class Progress
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Progress(Action<string> log = null)
        {
            Begin = DateTime.Now;
            End = Begin;
            Total = 0.0d;
            Done = 0.0d;
            UpdateInterval = DefaultUpdateInterval;
            this.Log = log;
        }

        public static TimeSpan DefaultUpdateInterval = new TimeSpan(0, 0, 5);

        public DateTime Begin { get; set; }
        public double Total { get; set; }
        public double Done { set; get; }
        public DateTime End { get; set; }
        public TimeSpan UpdateInterval { get; set; }
        DateTime nextUpdate = DateTime.MinValue;
        Action<string> Log { set; get; }

        public bool Update(double done)
        {
            End = DateTime.Now;
            Done = done;
            if (End > nextUpdate)
            {
                if (Log != null)
                {
                    Log(this.ToString());
                }
                nextUpdate = End + UpdateInterval;
                return true;
            }
            else
            {
                return false;
            }
        }

        public double Percent
        { 
            get
            {
                return SafeDiv(Done, Total) * 100.0;
            }
        }

        public double Rate
        {
            get
            {
                return SafeDiv(Done, Duration.TotalSeconds);
            }
        }

        public TimeSpan Duration
        {
            get { return End - Begin; }
        }

        public TimeInterval Time { get { return new TimeInterval(Begin, End);  } }

        public TimeSpan RemainingTime
        {
            get
            {
                return SafeTimeSpanFromSeconds(SafeDiv(Total - Done, Rate));
            }
        }

        TimeSpan SafeTimeSpanFromSeconds(double sec)
        {
            if (sec < TimeSpan.MaxValue.TotalSeconds)
            {
                return TimeSpan.FromSeconds(sec);
            }
            else
            {
                return TimeSpan.MaxValue;
            }
        }

        public DateTime EstimatedTimeOfArrival
        {
            get
            {
                try
                {
                    return End + RemainingTime;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return DateTime.MaxValue;
                }
            }
        }

        /// <summary>
        /// Safe division
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <returns>n/d, 0 if d == 0</returns>
        public static double SafeDiv(double n, double d)
        {
            var r = n / d;
            if (Double.IsNaN(r) || Double.IsInfinity(r))
            {
                return 0;
            }
            else
            {
                return r;
            }
        }

        public override string ToString()
        {
            return String.Format(MetricPrefix.Instance, "{0:F1}% ({1:M}/{2:M}, rate={3:M}/s, rem={4}, eta={5})", Percent, Done, Total, Rate, RemainingTime, EstimatedTimeOfArrival);
        }
    }
}
