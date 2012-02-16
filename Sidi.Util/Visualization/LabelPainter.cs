using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    public class LabelPainter<T> where T : ITree
    {
        public StringFormat StringFormat;

        TreeMapControl<T> treeMapControl;

        public bool[] LevelVisible = new bool[0x100];

        public LabelPainter(TreeMapControl<T> treeMapControl)
        {
            this.treeMapControl = treeMapControl;
            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Font = new Font(FontFamily.GenericSansSerif, 10.0f);

            ShowLevels(2);
        }

        public float MinArea = 100.0f;

        public void ShowLevels(int index)
        {
            int i;
            for (i = 0; i < 1; ++i)
            {
                LevelVisible[i] = false;
            }
            for (; i <= index; ++i)
            {
                LevelVisible[i] = true;
            }
            for (; i < LevelVisible.Length;  ++i)
            {
                LevelVisible[i] = false;
            }
        }

        public void Focus(Point focusPoint)
        {
            this.focusPoint = focusPoint;
            focusPointEnabled = true;
        }

        bool focusPointEnabled = false;
        Point focusPoint;

        public void Paint(PaintEventArgs e)
        {
            var treeMap = treeMapControl.TreeLayout;
            float maxArea = treeMap.LayoutTree.Data.Rectangle.Area();
            alphaF = 220.0 / (Math.Log10(maxArea) - Math.Log10(MinArea));
            if (focusPointEnabled)
            {
                PaintRecursiveFocusPoint(e, treeMap.LayoutTree, 0);
            }
            else
            {
                PaintRecursive(e, treeMap.LayoutTree, 0);
            }
        }

        public Font Font;
        public float MinFontSize = 5.0f;
        public bool LeafsOnly = false;
        public Func<T, string> Text = t => t.ToString();

        double alphaF;

        bool PaintRecursive(PaintEventArgs e, Tree<TreeMapLayout.Layout> n, int level)
        {
            var rectArea = n.Data.Rectangle.Area();
            if (!e.ClipRectangle.IntersectsWith(n.Data.Rectangle.ToRectangle()))
            {
                return false;
            }

            if (LevelVisible[level])
            {
                var rect = n.Data.Rectangle.ToRectangleF();
                var text = Text((T)n.Data.TreeNode);
                var textSize = e.Graphics.MeasureString(text, Font);
                var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
                if ((scale * textSize.Height * 8) < rect.Height)
                {
                    scale *= 2;
                }

                byte a = Util.ClipByte(level * 32 + 128);

                var white = new SolidBrush(Color.FromArgb(a, Color.White));
                float fontSize = Math.Max(Font.Size * scale, 1.0f);

                if (fontSize < MinFontSize)
                {
                    return false;
                }

                var font = new Font(FontFamily.GenericSansSerif, fontSize);

                e.Graphics.DrawString(
                text,
                font,
                white,
                rect,
                StringFormat
                );
            }

            foreach (var c in n.Children)
            {
                PaintRecursive(e, c, level + 1);
            }

            return true;
        }

        public bool DrawLabel(Graphics g, string text, RectangleF rect)
        {
            var textSize = g.MeasureString(text, Font);
            var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
            if ((scale * textSize.Height * 8) < rect.Height)
            {
                scale *= 2;
            }
            byte a = 255;

            var white = new SolidBrush(Color.FromArgb(a, Color.White));
            float fontSize = Math.Max(Font.Size * scale, 1.0f);

            if (fontSize < MinFontSize)
            {
                return false;
            }

            var font = new Font(FontFamily.GenericSansSerif, fontSize);

            g.DrawString(
            text,
            font,
            white,
            rect,
            StringFormat
            );
            return true;

        }

        bool PaintRecursiveFocusPoint(PaintEventArgs e, Tree<TreeMapLayout.Layout> n, int level)
        {
            var rectArea = n.Data.Rectangle.Area();
            if (!e.ClipRectangle.IntersectsWith(n.Data.Rectangle.ToRectangle()))
            {
                return false;
            }

            var rect = n.Data.Rectangle.ToRectangleF();

            if (rect.Contains(focusPoint))
            {
                if (n.Children.Any())
                {
                    foreach (var c in n.Children)
                    {
                        PaintRecursiveFocusPoint(e, c, level+1);
                    }
                }
                else
                {
                    var text = Text((T)n.Data.TreeNode);
                    DrawLabel(e.Graphics, text, rect);
                }
                return true;
            }
            else
            {
                var text = Text((T)n.Data.TreeNode);
                DrawLabel(e.Graphics, text, rect);
                return true;
            }
        }
    }
}
