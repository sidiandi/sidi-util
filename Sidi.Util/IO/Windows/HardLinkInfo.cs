using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO.Windows
{
    class HardLinkInfo : IHardLinkInfo
    {
        public static HardLinkInfo Get(LPath path)
        {
            return new HardLinkInfo(path);
        }

        HardLinkInfo(LPath path)
        {
            this.path = path;
        }

        LPath path;

        Sidi.IO.Windows.FileSystem FileSystem
        {
            get
            {
                return (Sidi.IO.Windows.FileSystem) path.FileSystem;
            }
        }

        NativeMethods.BY_HANDLE_FILE_INFORMATION GetByHandleFileInformation()
        {
            using (var handle = NativeMethods.CreateFile(
                FileSystem.GetLongPathApiParameter(path),
                System.IO.FileAccess.Read,
                System.IO.FileShare.Read, IntPtr.Zero,
                System.IO.FileMode.Open, System.IO.FileAttributes.Normal,
                IntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    throw new Win32Exception(path);
                }

                var fileInfo = new NativeMethods.BY_HANDLE_FILE_INFORMATION();
                NativeMethods.GetFileInformationByHandle(handle, out fileInfo)
                    .CheckApiCall(path);
                return fileInfo;
            }
        }

        public int FileLinkCount
        {
            get
            {
                return (int)GetByHandleFileInformation().NumberOfLinks;
            }
        }

        public long FileIndex
        {
            get
            {
                var fileInfo = GetByHandleFileInformation();
                return (long)(((ulong)fileInfo.FileIndexHigh << 32) + (ulong)fileInfo.FileIndexLow);
            }
        }

        static string[] GetFileSiblingHardLinks(LPath filepath)
        {
            var fs = (Sidi.IO.Windows.FileSystem)filepath.FileSystem;

            List<string> result = new List<string>();
            uint stringLength = 256;
            StringBuilder sb = new StringBuilder(256);
            NativeMethods.GetVolumePathName(fs.GetLongPathApiParameter(filepath), sb, stringLength);
            string volume = sb.ToString();
            sb.Length = 0; stringLength = 256;
            IntPtr findHandle = NativeMethods.FindFirstFileNameW(fs.GetLongPathApiParameter(filepath), 0, ref stringLength, sb);
            if (findHandle.ToInt32() != -1)
            {
                do
                {
                    StringBuilder pathSb = new StringBuilder(volume, 256);
                    NativeMethods.PathAppend(pathSb, sb.ToString());
                    result.Add(pathSb.ToString());
                    sb.Length = 0; stringLength = 256;
                } while (NativeMethods.FindNextFileNameW(findHandle, ref stringLength, sb));
                NativeMethods.FindClose(findHandle);
                return result.ToArray();
            }
            return null;
        }

        public IList<LPath> HardLinks
        {
            get
            {
                return GetFileSiblingHardLinks(path)
                    .Select(x => new LPath(x))
                    .ToList();
            }
        }
    }
}
