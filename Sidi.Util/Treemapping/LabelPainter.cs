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
using Sidi.Extensions;

namespace Sidi.Treemapping
{
    public enum InteractionMode
    {
        Static,
        MouseFocus,
        Max
    };

    public class LabelPainter
    {
        public InteractionMode InteractMode { set; get; }

        public StringFormat StringFormat { set; get; }

        public bool[] LevelVisible { get; set;  }

        public LabelPainter()
        {
            LevelVisible = new bool[0x100];
            MinArea = 1000;
            MinFontSize = 5;
            LeafsOnly = false;
            Text = t => t == null ? String.Empty : t.Tag.SafeToString();

            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None,
            };

            Font = new Font(FontFamily.GenericSansSerif, 10.0f);

            ShowLevels(0x80);
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

        public void Focus(System.Windows.Point worldFocusPoint)
        {
            this.worldFocusPoint = worldFocusPoint;
            focusPointEnabled = true;
        }

        bool focusPointEnabled = false;
        System.Windows.Point worldFocusPoint;
        double areaScale;

        public void Paint(TreePaintArgs pa)
        {
            var g = pa.PaintEventArgs.Graphics;
            double maxArea = pa.Tree.Data.Rectangle.Area;
            alphaF = 220.0 / (Math.Log10(maxArea) - Math.Log10(MinArea));
            areaScale = pa.WorldToScreen.Transform(RectangleD.FromLTRB(0, 0, 1, 1)).Area;

            if (focusPointEnabled)
            {
                PaintFocusPoint(pa);
            }
            else
            {
                PaintRecursive(pa, 0);
            }
        }

        void PaintFocusPoint(TreePaintArgs pa)
        {
            var g = pa.PaintEventArgs.Graphics;
            PaintRecursiveFocusPoint(pa, 0);
        }

        void PaintRecursiveFocusPoint(TreePaintArgs pa, int level)
        {
            bool drawLabel = !pa.Tree.Data.Rectangle.Contains(worldFocusPoint) || pa.Tree.IsLeaf();

            if (!drawLabel)
            {
                // determine if there is enough area to draw a label in at least 75% of all childs
                double totalArea = 0;
                double drawableArea = 0;
                foreach (var c in pa.Tree.Children)
                {
                    var a = c.Data.Rectangle.Area * areaScale;
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
                var text = Text(pa.Tree.Data.Tag);
                DrawLabel(pa, text, pa.Tree.Data.Rectangle);
            }
            else
            {
                foreach (var c in pa.Tree.Children)
                {
                    var newPa = pa.Clone();
                    newPa.Tree = c;
                    PaintRecursiveFocusPoint(newPa, level + 1);
                };
            }
        }
        public Font Font { set; get; }
        public float MinFontSize { set; get; }
        public bool LeafsOnly { get; set; }
        public Func<TreeNode, string> Text { set; get; }

        double alphaF;

        public bool ShowLabelFrame { get; set; }

        bool PaintRecursive(TreePaintArgs pa, int level)
        {
            var g = pa.PaintEventArgs.Graphics;
            var treeRectScreen = pa.WorldToScreen.Transform(pa.Tree.Data.Rectangle);

            if (!treeRectScreen.Intersects(pa.PaintEventArgs.ClipRectangle))
            {
                return false;
            }

            var levelVisible = 
                    LevelVisible[level] &&
                    !treeRectScreen.Includes(pa.ScreenRect);

            if (levelVisible)
            {
                var text = Text(pa.Tree.Data.Tag);
                var textSize = g.MeasureString(text, Font);
                var scale = Math.Min(treeRectScreen.Width / Math.Max(1.0f, textSize.Width), treeRectScreen.Height / Math.Max(1.0f, textSize.Height));
                if ((scale * textSize.Height * 8) < treeRectScreen.Height)
                {
                    scale *= 2;
                }

                byte a = Util.ClipByte(level * 32 + 128);

                float fontSize = Math.Max(Font.Size * (float)scale, 1.0f);

                if (fontSize < MinFontSize)
                {
                    return false;
                }

                using (var white = new SolidBrush(Color.FromArgb(a, Color.White)))
                using (var font = new Font(FontFamily.GenericSansSerif, fontSize))
                {
                    g.DrawString(
                        text,
                        font,
                        white,
                        treeRectScreen.ToRectangle(),
                        StringFormat
                        );
                }
            }

            foreach (var c in pa.Tree.Children)
            {
                PaintRecursive(new TreePaintArgs
                {
                    PaintEventArgs = pa.PaintEventArgs,
                    WorldToScreen = pa.WorldToScreen,
                    Tree = c
                }, level + 1);
            }

            return true;
        }

        /// <summary>
        /// Draws a label in world rectangle rect
        /// </summary>
        /// <param name="pa"></param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel(TreePaintArgs pa, string text, RectangleD rect)
        {
            return DrawLabel1(pa, text, rect);
        }

        /// <summary>
        /// Draws a label in world rectangle rect
        /// </summary>
        /// <param name="pa"></param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel2(TreePaintArgs pa, string text, RectangleD rect)
        {
            try
            {
                rect = pa.WorldToScreen.Transform(rect);
                var g = pa.PaintEventArgs.Graphics;

                var charDims = Enumerable.Range(0, text.Length)
                    .Select(i => g.MeasureString(text.Substring(i, 1), this.Font))
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
                        g.DrawString(
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
        /// <param name="pa"></param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel1(TreePaintArgs pa, string text, RectangleD rect)
        {
            var graphics = pa.PaintEventArgs.Graphics;
            rect = pa.WorldToScreen.Transform(rect);

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
        /// <param name="pa"></param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool DrawLabel3(TreePaintArgs pa, string text, RectangleD rect)
        {
            var graphics = pa.PaintEventArgs.Graphics;
            rect = pa.WorldToScreen.Transform(rect);

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
