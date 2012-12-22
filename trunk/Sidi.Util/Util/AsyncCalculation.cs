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
using System.Threading;

namespace Sidi.Util
{
    public class AsyncCalculation<Input, Output>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Func<Input, Output> work;
        Input query = default(Input);
        Output result = default(Output);
        Thread worker = null;

        public AsyncCalculation(Func<Input, Output> work)
        {
            this.work = work;
        }
        
        public Input Query
        {
            set
            {
                lock (this)
                {
                    query = value;
                    Start();
                }
            }

            get
            {
                lock (this)
                {
                    return query;
                }
            }
        }

        public event EventHandler Complete;

        void Start()
        {
            if (worker != null)
            {
                worker.Abort();
                worker = null;
            }

            worker = new Thread(() =>
            {
                Input i = default(Input);
                try
                {
                    lock (this)
                    {
                        i = Query;
                    }
                    Output o = work(i);
                    lock (this)
                    {
                        worker = null;
                        result = o;

                        if (Complete != null)
                        {
                            Complete(this, EventArgs.Empty);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    log.InfoFormat("Worker with input {0} aborted.", i);
                    Thread.ResetAbort();
                }
            });

            worker.Start();
        }

        public Output Result
        {
            get
            {
                lock (this)
                {
                    return result;
                }
            }
        }

        public bool Busy
        {
            get
            {
                lock (this)
                {
                    return worker != null;
                }
            }
        }

        public void Wait()
        {
            Thread w;
            lock (this)
            {
                w = worker;
            }

            if (w != null)
            {
                w.Join();
            }
        }
    }
}
