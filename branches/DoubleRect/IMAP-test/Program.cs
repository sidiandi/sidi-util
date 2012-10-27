using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace IMAP_test
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
            var server = new Sidi.IMAP.Server();
            /*
            server.Start(new System.Net.Sockets.TcpListener(
                    Dns.GetHostEntry(System.Environment.MachineName).AddressList.First(),
                    Sidi.IMAP.Server.DefaultPort));
             */
            server.StartLoopback();
            Console.ReadLine();
            server.Stop();
        }
    }
}
