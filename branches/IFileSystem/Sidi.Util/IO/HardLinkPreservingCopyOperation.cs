using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO
{
    /// <summary>
    /// Copies files from one volume to another while preserving hardlinks
    /// This means that is two files on the source volume are hard-linked,
    /// the destination files will be hard-linked together as well.
    /// </summary>
    public class HardLinkPreservingCopyOperation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        void CheckVolume(LPath path, ref LPath volume)
        {
            if (volume == null)
            {
                volume = path.Root;
            }
            else
            {
                if (!volume.Equals(path.Root))
                {
                    throw new Exception(String.Format("Volume of {2} must be {0} but is {1}", volume, path.Root, path));
                }
            }
        }

        Operation op = new Operation()
        {
            Fast = true,
            Overwrite = true
        };

        public void Copy(LPath source, LPath destination)
        {
            CheckVolume(source, ref sourceVolume);
            CheckVolume(destination, ref destinationVolume);

            CopyRecursive(source, destination);

        }

        void CopyRecursive(LPath source, LPath destination)
        {
            if (source.IsDirectory)
            {
                var children = source.Children;
                if (children.Count > 0)
                {
                    destination.EnsureDirectoryExists();
                    foreach (var c in source.Children)
                    {
                        CopyRecursive(c, destination.CatDir(c.FileName));
                    }
                }
            }
            else
            {
                var fi = source.HardLinkInfo.FileIndex;
                LPath existingDestinationFile;
                if (sourceFileIdToDestinationPath.TryGetValue(fi, out existingDestinationFile))
                {
                    // re-create hard link
                    log.InfoFormat("re-create link at {0} as {1} -> {2}",
                        source,
                        destination, existingDestinationFile);
                    fileSystem.CreateHardLink(destination, existingDestinationFile);
                }
                else
                {
                    op.CopyFile(source, destination);
                    sourceFileIdToDestinationPath[fi] = destination;
                }
            }
        }

        Dictionary<long, LPath> sourceFileIdToDestinationPath = new Dictionary<long, LPath>();
        LPath sourceVolume;
        LPath destinationVolume;
        IFileSystem fileSystem = FileSystem.Current;
    }
}
