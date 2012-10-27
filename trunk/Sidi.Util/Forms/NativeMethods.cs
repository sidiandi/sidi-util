using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Sidi.Forms
{
    internal static class NativeMethods
    {
        ///<summary>Frees a block of task memory previously allocated through a call to the CoTaskMemAlloc or CoTaskMemRealloc function.</summary>
        ///<param name="hMem">Pointer to the memory block to be freed.</param>
        [DllImport("ole32.dll", CharSet = CharSet.Ansi)]
        internal static extern void CoTaskMemFree(IntPtr hMem);
        ///<summary>The lstrcat function appends one string to another.</summary>
        ///<param name="lpString1">Pointer to a null-terminated string. The buffer must be large enough to contain both strings.</param>
        ///<param name="lpString2">Pointer to the null-terminated string to be appended to the string specified in the lpString1 parameter.</param>
        ///<returns>If the function succeeds, the return value is a pointer to the buffer.<br>If the function fails, the return value is IntPtr.Zero.</br></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        internal static extern IntPtr lstrcat(string lpString1, string lpString2);
        ///<summary>Displays a dialog box that enables the user to select a shell folder.</summary>
        ///<param name="lpbi">Address of a BROWSEINFO structure that contains information used to display the dialog box.</param>
        ///<returns>Returns the address of an item identifier list that specifies the location of the selected folder relative to the root of the namespace. If the user chooses the Cancel button in the dialog box, the return value is IntPtr.Zero.</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        internal static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);
        ///<summary>Converts an item identifier list to a file system path.</summary>
        ///<param name="pidList">Address of an item identifier list that specifies a file or directory location relative to the root of the namespace (the desktop).</param>
        ///<param name="lpBuffer">Address of a buffer to receive the file system path. This buffer must be at least MAX_PATH characters in size.</param>
        ///<returns>Returns a nonzero value if successful, or zero otherwise.</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        internal static extern int SHGetPathFromIDList(IntPtr pidList, StringBuilder lpBuffer);
    }
}
