using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public class CopyFileProgress
    {
        public CopyFileProgress(
            DateTime Begin,
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            int StreamNumber)
        {
            this.Begin = Begin;
            this.TotalFileSize = TotalFileSize;
            this.TotalBytesTransferred = TotalBytesTransferred;
            this.StreamSize = TotalBytesTransferred;
            this.StreamBytesTransferred = StreamBytesTransferred;
            this.StreamNumber = StreamNumber;
        }

        public DateTime Begin { get; private set; }
        public long TotalFileSize { get; private set; }
        public long TotalBytesTransferred { get; private set; }
        public long StreamSize { get; private set; }
        public long StreamBytesTransferred { get; private set; }
        public int StreamNumber { get; private set; }

        public override string ToString()
        {
            return new Sidi.Util.Progress
            {
                Begin = Begin,
                End = DateTime.Now,
                Done = TotalBytesTransferred,
                Total = TotalFileSize,
            }.ToString();
        }
    }

}
