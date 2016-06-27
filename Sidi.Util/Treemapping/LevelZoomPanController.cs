using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sidi.Treemapping
{
    public class LevelZoomPanController : IDisposable
    {
        public LevelZoomPanController(View view)
        {
            this.view = view;

            // view.MouseWheel += View_MouseWheel;
            view.MouseClick += View_MouseClick;
            view.KeyDown += View_KeyDown;
        }

        private void View_MouseClick(object sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                if (e.Button == MouseButtons.Left)
                {
                    LevelDown(e.Location);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    LevelUp();
                }
            }
        }

        private void View_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Alt) != 0)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        LevelUp();
                        break;
                }
            }
        }

        public void LevelUp()
        {
            if (view.Tree.Parent != null)
            {
                view.Tree = view.Tree.Parent;
                view.WorldToScreen = new System.Windows.Media.Matrix();
            }
        }

        public void LevelDown(System.Drawing.Point clientPoint)
        {
            var w = view.GetWorldPoint(clientPoint);
            var c = view.Layout.GetNodeAt(w).Data.Tag;
            if (c != null)
            {
                view.Tree = c;
                view.WorldToScreen = new System.Windows.Media.Matrix();
            }
        }

        private void View_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                if (e.Delta > 0)
                {
                    LevelDown(e.Location);
                }
                else if (e.Delta < 0)
                {
                    LevelUp();
                }
            }
        }

        readonly View view;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    view.MouseWheel -= View_MouseWheel;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LevelZoomPanController() {
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
