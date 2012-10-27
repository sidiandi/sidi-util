using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;

namespace Sidi.IMAP
{
    [TestFixture]
    public class ServerTest
    {
        [Test, Explicit("server")]
        public void Serve()
        {
            var server = new Sidi.IMAP.Server();
            server.StartLoopback();
            Thread.Sleep(TimeSpan.FromDays(1));
        }
    }
}   
