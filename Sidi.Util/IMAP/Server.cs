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
            Session session = new Session(tcpClient);
            session.Run();
        }
    }
}
