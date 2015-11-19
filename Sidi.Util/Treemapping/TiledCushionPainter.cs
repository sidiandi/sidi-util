using Sidi.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sidi.Treemapping
{
    public class TiledCushionPainter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        readonly CushionPainter cushionPainter;
        Tiles tiles = new Tiles(new System.Drawing.Size(0x100, 0x100));

        public TiledCushionPainter(CushionPainter cushionPainter)
        {
            this.cushionPainter = cushionPainter;
            this.bitmaps = new DefaultValueDictionary<Tile, Bitmap>(ProvideBitmap) { StoreDefaults = true };
        }

        IDictionary<Tile, Bitmap> bitmaps;

        Bitmap ProvideBitmap(Tile i)
        {
            return cushionPainter.Render(Tree, i.WorldRect, tiles.Size);
        }

        TreeNode Tree
        {
            get
            {
                return m_tree;
            }

            set
            {
                if (m_tree != value)
                {
                    Clear();
                }
                m_tree = value;
            }
        }
        TreeNode m_tree;

        public void Clear()
        {
            foreach (var b in bitmaps.Values)
            {
                b.Dispose();
            }
            bitmaps.Clear();
        }

        public void Paint(TreePaintArgs pa)
        {
            var t = tiles.Get(pa.WorldToScreen, pa.PaintEventArgs.ClipRectangle);
            this.Tree = pa.Tree;

            var g = pa.PaintEventArgs.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            foreach (var i in t)
            {
                var bitmap = bitmaps[i];
                {
                    var sr = i.ScreenRect.ToRectangleF();
                    g.DrawImage(bitmap, sr);

                    // for debugging
                    // g.DrawRectangle(Pens.Black, i.ScreenRect.ToRectangle());
                    // g.DrawString(i.ToString(), Control.DefaultFont, Brushes.Black, sr.Left, sr.Top);
                }
            }
        }
    }
}
