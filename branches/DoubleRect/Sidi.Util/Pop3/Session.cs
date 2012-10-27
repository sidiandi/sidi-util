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
using System.IO;
using Sidi.Util;
using Sidi.Extensions;
using log4net;

namespace Sidi.Net.Pop3
{
    public class PopException : Exception
    {
        public PopException(string msg)
        : base(msg)
        {
        }
    }

    public interface IMailBox
    {
        IList<IMailItem> Mails { get; }
        void Update(bool[] deleteFlags);
    }
    
    public interface IMailItem
    {
        string Content { get; }
        string Top(int lines);
        string Uid{ get;}
    }

    public class Session : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        TcpClient connection;
        NetworkStream stream;
        StreamWriter Out;
        StreamReader In;        

        readonly string positive = "+OK";
        readonly string negative = "-ERR";

        public Server.MailboxProviderFunc MailboxProvider;
        
        string user;
        string pass;
        IMailBox mailbox;
        bool[] deleteFlags;

        public IMailBox Mailbox
        {
            get { return mailbox; }
            set
            {
                mailbox = value;
                if (mailbox == null)
                {
                    deleteFlags = null;
                }
                else
                {
                    deleteFlags = new bool[Mails.Count];
                }
            }
        }
        
        public IList<IMailItem> Mails
        {
            get { return Mailbox.Mails; }
        }

        public Session(TcpClient tcpClient)
        {
            ndc = log4net.ThreadContext.Stacks["NDC"].Push(tcpClient.GetRemoteEndPoint().ToString());
            connection = tcpClient;
            stream = tcpClient.GetStream();
            Out = new StreamWriter(stream);
            Out.AutoFlush = true;
            In = new StreamReader(stream);

            commands["USER"] = this.USER;
            commands["PASS"] = this.PASS;
            commands["QUIT"] = this.Quit;
            commands["STAT"] = this.STAT;
            commands["LIST"] = this.LIST;
            commands["RETR"] = this.RETR;
            commands["DELE"] = this.DELE;
            commands["NOOP"] = this.NOOP;
            commands["RSET"] = this.RSET;
            commands["TOP"] = this.TOP;
            commands["UIDL"] = this.UIDL;
        }


        void USER(string[] args)
        {
            AssertState(State.AUTHORIZATION);
            user = args[1];
            Respond(true);
        }

        void PASS(string[] args)
        {
            AssertState(State.AUTHORIZATION);
            pass = args[1];
            Mailbox = MailboxProvider(connection, user, pass);
            if (Mailbox == null)
            {
                Respond(false);
                state = State.QUIT;
            }
            else
            {
                Respond(true);
                state = State.TRANSACTION;
            }
        }

        void Quit(string[] args)
        {
            switch (state)
            {
                case State.AUTHORIZATION:
                    state = State.QUIT;
                    Respond(true);
                    break;
                case State.TRANSACTION:
                    Respond(true);
                    state = State.UPDATE;
                    state = State.QUIT;
                    PerformUpdate();
                    break;
                default:
                    Respond(false);
                    break;
            }
        }

        void PerformUpdate()
        {
            Mailbox.Update(deleteFlags);
        }

         void STAT(string[] args)
         {
             AssertState(State.TRANSACTION);
             int count = Mails.Count;
             Respond(true, String.Format("{0} {1}", count, count * 1000));
         }

         public string ScanListing(int i)
         {
             return String.Format("{0} {1}", i, 1000);
         }

         void LIST(string[] args)
         {
             AssertState(State.TRANSACTION);
             if (args.Length > 1)
             {
                 int messageNumber = int.Parse(args[1]);
                 Respond(true, ScanListing(messageNumber));
             }
             else
             {
                 Respond(true);
                 for (int i=1; i <= Mails.Count; ++i)
                 {
                     Out.WriteLine(ScanListing(i));
                 }
                 TerminateMultiline();
             }
         }

         public string UidListing(int i)
         {
             return String.Format("{0} {1}", i, Mails[i-1].Uid);
         }

         void UIDL(string[] args)
         {
             AssertState(State.TRANSACTION);
             if (args.Length > 1)
             {
                 int messageNumber = int.Parse(args[1]);
                 Respond(true, UidListing(messageNumber));
             }
             else
             {
                 Respond(true);
                 for (int i = 1; i <= Mails.Count; ++i)
                 {
                     Out.WriteLine(UidListing(i));
                     log.Debug(UidListing(i));
                 }
                 TerminateMultiline();
             }
         }

         void TerminateMultiline()
         {
             Out.WriteLine(".");
             Out.Flush();
         }

         void RETR(string[] args)
         {
             AssertState(State.TRANSACTION);
             int messageNumber = int.Parse(args[1]);
             IMailItem i = Mails[messageNumber-1];
             MlResponse(true, String.Empty, i.Content);
         }

         void DELE(string[] args)
         {
             AssertState(State.TRANSACTION);
             int messageNumber = int.Parse(args[1]);
             deleteFlags[messageNumber-1] = true;
             Respond(true);
         }

         void NOOP(string[] args)
         {
             AssertState(State.TRANSACTION);
             Respond(true);
         }

         void RSET(string[] args)
         {
             AssertState(State.TRANSACTION);
             for (int i=0; i<deleteFlags.Length; ++i)
             {
                 deleteFlags[i] = false;
             }
             Respond(true);
         }

         void TOP(string[] args)
         {
             AssertState(State.TRANSACTION);
             int messageNumber = int.Parse(args[1]);
             int lines = int.Parse(args[2]);
             IMailItem item = Mails[messageNumber-1];
             MlResponse(true, String.Empty, item.Top(lines));
         }

        enum State
        {
            AUTHORIZATION,
            TRANSACTION,
            UPDATE,
            QUIT
        };

        State _state;

        State state
        {
            get { return _state; }
            set
            {
                if (log.IsDebugEnabled) log.DebugFormat("State change: {0} -> {1}", _state, value);
                _state = value;
            }
        }

        public void Run()
        {
            log.Info("Session begin");
            state = State.AUTHORIZATION;
            Respond(true, "POP3 server ready");
            while (state != State.QUIT)
            {
                ProcessCommand();
            }
            log.Info("Session end");
        }

        delegate void CommandHandler(string[] args);
        System.Collections.Generic.Dictionary<string, CommandHandler> commands = new System.Collections.Generic.Dictionary<string, CommandHandler>();

        void ProcessCommand()
        {
            try
            {
                string line = In.ReadLine();
                if (line == null)
                {
                    state = State.QUIT;
                    return;
                }

                if (line.StartsWith("PASS"))
                {
                    log.Debug("PASS <omitted>");
                }
                else
                {
                    log.Debug(line);
                }

                string[] args = System.Text.RegularExpressions.Regex.Split(line, "\\s+");
                string command = args[0];
                try
                {
                    commands[command](args);
                }
                catch (Exception e)
                {
                    log.Error("Error while processing " + line, e);
                    Respond(false, e.Message);
                }
            }
            catch (Exception e)
            {
                log.Error("error", e);
                state = State.QUIT;
            }
        }

        void Respond(bool ok, string response)
        {
            string line = String.Format("{0} {1}", ok ? positive : negative, response.OneLine(1024));
            Out.WriteLine(line);
            log.Debug(line);
            if (!ok)
            {
            }
        }

        void MlResponse(bool ok, string response, string data)
        {
            Respond(ok, response);
            Out.WriteLine(data);
            log.Debug(data);
            TerminateMultiline();
        }

        void Respond(bool ok)
        {
            Respond(ok, String.Empty);
        }

        void AssertState(State s)
        {
            if (state != s)
            {
                throw new PopException("Illegal command in this state.");
            }
        }

        IDisposable ndc;

        #region IDisposable Members

        public void Dispose()
        {
            if (ndc != null)
            {
                ndc.Dispose();
                ndc = null;
            }
        }

        #endregion
    }
}
