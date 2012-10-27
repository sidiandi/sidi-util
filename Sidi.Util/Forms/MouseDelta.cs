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
using System.Diagnostics;
using System.ComponentModel;

namespace Sidi.Forms
{
    public class MouseDeltaEventArgs : EventArgs
    {
        public MouseDeltaEventArgs(Point delta, Point total)
        {
            m_delta = delta;
            m_total = total;
        }

        public Point Delta { get { return m_delta; } }
        public Point Total { get { return m_total; } }

        Point m_delta;
        Point m_total;
    }

    public delegate void MouseDeltaEventHandler(object sender, MouseDeltaEventArgs e);

    public class MouseDelta : IDisposable, IMessageFilter
    {
        Point m_delta = new Point();
        Point m_total = new Point();
        Point m_initialCursorPosition;

        public event MouseDeltaEventHandler Move;

        protected virtual void OnMouseDelta(MouseDeltaEventArgs e)
        {
            if (Move != null)
            {
                Move(this, e);
            }
        }
        
        public MouseDelta()
        {
            m_initialCursorPosition = Cursor.Position;
            Application.AddMessageFilter(this);
            Cursor.Hide();
            RegisterMouseInput();
        }

        void RegisterMouseInput()
        {
            // Register for mouse notification
            unsafe
            {
                try
                {
                    NativeMethods.RAWINPUTDEVICE myRawDevice = new NativeMethods.RAWINPUTDEVICE();
                    myRawDevice.usUsagePage = 0x01;
                    myRawDevice.usUsage = 0x02;
                    myRawDevice.dwFlags = 0;
                    myRawDevice.hwndTarget = (void*) 0;

                    if (NativeMethods.RegisterRawInputDevices(&myRawDevice, 1, (uint)sizeof(NativeMethods.RAWINPUTDEVICE)) == false)
                    {
                        int err = Marshal.GetLastWin32Error();
                        throw new Win32Exception(err, "ListeningWindow::RegisterRawInputDevices");
                    }
                }

                catch { throw; }
            }
        }

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            // Listen for operating system messages
            switch (m.Msg)
            {
                case WM_INPUT:
					{
						try
						{
							unsafe
							{
								uint dwSize, receivedBytes;
                                uint sizeof_RAWINPUTHEADER = (uint)(sizeof(NativeMethods.RAWINPUTHEADER));

								// Find out the size of the buffer we have to provide
                                int res = NativeMethods.GetRawInputData(m.LParam.ToPointer(), RID_INPUT, null, &dwSize, sizeof_RAWINPUTHEADER);
						
								if(res == 0)
								{
									// Allocate a buffer and ...
									byte* lpb = stackalloc byte[(int)dwSize];

									// ... get the data
                                    receivedBytes = (uint)NativeMethods.GetRawInputData((NativeMethods.RAWINPUTMOUSE*)(m.LParam.ToPointer()), RID_INPUT, lpb, &dwSize, sizeof_RAWINPUTHEADER);
									if ( receivedBytes == dwSize )
									{
                                        NativeMethods.RAWINPUTMOUSE* mouseData = (NativeMethods.RAWINPUTMOUSE*)lpb;

										// Finally, analyze the data
										if(mouseData->header.dwType == RIM_TYPEMOUSE)
										{
                                            m_delta.X = mouseData->lLastX;
                                            m_delta.Y = mouseData->lLastY;
                                            m_total.Offset(m_delta);
                                            MouseDeltaEventArgs e = new MouseDeltaEventArgs(m_delta, m_total);
                                            OnMouseDelta(e);
                                        }
									}
									else
									{
										string errMsg = string.Format("WndProc::GetRawInputData (2) received {0} bytes while expected {1} bytes", receivedBytes, dwSize);
										throw new Exception(errMsg);
									}
								}
								else
								{
									string errMsg = string.Format("WndProc::GetRawInputData (1) returned non zero value ({0})", res);
									throw new Exception(errMsg);
								}
							}
						}

						catch {throw;}
					}
                    break;
                default:
                    break;
            }
            return false;        
        }

        #endregion

        #region Win32 interop

        // In case you want to have a comprehensive overview of calling conventions follow the next link:
        // http://www.codeproject.com/cpp/calling_conventions_demystified.asp

        #endregion

        #region Declarations

        private const int
            WS_CLIPCHILDREN = 0x02000000,
            WM_INPUT = 0x00FF,
            RIDEV_INPUTSINK = 0x00000100,
            RID_INPUT = 0x10000003,
            RIM_TYPEMOUSE = 0,
            RIM_TYPEKEYBOARD = 1;

        #endregion




        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
                Application.RemoveMessageFilter(this);
                Cursor.Show();
                Cursor.Position = m_initialCursorPosition;
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~MouseDelta()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
        }
}
