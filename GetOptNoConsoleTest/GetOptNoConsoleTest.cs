using NUnit.Framework;
using Sidi.CommandLine;
using Sidi.IO;
using System;
using System.IO;

namespace Sidi.np
{

    [Usage("Does nothing")]
    [TestFixture]
    public class np : IArgumentHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static int Main(string[] args)
        {
            return GetOpt.Run(new np(), args);
        }

        public void ProcessArguments(string[] args)
        {
        }

        [Test]
        public void Nothing()
        {
        }
    }
}