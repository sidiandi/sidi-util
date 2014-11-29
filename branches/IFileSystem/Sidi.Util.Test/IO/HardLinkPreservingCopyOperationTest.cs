using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Sidi.Test;

namespace Sidi.IO
{
    [TestFixture]
    public class HardLinkPreservingCopyOperationTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string SafeGetDriveFormat(DriveInfo d)
        {
            try
            {
                return d.DriveFormat;
            }
            catch
            {
                return String.Empty;
            }
        }
        
        [Test, Explicit("requires second NTFS HDD")]
        public void Test()
        {
            var sourceDir = TestFile("copy-hardlink-test");

            // determine target drive
            var targetDrive = System.IO.DriveInfo.GetDrives()
                .FirstOrDefault(x => SafeGetDriveFormat(x).Equals("NTFS") && !new LPath(x.RootDirectory.FullName).IsAncestor(sourceDir));

            if (targetDrive == null)
            {
                log.Warn("Test skipped. No target NTFS drive found.");
                return;
            }
            
            var count = 10;
            sourceDir.EnsureNotExists();
            sourceDir.EnsureDirectoryExists();
            var f = sourceDir.CatDir("orig");
            f.WriteAllText("hello");
            for (int i = 0; i < count; ++i)
            {
                FileSystem.Current.CreateHardLink(f.CatName(i.ToString()), f);
            }
            Assert.AreEqual(count + 1, f.HardLinkInfo.FileLinkCount);

            var destinationDir = new LPath(targetDrive.RootDirectory.FullName).CatDir(@"temp\copy-hardlink-test");
            destinationDir.EnsureNotExists();
            destinationDir.EnsureDirectoryExists();

            var co = new HardLinkPreservingCopyOperation();
            co.Copy(sourceDir, destinationDir);
            var c = destinationDir.Children;
            Assert.AreEqual(count + 1, c.Count);
            var g = c.GroupBy(x => x.HardLinkInfo.FileIndex);

            Assert.AreEqual(1, g.Count());

            destinationDir.EnsureNotExists();
        }
    }
}
