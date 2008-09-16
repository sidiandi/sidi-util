// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sidi.Net.Pop3
{
    /// <summary>
    /// Implements a POP3 server as specified in http://tools.ietf.org/html/rfc1939
    /// </summary>
    public class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;

        public delegate IMailBox MailboxProviderFunc(TcpClient connection, string user, string password);

        public MailboxProviderFunc MailboxProvider;

        public Server()
        {
        }

        public void Start()
        {
            Start(new TcpListener(IPAddress.Any, 110));
        }

        public void StartLoopback()
        {
            Start(new TcpListener(IPAddress.Loopback, 110));
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
            Session session = new Session(tcpClient);
            session.MailboxProvider = MailboxProvider;
            session.Run();
        }
    }
}
