// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
    public class ItemView<Item> : Control
    {
        public class PaintArgs
        {
            public PaintArgs(Graphics graphics, int index, Rectangle r, ItemView<Item> view)
            {
                m_graphics = graphics;
                m_index = index;
                m_rect = r;
                m_view = view;
            }

            Graphics m_graphics;

            public Graphics Graphics
            {
                get { return m_graphics; }
            }

            Rectangle m_rect;

            public Rectangle Rect
            {
                get { return m_rect; }
            }

            public Item Item
            {
                get { return View.List[Index]; }
            }

            int m_index;

            public int Index
            {
                get { return m_index; } 
            }
            
            ItemView<Item> m_view;

            public ItemView<Item> View
            {
                get { return m_view; }
            }

            public bool Focused
            {
                get { return View.FocusedItemIndex == Index; }
            }

            public bool Selected
            {
                get { return View.IsSelected(Index); }
            }

            public Brush ForegroundBrush
            {
                get
                {
                    if (Selected)
                    {
                        return new SolidBrush(Color.FromKnownColor(KnownColor.HighlightText));
                    }
                    else
                    {
                        return new SolidBrush(Color.FromKnownColor(KnownColor.ControlText));
                    }
                }
            }

            public Brush BackgroundBrush
            {
                get
                {
                    if (Selected)
                    {
                        return new SolidBrush(Color.FromKnownColor(KnownColor.Highlight));
                    }
                    else
                    {
                        Color c;
                        Point g = m_view.ItemLayout.GridLocation(Index);
                        int cIndex = g.X + g.Y;
                        if (cIndex % 2 == 0)
                        {
                            c = Color.FromKnownColor(KnownColor.ControlLightLight);
                        }
                        else
                        {
                            c = Color.FromKnownColor(KnownColor.ControlLight);
                        }
                        return new SolidBrush(c);
                    }
                }
            }
        }

        public interface IItemFormat
        {
            void Paint(PaintArgs e);
        }

        public delegate string Stringifier(Item item);

        public IItemFormat CreateStringFormat(Stringifier d)
        {
            return new DelegateStringItemFormat(d);
        }

        public class StringItemFormat : IItemFormat
        {
            #region IItemFormat Members

            Size m_itemSize = new Size(300, 16);

            public Size ItemSize
            {
                get { return m_itemSize; }
                set { m_itemSize = value; }
            }

            public void AfterPaint(PaintArgs e)
            {
            }

            public void Paint(PaintArgs e)
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = 
                    StringFormatFlags.NoWrap | 
                    StringFormatFlags.FitBlackBox |
                    0;
                sf.Trimming = StringTrimming.None;
                e.Graphics.FillRectangle(e.BackgroundBrush, e.Rect);
                e.Graphics.DrawString(ToString(e), e.View.Font, e.ForegroundBrush, e.Rect,sf);
            }

            #endregion

            protected virtual string ToString(PaintArgs e)
            {
                return e.Index.ToString();
            }
        }

        public class DelegateStringItemFormat : StringItemFormat
        {
            Stringifier stringifier;

            public DelegateStringItemFormat(Stringifier d)
            {
                stringifier = d;
            }

            protected override string ToString(PaintArgs e)
            {
                return stringifier(e.Item);
            }
        }

        public class PropertyStringItemFormat : StringItemFormat 
        {
            string m_propertyName;

            public PropertyStringItemFormat(string propertyName)
            {
                m_propertyName = propertyName;
            }

            protected override string ToString(PaintArgs e)
            {
                try
                {
                    object o = e.Item;
                    return o.GetType().GetProperty(m_propertyName).GetValue(o, new object[] { }).ToString();
                }
                catch (Exception)
                {
                    return "?";
                }
            }
        }

        public class DefaultItemFormat : StringItemFormat
        {
            protected override string ToString(PaintArgs e)
            {
                return e.Item.ToString();
            }
        }

        public class SubItemFormat : IItemFormat
        {
            #region IItemFormat Members

            public void AfterPaint(PaintArgs e)
            {
            }
            
            public void Paint(PaintArgs e)
            {
                Point o = e.Rect.Location;
                int i;
                for (i=0; i<m_columns.Count-1; ++i)
                {
                    IItemFormat itemFormat = m_columns[i].ItemFormat;
                    Rectangle r = new Rectangle(o, new Size(m_columns[i].Size.Width, e.Rect.Height));
                    itemFormat.Paint(new PaintArgs(e.Graphics, e.Index, r, e.View));
                    o.Offset(r.Width, 0);
                }
                for (; i < m_columns.Count; ++i)
                {
                    IItemFormat itemFormat = m_columns[i].ItemFormat;
                    Rectangle r = new Rectangle(o.X, o.Y, e.Rect.Width - o.X, e.Rect.Height - o.Y);
                    itemFormat.Paint(new PaintArgs(e.Graphics, e.Index, r, e.View));
                    o.Offset(r.Width, 0);
                }
            }

            #endregion

            public class Column
            {
                public Column(Size size, IItemFormat itemFormat)
                {
                    m_size = size;
                    m_itemFormat = itemFormat;
                }

                Size m_size;
                IItemFormat m_itemFormat;

                public Size Size
                {
                    get { return m_size; }
                }

                public IItemFormat ItemFormat
                {
                    get { return m_itemFormat; } 
                }
            }

            public class ColumnCollection : List<Column>
            {
                public void Add(IItemFormat itemFormat, int width)
                {
                    Add(new Column(new Size(width, 0), itemFormat));
                }
            }
            
            ColumnCollection m_columns = new ColumnCollection();

            public ColumnCollection Columns
            {
                get { return m_columns; }
            }
        }

        HScrollBar hScrollBar;
        VScrollBar vScrollBar;

        public ItemView()
        {
            
            this.SuspendLayout();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserMouse |
                ControlStyles.UserPaint |
                0, true);

            hScrollBar = new HScrollBar();
            hScrollBar.Scroll += new ScrollEventHandler(ScrollBar_HScroll);
            Controls.Add(hScrollBar);

            vScrollBar = new VScrollBar();
            vScrollBar.Scroll += new ScrollEventHandler(ScrollBar_VScroll);
            Controls.Add(vScrollBar);

            ItemLayout = new ItemLayoutRows(16);
            ItemFormat = new DefaultItemFormat();
            KeyDown += new KeyEventHandler(ItemView_KeyDown);
            m_updateTimer.Tick += new EventHandler(m_updateTimer_Tick);
            m_focusedItemIndex = 0;

            MouseWheel += new MouseEventHandler(ItemView_MouseWheel);

            this.ResumeLayout(false);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData != Keys.Tab;
        }

        void ItemView_MouseWheel(object sender, MouseEventArgs e)
        {
            ScrollSteps(new Size(0, - e.Delta / Sidi.Forms.MouseWheelSupport.Unit));
        }

        public void ScrollSteps(Size d)
        {
            Point offset = Offset;
            offset.X += d.Width * smallChange.Width;
            offset.Y += d.Height * smallChange.Height;
            Offset = offset;
        }

        void ScrollBar_HScroll(object sender, ScrollEventArgs e)
        {
            Point p = new Point(e.NewValue, vScrollBar.Value);
            SetOffsetInternal(p);
        }

        void ScrollBar_VScroll(object sender, ScrollEventArgs e)
        {
            Point p = new Point(hScrollBar.Value, e.NewValue);
            SetOffsetInternal(p);
        }

        Rectangle OffsetRect
        {
            get
            {
                Rectangle r = ContentRect;
                Rectangle c = ClientRectangle;
                r.Width -= c.Width;
                r.Height -= c.Height;
                return r;
            }    
        }

        void SetOffsetInternal(Point value)
        {
            offset = value.Clip(OffsetRect);
            Invalidate();
        }

        void SyncScrollbars()
        {
            SetClippedValue(hScrollBar, Offset.X);
            SetClippedValue(vScrollBar, Offset.Y);
        }

        void ClipValue(ScrollBar sb)
        {
            int v = sb.Value;
            if (v > sb.Maximum - sb.LargeChange)
            {
                v = sb.Maximum - sb.LargeChange;
            }
            if (v < sb.Minimum)
            {
                v = sb.Minimum;
            }
        }

        void SetClippedValue(ScrollBar sb, int v)
        {
            if (v > sb.Maximum - sb.LargeChange)
            {
                v = sb.Maximum - sb.LargeChange;
            }
            if (v < sb.Minimum)
            {
                v = sb.Minimum;
            }
            sb.Value = v;
        }

        void m_updateTimer_Tick(object sender, EventArgs e)
        {
            PerformLayout();
        }

        void ItemView_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        KbGotoItem(FocusedItemIndex - 1);
                        break;
                    case Keys.Down:
                        KbGotoItem(FocusedItemIndex + 1);
                        break;
                    case Keys.End:
                        KbGotoItem(All.End - 1);
                        break;
                    case Keys.Home:
                        KbGotoItem(0);
                        break;
                    case Keys.A:
                        if (e.Control)
                        {
                            Selection.Set(All);
                            OnSelectionChanged();
                        }
                        break;
                    case Keys.PageDown:
                        KbGotoItem(FocusedItemIndex + VisibleItemRange.Length);
                        break;
                    case Keys.PageUp:
                        KbGotoItem(FocusedItemIndex - VisibleItemRange.Length);
                        break;
                    case Keys.Space:
                        KbSelectItem();
                        break;
                    case Keys.Enter:
                        OnItemsActivated();
                        break;
                    default:
                        return;
                }
            }

            e.Handled = true;
        }

        public event EventHandler FocusedItemChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler ItemsActivated;

        protected virtual void OnItemsActivated()
        {
            if (ItemsActivated != null)
            {
                ItemsActivated(this, EventArgs.Empty);
            }
        }

        static public ItemView<Item> Create(IList<Item> list)
        {
            ItemView<Item> view = new ItemView<Item>();
            view.List = list;
            return view;
        }

        IList<Item> m_list = null;
        IItemFormat m_itemFormat;
        IItemLayout m_itemLayout;
        int m_focusedItemIndex = 0;

        public IList<Item> List
        {
            get
            {
                IList<Item> list = ProvideList();
                if (list != m_list)
                {
                    List = list;
                }
                return m_list;
            }
            set
            {
                if (m_list == value)
                {
                    // return;
                }

                m_list = value;
                m_focusedItemIndex = All.Clip(m_focusedItemIndex);
                m_selection.Intersect(new IntSet(All));
                UpdateLayout();
            }
        }

        protected virtual IList<Item> ProvideList()
        {
            return m_list;
        }

        public IItemFormat ItemFormat
        {
            get { return m_itemFormat; }
            set
            {
                m_itemFormat = value;
                UpdateLayout();
            }
        }

        public IItemLayout ItemLayout
        {
            get { return m_itemLayout; }
            set
            {
                m_itemLayout = value;
                // todo
                // SmallChange = m_itemLayout.ItemRect(0).Size;
                UpdateLayout();
            }
        }

        Rectangle ItemRect(int index)
        {
            Rectangle r = m_itemLayout.ItemRect(index);
            r.Offset(-Offset.X, -Offset.Y);
            return r;
        }

        public void EnsureVisible(int index)
        {
            EnsureVisible(ItemLayout.ItemRect(index));
        }

        Rectangle VisibleRect
        {
            get
            {
                Rectangle r = ClientRectangle;
                r.Offset(Offset);
                return r;
            }
        }

        public void EnsureVisible(Rectangle r)
        {
            Rectangle v = VisibleRect;
            if (!v.Contains(r))
            {
                Point o = new Point();

                if (r.Left < v.Left)
                {
                    o.X = r.Left;
                }

                if (v.Right < r.Right)
                {
                    o.X = r.Right - v.Width;
                }

                if (r.Top < v.Top)
                {
                    o.Y = r.Top;
                }

                if (v.Bottom < r.Bottom)
                {
                    o.Y = r.Bottom - v.Height;
                }

                Offset = o;
            }
        }

        public void EnsureVisibleMaxMove(Rectangle r)
        {
            Rectangle v = VisibleRect;
            if (!v.Contains(r))
            {
                Point o = new Point();

                if (r.Left < v.Left)
                {
                    o.X = r.Right - v.Width;
                }

                if (v.Right < r.Right)
                {
                    o.X = r.Left;
                }

                if (r.Top < v.Top)
                {
                    o.Y = r.Bottom - v.Height;
                }

                if (v.Bottom < r.Bottom)
                {
                    o.Y = r.Top;
                }

                Offset = o;
            }
        }

        Point ToItemCoords(Point windowCoords)
        {
            Point ic = windowCoords;
            ic.Offset(Offset);
            return ic;
        }

        int ItemIndexAt(Point windowCoords)
        {
            return m_itemLayout.IndexAt(ToItemCoords(windowCoords));
        }

        Point offset;

        Point Offset
        {
            get { return offset; }
            set
            {
                SetOffsetInternal(value);
                SyncScrollbars();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateLayout();
        }

        void UpdateLayout()
        {
            LayoutContext context = new LayoutContext();
            context.WindowSize = this.ClientSize;
            context.ItemCount = All.End;
            ItemLayout.Update(context);

            // update scrollbars


            PerformLayout();
        }

        Rectangle Union(Rectangle r1, Rectangle r2)
        {
            Point tl = new Point(Math.Min(r1.Left, r2.Left), Math.Min(r1.Top, r2.Top));
            Point br = new Point(Math.Max(r1.Right, r2.Right), Math.Max(r1.Bottom, r2.Bottom));
            return new Rectangle(tl, br.Sub(tl));
        }

        Size smallChange = new Size(64, 64);

        Rectangle ContentRect
        {
            get
            {
                return Union(
                    ItemLayout.ItemRect(0),
                    ItemLayout.ItemRect(All.End-1));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            UpdateLayout();

            Rectangle c = ClientRectangle;
            Rectangle sr = ContentRect;

            bool hScrollBarVisible = (c.Width < sr.Width);
            bool vScrollBarVisible = (c.Height < sr.Height);

            Size scrollbarSize = new Size(
                vScrollBarVisible ? vScrollBar.Size.Width : 0,
                hScrollBarVisible ? hScrollBar.Size.Height : 0);

            Rectangle viewportWindowRect = new Rectangle(
                c.Left, c.Top,
                c.Width - scrollbarSize.Width,
                c.Height - scrollbarSize.Height);
            
            if (hScrollBarVisible)
            {
                hScrollBar.Bounds = new Rectangle
                (
                    c.Left, c.Bottom - scrollbarSize.Height,
                    c.Width - scrollbarSize.Width, scrollbarSize.Height
                );
            }
            hScrollBar.Visible = hScrollBarVisible;

            if (vScrollBarVisible)
            {
                vScrollBar.Bounds = new Rectangle
                (
                    c.Right - scrollbarSize.Width, c.Top,
                    scrollbarSize.Width, c.Height - scrollbarSize.Height
                );
            }
            vScrollBar.Visible = vScrollBarVisible;

            if (hScrollBarVisible)
            {
                hScrollBar.Minimum = sr.Left;
                hScrollBar.Maximum = sr.Right;
                hScrollBar.LargeChange = System.Math.Max(0, viewportWindowRect.Width);
                hScrollBar.SmallChange = smallChange.Width;
                ClipValue(hScrollBar);
            }

            if (vScrollBarVisible)
            {
                vScrollBar.Minimum = sr.Top;
                vScrollBar.Maximum = sr.Bottom;
                vScrollBar.LargeChange = System.Math.Max(0, viewportWindowRect.Height);
                vScrollBar.SmallChange = smallChange.Height;
                ClipValue(vScrollBar);
            }

            ReadScrollbars();
        }

        void ReadScrollbars()
        {
            Point p = new Point(hScrollBar.Value, vScrollBar.Value);
            SetOffsetInternal(p);
        }

        public Interval VisibleItemRange
        {
            get
            {
                Rectangle r = ClientRectangle;
                return new Interval(
                    ItemIndexAt(r.Location),
                    ItemIndexAt(new Point(r.Right, r.Bottom)) + 1).Intersect(All);
            }
        }

        protected override void  OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle c = ClientRectangle;

            g.FillRectangle(new SolidBrush(this.BackColor), c);

            if (List == null || List.Count == 0)
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                g.DrawString("This list is empty.",
                    this.Font,
                    new SolidBrush(Color.FromKnownColor(KnownColor.ControlText)),
                    c,
                    sf);
            }
            else
            {
                Interval visibleItemRange = VisibleItemRange;
                for (int i = visibleItemRange.End - 1; i >= visibleItemRange.Begin; --i)
                {
                    PaintItem(g, i);
                }
            }
        }

        void PaintItem(Graphics g, int index)
        {
            Rectangle r = ItemRect(index);
            Matrix t = new Matrix();
            t.Translate(r.Left, r.Top);
            g.Transform = t;

            PaintArgs paintArgs = new PaintArgs(g, index, new Rectangle(0, 0, r.Width, r.Height), this);

            m_itemFormat.Paint(paintArgs);
            
            if (paintArgs.Focused)
            {
                ControlPaint.DrawFocusRectangle(paintArgs.Graphics, paintArgs.Rect);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                MouseGotoItem(ItemIndexAt(e.Location));
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            OnItemsActivated();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int i = ItemIndexAt(e.Location);
                if (i != FocusedItemIndex)
                {
                    // MouseGotoItem(i);
                }
            }
        }

        public virtual void OnFocusedItemChanged()
        {
            if (FocusedItemChanged != null)
            {
                FocusedItemChanged(this, EventArgs.Empty);
            }
        }

        public virtual void OnSelectionChanged()
        {
            Invalidate();
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }

        public int FocusedItemIndex
        {
            get
            {
                return m_focusedItemIndex;
            }

            set
            {
                value = All.Clip(value);
                if (value == m_focusedItemIndex)
                {
                    return;
                }

                InvalidateItem(FocusedItemIndex);
                m_focusedItemIndex = value;
                InvalidateItem(FocusedItemIndex);
                EnsureVisible(FocusedItemIndex);
                Selection.Set(Interval.FromClosedInterval(FocusedItemIndex, FocusedItemIndex));
            }
        }

        public Item FocusedItem
        {
            get
            {
                return List[FocusedItemIndex];
            }
        }

        IIntSet m_selection = new IntSet();
        int m_selectionBegin = 0;

        public bool IsSelected(int index)
        {
            return m_selection.Contains(index);
        }

        public IIntSet Selection
        {
            get { return m_selection; }
            set
            {
                if (List == null)
                {
                    return;
                }
                m_selection = value;
                m_selection.Intersect(new IntSet(new Interval(0, List.Count)));
                Invalidate();
            }
        }

        public Interval All
        {
            get 
            {
                if (List == null)
                {
                    return new Interval(0, 0);
                }
                else
                {
                    lock (List)
                    {
                        return new Interval(0, List.Count);
                    }
                }
            }
        }

        public void InvalidateItem(int index)
        {
            Invalidate(ItemRect(index));
        }

        public IEnumerable<Item> SelectionEnumerator
        {
            get
            {
                IList<Item> list = List;
                IIntSet selection = (IIntSet) m_selection.Clone();
                foreach (int i in selection)
                {
                    yield return list[i];
                }
            }
        }

        int m_updateInterval = 0;
        Timer m_updateTimer = new Timer();

        public int UpdateInterval
        {
            get { return m_updateInterval; }
            set
            {
                m_updateInterval = value;
                if (m_updateInterval == 0)
                {
                    m_updateTimer.Stop();
                }
                else
                {
                    m_updateTimer.Interval = m_updateInterval;
                    m_updateTimer.Start();
                }
            }
        }

        void UiSetFocusedItem(int index)
        {
            InvalidateItem(FocusedItemIndex);
            m_focusedItemIndex = All.Clip(index);
            EnsureVisible(FocusedItemIndex);
            InvalidateItem(FocusedItemIndex);
            OnFocusedItemChanged();
        }

        void KbGotoItem(int index)
        {
            UiSetFocusedItem(index);
            if (ShiftPressed)
            {
                UiSetSelection(false, true, m_selectionBegin, FocusedItemIndex);
            }
            else
            {
                if (ControlPressed)
                {
                }
                else
                {
                    UiSetSelection(false, true, FocusedItemIndex, FocusedItemIndex);
                }
                m_selectionBegin = FocusedItemIndex;
            }
        }

        void MouseGotoItem(int index)
        {
            UiSetFocusedItem(index);
            if (ShiftPressed)
            {
                UiSetSelection(false, true, m_selectionBegin, FocusedItemIndex);
            }
            else
            {
                if (ControlPressed)
                {
                    UiSetSelection(true, !Selection.Contains(FocusedItemIndex), FocusedItemIndex, FocusedItemIndex);
                }
                else
                {
                    UiSetSelection(false, true, FocusedItemIndex, FocusedItemIndex);
                }
                m_selectionBegin = FocusedItemIndex;
            }
        }

        void KbSelectItem()
        {
            if (ShiftPressed)
            {
                UiSetSelection(false, true, m_selectionBegin, FocusedItemIndex);
            }
            else
            {
                if (ControlPressed)
                {
                    UiSetSelection(true, !Selection.Contains(FocusedItemIndex), FocusedItemIndex, FocusedItemIndex);
                }
                else
                {
                    UiSetSelection(false, true, FocusedItemIndex, FocusedItemIndex);
                }
            }
        }

        void UiSetSelection(bool add, bool on, int i1, int i2)
        {
            Interval interval = Interval.FromClosedInterval(i1, i2);
            if (!add)
            {
                Selection.Clear();
            }
            if (on)
            {
                Selection.Add(interval);
            }
            else
            {
                Selection.Remove(interval);
            }
            OnSelectionChanged();
        }

        bool ShiftPressed
        {
            get { return (ModifierKeys & Keys.Shift) == Keys.Shift; }
        }

        bool ControlPressed
        {
            get { return (ModifierKeys & Keys.Control) == Keys.Control; }
        }
    }
}
