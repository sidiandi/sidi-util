using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.TreeMap
{
    class LabelPainterController : IDisposable
    {
        View view;
        LabelPainter painter;

        public LabelPainterController(View view, LabelPainter painter)
        {
            this.view = view;
            this.painter = painter;

            view.MouseMove += View_MouseMove;
        }

        private void View_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            painter.Focus(view.WorldToScreen.GetInverse().Transform(e.Location.ToPointD()));
            view.Invalidate();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    view.MouseMove -= View_MouseMove;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LabelPainterController() {
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
