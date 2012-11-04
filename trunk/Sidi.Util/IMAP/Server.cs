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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Sidi.IMAP
{
    public class Server
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TcpListener tcpListener;
        private Thread listenThread;

        public Server()
        {
        }

        public const int DefaultPort = 143;
        
        public void Start()
        {
            Start(new TcpListener(IPAddress.Any, DefaultPort));
        }

        public void StartLoopback()
        {
            Start(new TcpListener(IPAddress.Loopback, DefaultPort));
        }

        public void Start(TcpListener listener)
        {
            this.tcpListener = listener;
            this.tcpListener.Start();
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        public void Stop()
        {
            this.tcpListener.Stop();
        }

        private void ListenForClients()
        {
            while (true)
            {
                try
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //create a thread to handle communication
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
                    clientThread.Start(client);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void HandleClientCommunication(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            Session(tcpClient);
            tcpClient.Close();
        }

        protected virtual void Session(TcpClient tcpClient)
        {
            using (var session = new Session(tcpClient))
            {
                session.Run();
            }
        }
    }
}
