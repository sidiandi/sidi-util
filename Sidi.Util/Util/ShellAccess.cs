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

            SHDocVw.InternetExplorer foregroundWindow = null;

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
