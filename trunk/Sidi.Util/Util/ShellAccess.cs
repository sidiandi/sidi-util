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
using Sidi.IO;
using System.Collections;
using SHDocVw;

namespace Sidi.Util
{
    public class Shell
    {
        Shell32.Shell shell = new Shell32.Shell();

        IEnumerable<IntPtr> GetZOrder(IntPtr parentWindow)
        {
            for (
                IntPtr w = Sidi.IO.NativeMethods.GetTopWindow(parentWindow); 
                w != IntPtr.Zero;
                w = Sidi.IO.NativeMethods.GetWindow(w, (uint) Sidi.IO.NativeMethods.GetWindow_Cmd.GW_HWNDNEXT))
            {
                yield return w;
            }
        }

        public SHDocVw.InternetExplorer GetForegroundWindow()
        {
            var shellWindows = ((IEnumerable)shell.Windows()).Cast<InternetExplorer>().ToDictionary(
                x => (IntPtr)x.HWND, x => x);

            return GetZOrder(IntPtr.Zero)
                .Select(hwnd =>
                    {
                        InternetExplorer x = null;
                        shellWindows.TryGetValue(hwnd, out x);
                        return x;
                    })
                .Where(x => x != null)
                .FirstOrDefault();
        }

        public IEnumerable<LPath> SelectedFiles
        {
            get
            {
                IntPtr handle = NativeMethods.GetForegroundWindow();

                var w = GetForegroundWindow();
                var items = ((Shell32.IShellFolderViewDual2)w.Document).SelectedItems();
                return items.Cast<Shell32.FolderItem>()
                    .Select(i => new LPath(i.Path));
            }
        }
    }
}
