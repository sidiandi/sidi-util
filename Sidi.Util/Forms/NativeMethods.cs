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
using System.Runtime.InteropServices;

#pragma warning disable 649

namespace Sidi.Forms
{
    internal static class NativeMethods
    {
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSELAST = 0x020D;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;


        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;
        public const int WM_DEADCHAR = 0x0103;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_SYSCHAR = 0x0106;
        public const int WM_SYSDEADCHAR = 0x0107;
        public const int WM_UNICHAR = 0x0109;
        public const int WM_KEYLAST = 0x0109;

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        ///<summary>Frees a block of task memory previously allocated through a call to the CoTaskMemAlloc or CoTaskMemRealloc function.</summary>
        ///<param name="hMem">Pointer to the memory block to be freed.</param>
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        internal static extern void CoTaskMemFree(IntPtr hMem);
        ///<summary>Displays a dialog box that enables the user to select a shell folder.</summary>
        ///<param name="lpbi">Address of a BROWSEINFO structure that contains information used to display the dialog box.</param>
        ///<returns>Returns the address of an item identifier list that specifies the location of the selected folder relative to the root of the namespace. If the user chooses the Cancel button in the dialog box, the return value is IntPtr.Zero.</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);
        ///<summary>Converts an item identifier list to a file system path.</summary>
        ///<param name="pidList">Address of an item identifier list that specifies a file or directory location relative to the root of the namespace (the desktop).</param>
        ///<param name="lpBuffer">Address of a buffer to receive the file system path. This buffer must be at least MAX_PATH characters in size.</param>
        ///<returns>Returns a nonzero value if successful, or zero otherwise.</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHGetPathFromIDList(IntPtr pidList, StringBuilder lpBuffer);

        #region Unsafe types
        internal unsafe struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public void* hwndTarget;
        };

        internal unsafe struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public void* hDevice;
            public void* wParam;
        };

        internal unsafe struct RAWINPUTHKEYBOARD
        {
            public RAWINPUTHEADER header;
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        };

        internal unsafe struct RAWINPUTMOUSE
        {
            public RAWINPUTHEADER header;
            public ushort usFlags;
            public ushort usButtonFlags;
            public ushort usButtonData;
            public uint ulRawButtons;
            public int lLastX;
            public int lLastY;
            public uint ulExtraInformation;
        };

        #endregion
        [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool RegisterRawInputDevices(RAWINPUTDEVICE* rawInputDevices, uint numDevices, uint size);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern unsafe int GetRawInputData(void* hRawInput,
            uint uiCommand,
            byte* pData,
            uint* pcbSize,
            uint cbSizeHeader
            );

    }
}
