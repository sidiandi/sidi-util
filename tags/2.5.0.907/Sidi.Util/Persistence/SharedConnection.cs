// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Data.SQLite;
using System.Threading;
using System.IO;

namespace Sidi.Persistence
{
    public class SharedConnection : IDisposable
    {
        public SharedConnection(SQLiteConnection c)
        {
            shared = new Shared(c);
            shared.AddRef();
        }

        public SharedConnection(SharedConnection c)
        {
            c.shared.AddRef();
            shared = c.shared;
        }

        public SQLiteConnection Connection
        {
            get
            {
                return shared.Connection;
            }
        }

        class Shared : IDisposable
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            SQLiteConnection connection;
            int clientThreadId;

            int references = 0;

            public Shared(SQLiteConnection c)
            {
                connection = c;
            }

            public SQLiteConnection Connection
            {
                get
                {
                    if (clientThreadId == 0)
                    {
                        clientThreadId = Thread.CurrentThread.ManagedThreadId;
                    }
                    else
                    {
                        if (clientThreadId != Thread.CurrentThread.ManagedThreadId)
                        {
                            throw new Exception(String.Format("Multi-Threaded use of {0}", connection.ConnectionString));
                        }
                    }
                    return connection;
                }
            }

            public void AddRef()
            {
                lock (this)
                {
                    ++references;
                }
            }

            public bool RemoveRef()
            {
                lock (this)
                {
                    --references;
                    return references > 0;
                }
            }

            public void Dispose()
            {
                Close();
            }

            void Close()
            {
                connection.Dispose();
                /*
                var csb = new SQLiteConnectionStringBuilder(connection.ConnectionString);
                var dbFile = csb.DataSource;
                connection.Dispose();
                GC.Collect();
                for (int i = 0; i < 10; ++i)
                {
                    try
                    {
                        using (var f = File.OpenWrite(dbFile))
                        {
                        }
                        break;
                    }
                    catch
                    {
                    }
                    Thread.Sleep(100);
                }
                 */
            }
        }
        Shared shared;



        private bool disposed = false;

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (!shared.RemoveRef())
                    {
                        shared.Dispose();
                    }

                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~SharedConnection()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

    }
}
