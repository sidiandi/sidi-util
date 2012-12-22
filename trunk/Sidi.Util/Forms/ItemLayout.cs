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
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Sidi.Util;
using Sidi.Collections;

namespace Sidi.Forms
{
    public class LayoutContext
    {
        public Size WindowSize;
        public int ItemCount;
    }
    
    public interface IItemLayout
    {
        int IndexAt(Point p);
        Rectangle ItemRect(int index);
        Point GridLocation(int index);
        void Update(LayoutContext context);
    }

    public class ItemLayoutFixedSize : IItemLayout
    {
        public ItemLayoutFixedSize(Size size)
        {
            m_itemSize = size;
        }

        Size m_itemSize;
        int m_columnCount = 1;

        #region IItemLayout Members

        public int IndexAt(Point p)
        {
            int r = p.Y / m_itemSize.Height;
            int c = p.X / m_itemSize.Width;
            return c + r * m_columnCount;
        }

        public Point GridLocation(int index)
        {
            int row = index / m_columnCount;
            int column = index - (row * m_columnCount);
            return new Point(column, row);
        }

        public Rectangle ItemRect(int index)
        {
            Point g = GridLocation(index);
            return new Rectangle(g.X * m_itemSize.Width, g.Y * m_itemSize.Height, m_itemSize.Width, m_itemSize.Height);
        }

        public void Update(LayoutContext context)
        {
            m_columnCount = (context.WindowSize.Width / m_itemSize.Width);
            if (m_columnCount < 1)
            {
                m_columnCount = 1;
            }
            int rowCount = (context.ItemCount / m_columnCount) + 1;
        }

        #endregion
    }

    public class ItemLayoutFit : IItemLayout
    {
        public ItemLayoutFit()
        {
            m_itemSize = new SizeF(1.0f, 1.0f);
            m_minItemSize = new SizeF(64.0f, 64.0f);
        }

        SizeF m_itemSize;
        int m_columnCount = 1;
        SizeF m_minItemSize;

        #region IItemLayout Members

        public int IndexAt(Point p)
        {
            int r = (int)((double)p.Y / m_itemSize.Height);
            int c = (int)((double)p.X / m_itemSize.Width);
            return c + r * m_columnCount;
        }

        public Point GridLocation(int index)
        {
            int row = index / m_columnCount;
            int column = index - (row * m_columnCount);
            return new Point(column, row);
        }

        public Rectangle ItemRect(int index)
        {
            int row = index / m_columnCount;
            int column = index - (row * m_columnCount);
            int x0 = (int)((double)column * m_itemSize.Width);
            int y0 = (int)((double)row * m_itemSize.Height);
            int x1 = (int)((double)(column + 1) * m_itemSize.Width);
            int y1 = (int)((double)(row + 1) * m_itemSize.Height);
            return new Rectangle(x0, y0, x1 - x0, y1 - y0);
        }

        public void Update(LayoutContext context)
        {
            // must recalc m_columnCount and m_itemSize based on
            // the size of the viewport window and itemCount

            if (context.ItemCount == 0 ||
                context.WindowSize.Width == 0 ||
                context.WindowSize.Height == 0)
            {
                m_columnCount = 1;
                m_itemSize = m_minItemSize;
                return;
            }

            Size n;

            // try to fit into viewport area without scroll bars
            GraMath.Tile(context.WindowSize, context.ItemCount, out n, out m_itemSize);
            if
            (
                m_itemSize.Width >= m_minItemSize.Width &&
                m_itemSize.Height >= m_minItemSize.Height
            )
            {
            }
            else
            {
                n.Width = (int)((float)context.WindowSize.Width / (float)m_minItemSize.Width);
                n.Height = GraMath.Ceil(context.ItemCount, n.Width) / n.Width;
                m_itemSize.Width = (float)context.WindowSize.Width / (float)n.Width;
                m_itemSize.Height = m_itemSize.Width;
                m_columnCount = n.Width;
            }
            m_columnCount = n.Width;
        }

        #endregion
    }

    public class ItemLayoutRows : IItemLayout
    {
        public ItemLayoutRows(int height)
        {
            m_itemSize = new Size(0, height);
        }

        Size m_itemSize;

        #region IItemLayout Members

        public int IndexAt(Point p)
        {
            return p.Y / m_itemSize.Height;
        }

        public Point GridLocation(int index)
        {
            int row = index;
            int column = 0;
            return new Point(column, row);
        }

        public Rectangle ItemRect(int index)
        {
            return new Rectangle(0, index * m_itemSize.Height, m_itemSize.Width, m_itemSize.Height);
        }

        public void Update(LayoutContext context)
        {
            m_itemSize.Width = context.WindowSize.Width;
        }
        #endregion
    }
}
