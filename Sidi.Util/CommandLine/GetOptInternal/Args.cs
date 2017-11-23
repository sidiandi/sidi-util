using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Sidi.CommandLine.GetOptInternal
{
    internal class Args : IEnumerator<string>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Args(string[] args)
        {
            this.args = args;
            this.i = -1;
        }
        public readonly string[] args;
        public int i;

        public string Current { get { return args[i]; } }

        public string Next { get { return args[i + 1]; } }

        object IEnumerator.Current => args[i];

        public void Dispose()
        {
        }

        public bool HasNext => i < args.Length - 1;

        public string InlineParameter { get; set; }

        public bool MoveNext()
        {
            try
            {
                if (HasNext)
                {
                    ++i;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(Sidi.Extensions.StringExtensions.ToString(Print));
                }
            }
        }

        public void Reset()
        {
            i = -1;
        }

        public void Print(TextWriter w)
        {
            w.WriteLine("Arguments");
            w.WriteLine("----");
            for (int j = 0; j<args.Length; ++j)
            {
                w.WriteLine("{0} {1}",
                    i == j ? "=>" : "  ",
                    args[j]);
            }
            w.WriteLine("----");
        }
    }
}
