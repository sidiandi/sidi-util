// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
                    (Win32Messages.WM_MOUSEFIRST <= m.Msg && m.Msg <= Win32Messages.WM_MOUSELAST) ||
                    (Win32Messages.WM_KEYFIRST <= m.Msg && m.Msg <= Win32Messages.WM_KEYLAST)
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
                    Win32Messages.SendMessage(m_controller.Handle, m.Msg, m.WParam, m.LParam);
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
