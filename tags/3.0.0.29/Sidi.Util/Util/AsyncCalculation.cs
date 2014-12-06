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
using System.Threading.Tasks;

namespace Sidi.Util
{
    public class AsyncCalculation<Input, Output>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Func<Input, Output> work;
        Input query = default(Input);
        bool newQuery = false;
        Output result = default(Output);
        Task worker = null;

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
                    newQuery = true;
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
            if (worker == null || worker.IsCompleted)
            {
                worker = Task.Factory.StartNew(() =>
                {
                    lock (this)
                    {
                        while (newQuery)
                        {
                            var q = Query;
                            newQuery = false;

                            Monitor.Exit(this);
                            Output r = default(Output);
                            try
                            {
                                r = work(q);
                            }
                            catch
                            {
                            }
                            finally
                            {
                                Monitor.Enter(this);
                            }

                            if (!newQuery)
                            {
                                this.result = r;
                                if (Complete != null)
                                {
                                    Complete(this, EventArgs.Empty);
                                }
                            }
                        }
                    }
                });
            }
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
            Task w = null;
            lock (this)
            {
                w = worker;
            }
            if (w != null)
            {
                w.Wait();
            }
        }
    }
}
