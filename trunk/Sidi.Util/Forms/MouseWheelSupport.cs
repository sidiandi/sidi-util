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
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Sidi.Forms
{
    class Win32Messages
    {
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSELAST = 0x020D;
        public const int WM_MOUSEMOVE                    = 0x0200;
        public const int WM_LBUTTONDOWN                  = 0x0201;
        public const int WM_LBUTTONUP                    = 0x0202;
        public const int WM_LBUTTONDBLCLK                = 0x0203;
        public const int WM_RBUTTONDOWN                  = 0x0204;
        public const int WM_RBUTTONUP                    = 0x0205;
        public const int WM_RBUTTONDBLCLK                = 0x0206;
        public const int WM_MBUTTONDOWN                  = 0x0207;
        public const int WM_MBUTTONUP                    = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;


        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYDOWN                     =0x0100;
        public const int WM_KEYUP                       =0x0101;
        public const int WM_CHAR                        =0x0102;
        public const int WM_DEADCHAR                    =0x0103;
        public const int WM_SYSKEYDOWN                  =0x0104;
        public const int WM_SYSKEYUP                    =0x0105;
        public const int WM_SYSCHAR                     =0x0106;
        public const int WM_SYSDEADCHAR                 =0x0107;
        public const int WM_UNICHAR                     =0x0109;
        public const int WM_KEYLAST                     =0x0109;

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }
    
    /// <summary>
    /// Mouse Wheel Support.
    /// </summary>
    public class MouseWheelSupport : NativeWindow, IMessageFilter
    {
        public const int Unit = 120;

        private Form parent;

        private MouseWheelSupport() { }
        public MouseWheelSupport(Form f)
        {

            this.parent = f;
            this.parent.HandleCreated += new EventHandler(this.OnParentHandleCreated);
            this.parent.HandleDestroyed += new EventHandler(this.OnParentHandleDestroyed);

        }

        private void OnParentHandleCreated(object sender, EventArgs e)
        {
            Application.AddMessageFilter(this);
        }

        private void OnParentHandleDestroyed(object sender, EventArgs e)
        {
            Application.RemoveMessageFilter(this);
        }

        // IMessageFilter impl.
        public virtual bool PreFilterMessage(ref Message m)
        {

            // Listen for operating system messages
            switch (m.Msg)
            {
                case Win32Messages.WM_MOUSEWHEEL:

                    // redirect the wheel message
                    Control child = GetTopmostChild(parent);

                    if (child != null)
                    {

                        if (m.HWnd == child.Handle && child.Focused)
                            return false;	// control is focused, so it should handle the wheel itself

                        if (m.HWnd != child.Handle)
                        {	// no recursion, please. Redirect message...
                            // Point p = child.PointToClient(Control.MousePosition);
                            Point p = Control.MousePosition;
                            Win32Messages.PostMessage(child.Handle, Win32Messages.WM_MOUSEWHEEL, m.WParam, new IntPtr(p.X + (p.Y << 16)));
                            return true;
                        }

                        return false;
                    }
                    break;
            }
            return false;
        }

        private Control GetTopmostChild(Control ctrl)
        {
            if (ctrl.Controls.Count > 0)
            {
                Point p = ctrl.PointToClient(Control.MousePosition);
                Control child = ctrl.GetChildAtPoint(p);
                if (child != null)
                {
                    return GetTopmostChild(child);
                }
                else
                {
                    return ctrl;
                }
            }
            else
            {
                return ctrl;
            }
        }
    }
}
