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

/*
    DirectoryDialog class for C#
		Version: 1.0		Date: 2002/04/02
*/
/*
    Copyright � 2002, The KPD-Team
    All rights reserved.
    http://www.mentalis.org/

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions
  are met:

    - Redistributions of source code must retain the above copyright
       notice, this list of conditions and the following disclaimer. 

    - Neither the name of the KPD-Team, nor the names of its contributors
       may be used to endorse or promote products derived from this
       software without specific prior written permission. 

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
  THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
  STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
  OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//<summary>The <strong>Org.Mentalis.Utilities.DirectoryDialog</strong> namespace provides a managed implementation of the SHBrowseForFolder function that creates a directory dialog.</summary>
namespace Sidi.Forms {
	///<summary>Contains parameters for the SHBrowseForFolder function and receives information about the folder selected by the user.</summary>
	[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Ansi )]
	internal struct BROWSEINFO {
		///<summary>Handle to the owner window for the dialog box.</summary>
		public IntPtr hWndOwner;
		///<summary>Address of an ITEMIDLIST structure specifying the location of the root folder from which to browse. Only the specified folder and its subfolders appear in the dialog box. This member can be IntPtr.Zero; in that case, the namespace root (the desktop folder) is used.</summary>
		public IntPtr pIDLRoot;
		///<summary>Address of a buffer to receive the display name of the folder selected by the user. The size of this buffer is assumed to be MAX_PATH bytes.</summary>
		public string pszDisplayName;
		///<summary>Address of a null-terminated string that is displayed above the tree view control in the dialog box. This string can be used to specify instructions to the user.</summary>
		public string lpszTitle;
		///<summary>Flags specifying the options for the dialog box.</summary>
		public int ulFlags;
		///<summary>Address of an application-defined function that the dialog box calls when an event occurs. This member can be IntPtr.Zero.</summary>
		public IntPtr lpfnCallback;
		///<summary>Application-defined value that the dialog box passes to the callback function, if one is specified.</summary>
		public int lParam;
		///<summary>Variable to receive the image associated with the selected folder. The image is specified as an index to the system image list.</summary>
		public int iImage;
	}

	///<summary>Specifies the type of items to browse for.</summary>
	[Flags]
    public enum BrowseForTypes {
		///<summary>Browse for computers.</summary>
		Computers = 0x1000,
		///<summary>Browse for directories.</summary>
		Directories = 0x1,
		///<summary>Browse for files and directories.</summary>
		///<remarks>Only works on shell version 4.71 and higher!</remarks>
		FilesAndDirectories = 0x4000, // Version 4.71
		///<summary>Browse for file system ancestors (an ancestor is a subfolder that is beneath the root folder in the namespace hierarchy.).</summary>
		FileSystemAncestors = 0x8
	}

	///<summary>Implements the SHBrowseForFolder function to show a common folder dialog.</summary>
	public class DirectoryDialog {
		///<summary>Initializes a new DirectoryDialog instance.</summary>
		public DirectoryDialog() {}
		///<summary>Shows the common folder dialog.</summary>
		///<param name="hWndOwner">The owner of the folder dialog.</param>
		///<returns>True when successful, false otherwise.</returns>
		protected bool RunDialog(IntPtr hWndOwner) {
			BROWSEINFO udtBI = new BROWSEINFO();
			IntPtr lpIDList;
			udtBI.pIDLRoot = IntPtr.Zero;
			udtBI.lpfnCallback = IntPtr.Zero;
			// set the owner of the window
			udtBI.hWndOwner = hWndOwner;
			// set the title of the window
			udtBI.lpszTitle =  Title;
			// set the flags of the dialog
			udtBI.ulFlags  = (int)BrowseFor;
			// create string buffer for display name
			StringBuilder buffer = new StringBuilder(MAX_PATH);
			buffer.Length = MAX_PATH;
			udtBI.pszDisplayName = buffer.ToString();
			// show the 'Browse for folder' dialog
			lpIDList = SHBrowseForFolder(ref udtBI);
			// examine the result
			if (!lpIDList.Equals(IntPtr.Zero)) {
				if (BrowseFor == BrowseForTypes.Computers) {
					m_Selected = udtBI.pszDisplayName.Trim();
				} else {
					StringBuilder path = new StringBuilder(MAX_PATH);
					// get the path from the IDList
					SHGetPathFromIDList(lpIDList, path);
					m_Selected = path.ToString();
				}
				// free the block of memory
				CoTaskMemFree(lpIDList);
				return true;
			} else {
				// user pressed cancel
				return false;
			}
		}
		///<summary>Shows the folder dialog.</summary>
		///<returns>DialogResult.OK when successful, DialogResult.Cancel otherwise.</returns>
		public DialogResult ShowDialog() {
			return ShowDialog(null);
		}
		///<summary>Shows the folder dialog.</summary>
		///<param name="owner">The owner of the folder dialog.</param>
		///<returns>DialogResult.OK when successful, DialogResult.Cancel otherwise.</returns>
		public DialogResult ShowDialog(IWin32Window owner) {
			IntPtr handle;
			if (owner != null)
				handle = owner.Handle;
			else
				handle = IntPtr.Zero;
			if (RunDialog(handle)) {
				return DialogResult.OK;
			} else {
				return DialogResult.Cancel;
			}
		}
		///<summary>Gets or sets the title of the dialog.</summary>
		///<value>A String representing the title of the dialog.</value>
		///<exceptions cref="ArgumentNullException">The specified value is null (VB.NET: Nothing)</exceptions>
		public string Title
        {
			get
            {
				return m_Title;
			}

			set 
            {
				if (value == null)
					throw new ArgumentNullException("value");
				m_Title = value;
			}
		}
		///<summary>Gets the selected item.</summary>
		///<value>A String representing the selected item.</value>
		public string Selected {
			get {
				return m_Selected;
			}
		}
		///<summary>Gets or sets the type of items to browse for.</summary>
		///<value>One of the BrowseForTypes values.</value>
		public BrowseForTypes BrowseFor {
			get {
				return m_BrowseFor;
			}
			set {
				m_BrowseFor = value;
			}
		}
		// private declares
		///<summary>Frees a block of task memory previously allocated through a call to the CoTaskMemAlloc or CoTaskMemRealloc function.</summary>
		///<param name="hMem">Pointer to the memory block to be freed.</param>
		[ DllImport( "ole32.dll", CharSet=CharSet.Ansi )]
		private static extern void CoTaskMemFree(IntPtr hMem);
		///<summary>The lstrcat function appends one string to another.</summary>
		///<param name="lpString1">Pointer to a null-terminated string. The buffer must be large enough to contain both strings.</param>
		///<param name="lpString2">Pointer to the null-terminated string to be appended to the string specified in the lpString1 parameter.</param>
		///<returns>If the function succeeds, the return value is a pointer to the buffer.<br>If the function fails, the return value is IntPtr.Zero.</br></returns>
		[ DllImport( "kernel32.dll", CharSet=CharSet.Ansi )]
		private static extern IntPtr lstrcat(string lpString1, string lpString2);
		///<summary>Displays a dialog box that enables the user to select a shell folder.</summary>
		///<param name="lpbi">Address of a BROWSEINFO structure that contains information used to display the dialog box.</param>
		///<returns>Returns the address of an item identifier list that specifies the location of the selected folder relative to the root of the namespace. If the user chooses the Cancel button in the dialog box, the return value is IntPtr.Zero.</returns>
		[ DllImport( "shell32.dll", CharSet=CharSet.Ansi )]
		private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);
		///<summary>Converts an item identifier list to a file system path.</summary>
		///<param name="pidList">Address of an item identifier list that specifies a file or directory location relative to the root of the namespace (the desktop).</param>
		///<param name="lpBuffer">Address of a buffer to receive the file system path. This buffer must be at least MAX_PATH characters in size.</param>
		///<returns>Returns a nonzero value if successful, or zero otherwise.</returns>
		[ DllImport( "shell32.dll", CharSet=CharSet.Ansi )]
		private static extern int SHGetPathFromIDList(IntPtr pidList, StringBuilder lpBuffer);
		// private variables
		///<summary>Specifies the maximum number of characters in a pathname.</summary>
		///<remarks>The value of this integer is 260.</remarks>
		private const int MAX_PATH = 260;
		///<summary>Holds a BrowseForTypes value that indicates what type of items to browse for.</summary>
		private BrowseForTypes m_BrowseFor = BrowseForTypes.Directories;
		///<summary>Holds the string with the title of the directory dialog.</summary>
		private string m_Title = "";
		///<summary>The a string with the selected item.</summary>
		private string m_Selected = "";
	}
}
