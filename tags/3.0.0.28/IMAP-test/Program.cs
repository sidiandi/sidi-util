// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;

namespace IMAP_test
{
    class Program
    {
        static void Main()
        {
            log4net.Config.BasicConfigurator.Configure();
            var server = new Sidi.Imap.Server();
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
