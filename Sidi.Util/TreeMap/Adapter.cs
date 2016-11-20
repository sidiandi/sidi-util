// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sidi.TreeMap
{
    public class TreeEventArgs<T> : EventArgs
    {
        public TreeEventArgs(ITree<T> tree)
        {
            this.Tree = tree;
        }

        public ITree<T> Tree { get; private set; }
    }

    /// <summary>
    /// Adapts a generic treemap view to an ITree&lt;T&gt;
    /// Obtain via View.GetAdapter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Adapter<T> : IDisposable
    {
        View view;

        public Adapter(View view)
        {
            this.view = view;
            view.Activate += View_Activate;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    view.Activate -= View_Activate;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        private void View_Activate(object sender, TreeEventArgs e)
        {
            if (Activate != null)
            {
                Activate(this.view, new TreeEventArgs<T>((ITree<T>)e.Tree));
            }
        }

        public Func<ITree<T>, Color> GetColor
        {
            set
            {
                view.GetColor = _ => value((ITree<T>)_);
            }
        }

        public Func<ITree<T>, string> GetLabel
        {
            set
            {
                view.GetLabel = _ => value((ITree<T>)_);
            }
        }

        public Func<ITree<T>, string> GetToolTipText
        {
            set
            {
                view.GetToolTipText = _ => value((ITree<T>)_);
            }
        }

        public Func<ITree<T>, double> GetSize
        {
            set
            {
                view.GetSize = _ => value((ITree<T>)_);
            }
        }

        public event EventHandler<TreeEventArgs<T>> Activate;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Adapter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
