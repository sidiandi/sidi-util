using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Sidi.Visualization
{
    public class LabelPainter<T> where T : ITree
    {
        public StringFormat StringFormat;

        TreeMapLayout treeMap;

        public LabelPainter(TreeMapLayout treeMap)
        {
            this.treeMap = treeMap;
            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Font = new Font(FontFamily.GenericSansSerif, 10.0f);
        }

        public float MinArea = 100.0f;

        public void Paint(PaintEventArgs e)
        {
            float maxArea = treeMap.LayoutTree.Data.Rectangle.Area();
            alphaF = 220.0 / (Math.Log10(maxArea) - Math.Log10(MinArea));
            PaintRecursive(e, treeMap.LayoutTree);
        }

        public Font Font;

        public Func<T, string> Text = t => t.ToString();

        double alphaF;

        bool PaintRecursive(PaintEventArgs e, Tree<TreeMapLayout.Layout> n)
        {
            var rectArea = n.Data.Rectangle.Area();
            if (rectArea < MinArea || !e.ClipRectangle.IntersectsWith(n.Data.Rectangle.ToRectangle()))
            {
                return false;
            }

            var a50 = Math.Max(rectArea * 0.6, MinArea);

            if (
                n.Parent != null &&
                (
                    n.Children.All(c => c.Data.Rectangle.Area() < a50)
                ))
            {
                var rect = n.Data.Rectangle.ToRectangleF();
                var text = Text((T)n.Data.TreeNode);
                var textSize = e.Graphics.MeasureString(text, Font);
                var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
                if ((scale * textSize.Height * 8) < rect.Height)
                {
                    scale *= 2;
                }

                if (scale < 1.0)
                {
                    return false;
                }

                var a = Util.ClipByte(255.0 - (Math.Log10(n.Data.Rectangle.Area()) - Math.Log10(MinArea)) * alphaF);

                var white = new SolidBrush(Color.FromArgb(a, Color.White));
                var font = new Font(FontFamily.GenericSansSerif, Font.Size * scale);

                e.Graphics.DrawString(
                    text,
                    font,
                    white,
                    rect,
                    StringFormat
                    );

                /*
                var gp = new GraphicsPath();
                gp.AddString(text, FontFamily.GenericSansSerif, (int)FontStyle.Regular, font.Size, rect, StringFormat);
                var outlinePen = new Pen(Color.FromArgb(a, Color.Black), 3.0f);
                e.Graphics.DrawPath(outlinePen, gp);
                e.Graphics.FillPath(white, gp);
                 */
            }

            foreach (var c in n.Children)
            {
                PaintRecursive(e, c);
            }

            return true;
        }
    }
}
