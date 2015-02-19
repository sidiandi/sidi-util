using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sidi.Util
{
    /// <summary>
    /// http://en.wikipedia.org/wiki/Base32
    /// </summary>
    public class Base32
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string Encode(byte[] data)
        {
            var s = new StringWriter();
            int i = 0;
            for (; i < data.Length; i += 5)
            {
                EncodeBlock(s, data, i);
            }
            return s.ToString();
        }

        static void EncodeBlock(TextWriter s, byte[] data, int i)
        {
            int d = 0;
            int n = data.Length;

            if (i >= n)
            {
                return;
            }
            d = data[i] >> 3;
            s.Write(alphabet[d & 0x1f]);

            if (i + 1 >= n)
            {
                d = (data[i] << 2) | 0;
                s.Write(alphabet[d & 0x1f]);
                return;
            }
            d = (data[i] << 2) | (data[i + 1] >> 6);
            s.Write(alphabet[d & 0x1f]);
            d = data[i + 1] >> 1;
            s.Write(alphabet[d & 0x1f]);

            if (i + 2 >= n)
            {
                d = (data[i + 1] << 4) | (0);
                s.Write(alphabet[d & 0x1f]);
                return;
            }
            d = (data[i + 1] << 4) | (data[i + 2] >> 4);
            s.Write(alphabet[d & 0x1f]);

            if (i + 3 >= n)
            {
                d = (data[i + 2] << 1) | (0);
                s.Write(alphabet[d & 0x1f]);
                return;
            }
            d = (data[i + 2] << 1) | (data[i + 3] >> 7);  
            s.Write(alphabet[d & 0x1f]);
            d = (data[i + 3] >> 2);
            s.Write(alphabet[d & 0x1f]);

            if (i + 4 >= n)
            {
                d = (data[i + 3] << 3) | (0);
                s.Write(alphabet[d & 0x1f]);
                return;
            }
            d = (data[i + 3] << 3) | (data[i + 4] >> 5);
            s.Write(alphabet[d & 0x1f]);
            d = (data[i + 4]);
            s.Write(alphabet[d & 0x1f]);
        }
    }
}
