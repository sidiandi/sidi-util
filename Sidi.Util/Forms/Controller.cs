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

namespace Sidi.Forms
{
    public class Controller : Control
    {
        bool m_consumed = false;
        MyMessageFilter m_messageFilter;
        
        public Controller()
        {
            m_messageFilter = new MyMessageFilter(this);
        }

        List<IntPtr> m_handles = new List<IntPtr>();
        List<IntPtr> m_ignoreHandles = new List<IntPtr>();
        
        public void Attach(Control c)
        {
            m_handles.Add(c.Handle);
        }

        protected override void  Dispose(bool disposing)
        {
            if (disposing)
            {
                m_messageFilter.Dispose();
            }
 	        base.Dispose(disposing);
        }

        public void Ignore(Control c)
        {
            m_ignoreHandles.Add(c.Handle);
        }

        public void Consume()
        {
            m_consumed = true;
        }

        public bool Consumed
        {
            get { return m_consumed; }
            set { m_consumed = value; }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true;
        }

        class MyMessageFilter : IMessageFilter, IDisposable
        {
            Controller m_controller;
            
            public MyMessageFilter(Controller c)
            {
                m_controller = c;
                Application.AddMessageFilter(this);
            }

            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {
                if (true)
                {
                    NativeWindow nw = NativeWindow.FromHandle(m.HWnd);
                    Control c = Control.FromHandle(m.HWnd);
                    if (c == null)
                    {
                        return false;
                    }
                    if (c.ToString().Contains("Menu"))
                    {
                        return false;
                    }
                }
                
                if (!(
                    (NativeMethods.WM_MOUSEFIRST <= m.Msg && m.Msg <= NativeMethods.WM_MOUSELAST) ||
                    (NativeMethods.WM_KEYFIRST <= m.Msg && m.Msg <= NativeMethods.WM_KEYLAST)
                ))
                {
                    return false;
                }

                if (m_controller.m_ignoreHandles.Contains(m.HWnd))
                {
                    return false;
                }

                if (m_controller.m_handles.Contains(m.HWnd))
                {
                    m_controller.Consumed = false;
                    NativeMethods.SendMessage(m_controller.Handle, m.Msg, m.WParam, m.LParam);
                    return m_controller.Consumed;
                }
                return false;
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                Application.RemoveMessageFilter(this);
            }

            #endregion
        }
    }
}
