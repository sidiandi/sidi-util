﻿using System;
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
            get { return shared.connection; }
        }

        class Shared : IDisposable
        {
            public SQLiteConnection connection;
            int references = 0;

            public Shared(SQLiteConnection c)
            {
                connection = c;
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
        
        public void Dispose()
        {
            if (!shared.RemoveRef())
            {
                shared.Dispose();
            }
        }
    }
}