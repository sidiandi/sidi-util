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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public LabelPainter(TreeMap treeMapControl)
        {
            this.treeMapControl = treeMapControl;

            LevelVisible = new bool[0x100];
            MinArea = 1000;
            MinFontSize = 5;
            LeafsOnly = false;
            Text = t => t == null ? String.Empty : t.ToString();

            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None,
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
            else if (e.KeyCode == Keys.F)
            {
                ShowLabelFrame = !ShowLabelFrame;
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

        public double MinArea { get; set; }

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
        System.Windows.Media.Matrix worldToScreenTransform;
        double areaScale;

        public void Paint(PaintEventArgs e)
        {
            var layout = treeMapControl.LayoutManager;
            if (layout == null)
            {
                return;
            }
            double maxArea = layout.Root.Bounds.Area;
            alphaF = 220.0 / (Math.Log10(maxArea) - Math.Log10(MinArea));
            worldToScreenTransform = e.Graphics.Transform.ToMatrixD();
            areaScale = worldToScreenTransform.Transform(new Bounds(0, 0, 1, 1)).Area;

            if (focusPointEnabled)
            {
                PaintFocusPoint(e);
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

        public bool ShowLabelFrame { get; set; }

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

                float fontSize = Math.Max(Font.Size * scale, 1.0f);

                if (fontSize < MinFontSize)
                {
                    return false;
                }

                using (var white = new SolidBrush(Color.FromArgb(a, Color.White)))
                using (var font = new Font(FontFamily.GenericSansSerif, fontSize))
                {
                    e.Graphics.DrawString(
                        text,
                        font,
                        white,
                        rect,
                        StringFormat
                        );
                }
            }

            foreach (var c in n.Children.Cast<Layout>())
            {
                PaintRecursive(e, c, level + 1);
            }

            return true;
        }

        /// <summary>
        /// Draws a label in world rectangle rect
        /// </summary>
        /// <param name="graphics">Assumes that graphics.Transform is identity</param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel(Graphics graphics, string text, Bounds rect)
        {
            return DrawLabel1(graphics, text, rect);
        }
        
        /// <summary>
        /// Draws a label in world rectangle rect
        /// </summary>
        /// <param name="graphics">Assumes that graphics.Transform is identity</param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel2(Graphics graphics, string text, Bounds rect)
        {
            try
            {
                rect = worldToScreenTransform.Transform(rect);

                var charDims = Enumerable.Range(0, text.Length)
                    .Select(i => graphics.MeasureString(text.Substring(i, 1), this.Font))
                    .ToArray();

                float totalWidth = charDims.Sum(r => r.Width);
                float totalHeight = charDims[0].Height;
                int lineCount = Math.Max(1, (int)Math.Round(Math.Sqrt((totalWidth / totalHeight) / (rect.Width / rect.Height))));

                var lines = Enumerable.Range(0, lineCount)
                    .Select(i =>
                        {
                            int b = i * text.Length / lineCount;
                            int e = (i + 1) * text.Length / lineCount;
                            return text.Substring(b, e - b);
                        })
                        .ToArray();

                var lineRect = rect.ToRectangleF();
                lineRect.Height = (float)rect.Height / (float)lineCount;


                var stringFormat = new StringFormat()
                {
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.None,
                };

                byte alpha = 255;
                using (var white = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                using (var font = new Font(FontFamily.GenericSansSerif, lineRect.Height * 0.8f))
                {
                    for (int i = 0; i < lines.Length; ++i)
                    {
                        graphics.DrawString(
                            lines[i],
                            font,
                            white,
                            lineRect,
                            stringFormat
                        );
                        lineRect.Offset(0, lineRect.Height);
                    }
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Draws a label in world rectangle rect
        /// </summary>
        /// <param name="graphics">Assumes that graphics.Transform is identity</param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel1(Graphics graphics, string text, Bounds rect)
        {
            rect = worldToScreenTransform.Transform(rect);

            var textSize = graphics.MeasureString(text, Font);
            var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
            if ((scale * textSize.Height * 8) < rect.Height)
            {
                scale *= 2;
            }

            byte a = 255;

            var fontSize = (float)Math.Max(Font.Size * scale, 1.0f);

            using (var white = new SolidBrush(Color.FromArgb(a, Color.White)))
            using (var font = new Font(FontFamily.GenericSansSerif, fontSize))
            {
                graphics.DrawString(
                    text,
                    font,
                    white,
                    rect.ToRectangleF(),
                    StringFormat
                );
                if (ShowLabelFrame)
                {
                    graphics.DrawRectangle(new Pen(white), rect.ToRectangle());
                }
            }
            return true;

        }

        /// <summary>
        /// Draws a label in world rectangle rect
        /// </summary>
        /// <param name="graphics">Assumes that graphics.Transform is identity</param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel3(Graphics graphics, string text, Bounds rect)
        {
            rect = worldToScreenTransform.Transform(rect);

            var textSize = graphics.MeasureString(text, Font);
            textSize.Width = Math.Max(textSize.Width, 1.0f);
            textSize.Height = Math.Max(textSize.Height, 1.0f);
            /*
            var scale = Math.Min(rect.Width / Math.Max(1.0f, textSize.Width), rect.Height / Math.Max(1.0f, textSize.Height));
            if ((scale * textSize.Height * 8) < rect.Height)
            {
                scale *= 2;
            }
            */
            var scale = Math.Sqrt((rect.Width * rect.Height) / (textSize.Width * textSize.Height)) * 0.8f;
            byte a = 255;

            var fontSize = (float)Math.Max(Font.Size * scale, 1.0f);

            using (var white = new SolidBrush(Color.FromArgb(a, Color.White)))
            using (var font = new Font(FontFamily.GenericSansSerif, fontSize))
            {
                graphics.DrawString(
                    text,
                    font,
                    white,
                    rect.ToRectangleF(),
                    StringFormat
                );
            }
            return true;

        }

        void PaintFocusPoint(PaintEventArgs e)
        {
            worldFocusPoint = treeMapControl.GetWorldPoint(focusPoint);
            var layout = treeMapControl.LayoutManager.Root;
            var state = e.Graphics.Save();
            try
            {
                e.Graphics.Transform = new Matrix();
                PaintRecursiveFocusPoint(e, layout, 0);
            }
            finally
            {
                e.Graphics.Restore(state);
            }
        }

        void PaintRecursiveFocusPoint(PaintEventArgs e, Layout layout, int level)
        {
            bool drawLabel = !layout.Bounds.Contains(worldFocusPoint) || !layout.Children.Any();

            if (!drawLabel)
            {
                // determine if there is enough area to draw a label in at least 75% of all childs
                double totalArea = 0;
                double drawableArea = 0;
                foreach (var c in layout.Children)
                {
                    var a = c.Bounds.Area * areaScale;
                    if (a >= MinArea)
                    {
                        drawableArea += a;
                    }
                    totalArea += a;
                }

                drawLabel = drawableArea < 0.75 * totalArea;
            }

            if (drawLabel)
            {
                var text = Text(layout.Tree);
                DrawLabel(e.Graphics, text, layout.Bounds);
            }
            else
            {
                foreach (var c in layout.Children)
                {
                    PaintRecursiveFocusPoint(e, c, level + 1);
                };
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
