using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public static class FileCompare
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool EqualByTimeAndLength(params LPath[] files)
        {
            return EqualByTimeAndLength(TimeSpan.Zero, files);
        }

        public static bool EqualByTimeAndLength(TimeSpan maxTimeDifference, params LPath[] files)
        {
            var info = files.Select(_ => _.Info).ToArray();

            if (!info.All(_ => _.IsFile))
            {
                return false;
            }

            var f = info.First();
            if (!info.Skip(1).All(_ => _.Length == f.Length))
            {
                return false;
            }

            var timeDifference = (info.Max(_ => _.LastWriteTimeUtc) - info.Min(_ => _.LastWriteTimeUtc));
            if (timeDifference <= maxTimeDifference)
            {
                return true;
            }
            else
            {
                log.Info(timeDifference);
                return false;
            }
        }

        static bool Equals(byte[] b1, byte[] b2, int count)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return Sidi.IO.Windows.NativeMethods.memcmp(b1, b2, count) == 0;
        }

        public static bool EqualByContent(LPath f1, LPath f2)
        {
            var i1 = f1.Info;
            var i2 = f2.Info;

            if (!i1.IsFile || !i2.IsFile)
            {
                return false;
            }

            const int bufSize = 0x1000;
            byte[] b1 = new byte[bufSize];
            byte[] b2 = new byte[bufSize];

            using (var s1 = f1.OpenRead())
            {
                using (var s2 = f2.OpenRead())
                {
                    int readCount = s1.Read(b1, 0, bufSize);
                    if (readCount != s2.Read(b2, 0, bufSize))
                    {
                        return false;
                    }

                    if (!Equals(b1, b2, readCount))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
