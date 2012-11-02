using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

#pragma warning disable 219

namespace Sidi.IO
{
    /// <summary>
    /// Checks if Sidi.IO names do not collide with System.IO names
    /// </summary>
    [TestFixture]
    public class NamespaceCompatibilityTest
    {
        /// <summary>
        /// Test names of static classes
        /// </summary>
        [Test]
        public void TestNames()
        {
            FileUtil.GetFileSystemInfo(Paths.BinDir);
            Console.WriteLine(Paths.BinDir);

            Copy copy = null;
            CopyOp copyOp = null;
            CopyProgress copyProgress = null;
            FileSystemInfo fsInfo = null;
            FileType fileType = null;
            Find find = null;
            FindConfig findConfig = null;
            LDirectory lDirectory = null;
            LFile lFile = null;
            LPath lPath = null;
            PathList pathList = null;
            SearchPath searchPath = null;
        }
    }
}
