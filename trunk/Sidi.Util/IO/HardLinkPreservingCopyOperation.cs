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
        void CheckVolume(LPath path, ref LPath volume)
        {
            if (volume == null)
            {
                volume = path.VolumePath;
            }
            else
            {
                if (!volume.Equals(path.VolumePath))
                {
                    throw new Exception(String.Format("Volume of {2} must be {0} but is {1}", volume, path.VolumePath, path));
                }
            }
        }

        public void Copy(LPath source, LPath destination)
        {
            CheckVolume(source, ref sourceVolume);
            CheckVolume(destination, ref destinationVolume);
            if (source.IsDirectory)
            {
                var children = source.Children;
                if (children.Count > 0)
                {
                    destination.EnsureDirectoryExists();
                    foreach (var c in source.Children)
                    {
                        Copy(c, destination.CatDir(c.FileName));
                    }
                }
            }
            else
            {
                var fi = source.Info.FileIndex;
                LPath existingDestinationFile;
                if (sourceFileIdToDestinationPath.TryGetValue(fi, out existingDestinationFile))
                {
                    // re-create hard link
                    
                    LFile.CreateHardLink(destination, existingDestinationFile);
                }
                else
                {
                    LFile.Copy(source, destination);
                    sourceFileIdToDestinationPath[fi] = destination;
                }
            }
        }

        Dictionary<ulong, LPath> sourceFileIdToDestinationPath = new Dictionary<ulong, LPath>();
        LPath sourceVolume;
        LPath destinationVolume;
    }
}
