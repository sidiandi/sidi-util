using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Util
{
    /// <summary>
    /// Executes an action on disposal
    /// </summary>
    public sealed class OnDispose : IDisposable
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="action">Action that will be executed on disposal of the created instance.</param>
        public OnDispose(Action action)
        {
            this.action = action;
        }

        Action action;
        bool disposed;

        /// <summary>
        /// Will execute the action that was specified in the constructor.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                action();
                disposed = true;
            }
        }
    }
}
