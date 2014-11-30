using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public static class LPathExtensions
    {
        public static void WriteAllText(this LPath path, string text)
        {
            path.EnsureParentDirectoryExists();
            using (var w = path.WriteText())
            {
                w.Write(text);
            }
        }

        public static string ReadAllText(this LPath path)
        {
            using (var r = path.ReadText())
            {
                return r.ReadToEnd();
            }
        }

        public static void Move(this LPath source, LPath destination)
        {
            source.FileSystem.Move(source, destination);
        }

        public static void RemoveDirectory(this LPath path)
        {
            path.FileSystem.RemoveDirectory(path);
        }

        public static void DeleteFile(this LPath path)
        {
            path.FileSystem.DeleteFile(path);
        }

        public static void CopyFile(
            this LPath source, 
            LPath dest, 
            IProgress<CopyFileProgress> progress = null,
            System.Threading.CancellationToken cancellationToken = new CancellationToken(), 
            CopyFileOptions options = null)
        {
            source.FileSystem.CopyFile(source, dest, progress, cancellationToken, options);
        }

        public static void CreateHardLink(this LPath existingFilename, LPath newHardlinkFilename)
        {
            existingFilename.FileSystem.CreateHardLink(newHardlinkFilename, existingFilename);
        }
    }
}
