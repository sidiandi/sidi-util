using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Sidi.Visualization
{
    public enum InteractionMode
    {
        Static,
        MouseFocus,
        Max
    };

    public class LabelPainter : IDisposable
    {
        public InteractionMode InteractMode { set; get; }

        public StringFormat StringFormat { set; get; }

        TreeMap treeMapControl;

        public bool[] LevelVisible { get; set;  }

        public LabelPainter(TreeMap treeMapControl)
        {
            this.treeMapControl = treeMapControl;

            LevelVisible = new bool[0x100];
            MinArea = 100.0f;
            MinFontSize = 5.0f;
            LeafsOnly = false;
            Text = t => t == null ? String.Empty : t.ToString();

            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Font = new Font(FontFamily.GenericSansSerif, 10.0f);

            treeMapControl.MouseMove += new MouseEventHandler(treeMapControl_MouseMove);
            treeMapControl.KeyDown += new KeyEventHandler(treeMapControl_KeyDown);
            treeMapControl.Paint += new PaintEventHandler(treeMapControl_Paint);

            ShowLevels(2);
        }

        void treeMapControl_Paint(object sender, PaintEventArgs e)
        {
            Paint(e);
        }

        void treeMapControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (HotkeysEnabled)
            {
                HandleHotKey(e);
            }
        }

        public bool HotkeysEnabled { get; set; }

        void treeMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            switch (InteractMode)
            {
                case InteractionMode.MouseFocus:
                    Focus(e.Location);
                    treeMapControl.Invalidate();
                    break;
                default:
                    break;
            }
        }

        void HandleHotKey(KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
            {
                int index = e.KeyCode - Keys.D1;
                if (e.Modifiers == Keys.Control)
                {
                    ShowLevels(index);
                }
                else
                {
                    ToggleLevelVisibility(index);
                }
                treeMapControl.Invalidate();
            }
            else if (e.KeyCode == Keys.Space)
            {
                ++InteractMode;
                if (InteractMode >= InteractionMode.Max)
                {
                    InteractMode = (InteractionMode)0;
                }

                treeMapControl.Invalidate();
            }
        }

        public float MinArea { get; set; }

        public void ShowLevels(int index)
        {
            focusPointEnabled = false;
            int i;
            for (i = 0; i < 1; ++i)
            {
                LevelVisible[i] = false;
            }
            for (; i <= index; ++i)
            {
                LevelVisible[i] = true;
            }
            for (; i < LevelVisible.Length; ++i)
            {
                LevelVisible[i] = false;
            }
        }

        public void ToggleLevelVisibility(int index)
        {
            focusPointEnabled = false;
            LevelVisible[index] = !LevelVisible[index];
        }

        public void Focus(Point focusPoint)
        {
            this.focusPoint = focusPoint;
            focusPointEnabled = true;
        }

        bool focusPointEnabled = false;
        Point focusPoint;
        System.Windows.Point worldFocusPoint;

        public void Paint(PaintEventArgs e)
        {
            var layout = treeMapControl.LayoutManager;
            double maxArea = layout.Root.Bounds.Area;
            alphaF = 220.0 / (Math.Log10(maxArea) - Math.Log10(MinArea));
            if (focusPointEnabled)
            {
                worldFocusPoint = treeMapControl.GetWorldPoint(focusPoint);
                PaintRecursiveFocusPoint(e, layout.Root, 0);
            }
            else
            {
                PaintRecursive(e, layout.Root, 0);
            }
        }

        public Font Font { set; get; }
        public float MinFontSize { set; get; }
        public bool LeafsOnly { get; set; }
        public Func<Tree, string> Text { set; get; }

        double alphaF;

        bool PaintRecursive(PaintEventArgs e, Layout n, int level)
        {
            var rectArea = n.Bounds.Area;
            if (!e.ClipRectangle.IntersectsWith(n.Bounds.ToRectangle()))
            {
                return false;
            }

            if (LevelVisible[level])
            {
                var rect = n.Bounds.ToRectangleF();
                var text = Text(n.Tree);
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

            foreach (var c in n.Children.Cast<Layout>())
            {
                PaintRecursive(e, c, level + 1);
            }

            return true;
        }

        public bool DrawLabel(Graphics graphics, string text, RectangleF rect)
        {
            var textSize = graphics.MeasureString(text, Font);
            var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
            if ((scale * textSize.Height * 8) < rect.Height)
            {
                scale *= 2;
            }
            byte a = 255;

            var white = new SolidBrush(Color.FromArgb(a, Color.White));
            float fontSize = Math.Max(Font.Size * scale, 1.0f);

            /*
            if (fontSize < MinFontSize)
            {
                return false;
            }
             */

            var font = new Font(FontFamily.GenericSansSerif, fontSize);

            graphics.DrawString(
            text,
            font,
            white,
            rect,
            StringFormat
            );
            return true;

        }

        bool PaintRecursiveFocusPoint(PaintEventArgs e, Layout layout, int level)
        {
            // var rectArea = n.Rectangle.Area();
            /*
            if (!e.ClipRectangle.IntersectsWith(layout.Rectangle.ToRectangle()))
            {
                return false;
            }
             */

            if (layout.Bounds.Contains(worldFocusPoint))
            {
                if (layout.Children.Any())
                {
                    foreach (var c in layout.Children)
                    {
                        PaintRecursiveFocusPoint(e, c, level + 1);
                    }
                }
                else
                {
                    var text = Text(layout.Tree);
                    DrawLabel(e.Graphics, text, layout.Bounds.ToRectangleF());
                }
                return true;
            }
            else
            {
                var text = Text(layout.Tree);
                DrawLabel(e.Graphics, text, layout.Bounds.ToRectangleF());
                return true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Font != null)
                {
                    Font.Dispose();
                    Font = null;
                }
            }
        }
    }
}
