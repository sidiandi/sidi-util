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
using Sidi.Extensions;

namespace Sidi.Util
{
    public static class InternetExplorerExtension
    {
        public static IEnumerable<LPath> GetSelectedFiles(this SHDocVw.InternetExplorer w)
        {
            if (w == null)
            {
                return Enumerable.Empty<LPath>();
            }

            var view = ((Shell32.IShellFolderViewDual2)w.Document);

            var items = view.SelectedItems()
                .OfType<Shell32.FolderItem>()
                .Select(i => new LPath(i.Path))
                .ToList();

            if (items.Any())
            {
                return items;
            }

            Console.WriteLine(w.LocationURL);
            return new LPath[] { LPath.Parse(w.LocationURL) };
        }
    }

    public class Shell
    {
        Shell32.Shell shell = new Shell32.Shell();

        IEnumerable<IntPtr> GetZOrder(IntPtr parentWindow)
        {
            for (
                IntPtr w = Sidi.IO.Windows.NativeMethods.GetTopWindow(parentWindow); 
                w != IntPtr.Zero;
                w = Sidi.IO.Windows.NativeMethods.GetWindow(w, (uint)Sidi.IO.Windows.NativeMethods.GetWindow_Cmd.GW_HWNDNEXT))
            {
                yield return w;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Topmost Windows Explorer or Internet Explorer window or null if no such window is open</returns>
        public bool TryGetForegroundWindow(out SHDocVw.InternetExplorer explorerWindow)
        {
            var shellWindows = ((IEnumerable)shell.Windows()).OfType<InternetExplorer>()
                .Select(x => { try { return new { Handle = x.HWND, Instance = x }; } catch { return null; } })
                .Where(x => x != null)
                .ToDictionary(x => x.Handle, x => x.Instance);

            explorerWindow = GetZOrder(IntPtr.Zero)
                .Select(hwnd => shellWindows.GetValueOrDefault((int)hwnd, null))
                .FirstOrDefault();

            return explorerWindow != null;
        }

        public SHDocVw.InternetExplorer GetForegroundWindow()
        {
            InternetExplorer e;
            if (TryGetForegroundWindow(out e))
            {
                return e;
            }
            else
            {
                throw new InvalidOperationException("The foreground window is not a Windows Explorer window.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>All Windows Explorer or Internet Explorer windows, sorted from topmost.</returns>
        public IEnumerable<SHDocVw.InternetExplorer> GetExplorerWindows()
        {
            var shellWindows = ((IEnumerable)shell.Windows()).OfType<InternetExplorer>()
                .Select(x => { try { return new { Handle = x.HWND, Instance = x }; } catch { return null; } })
                .Where(x => x != null)
                .ToDictionary(x => x.Handle, x => x.Instance);

            return GetZOrder(IntPtr.Zero)
                .Select(hwnd => shellWindows.GetValueOrDefault((int)hwnd, null))
                .Where(x => x != null);
        }

        /// <summary>
        /// Get selected files from topmost explorer window orr empty list if no such window exists
        /// </summary>
        public IEnumerable<LPath> SelectedFiles
        {
            get
            {
                InternetExplorer e;
                if (TryGetForegroundWindow(out e))
                {
                    return e.GetSelectedFiles();
                }
                else
                {
                    return Enumerable.Empty<LPath>();
                }
            }
        }

        public LPath GetOpenDirectory()
        {
            var w = GetForegroundWindow();
            var url = w.LocationURL;
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new InvalidOperationException("No directory open in Windows shell");
            }

            return new LPath(new Uri(url).LocalPath);
        }
    }
}
