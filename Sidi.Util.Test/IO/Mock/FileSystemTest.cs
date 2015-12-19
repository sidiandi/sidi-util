using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.IO.Mock;
using System.Threading;
using System.IO;
using NUnit.Framework;
using Sidi.Extensions;
using Sidi.Test;

namespace Sidi.IO.Mock
{
    [TestFixture]
    public class FileSystemTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void UseMockFileSystem()
        {
            //! [Usage]
            using (var mockFs = new Sidi.IO.Mock.FileSystem())
            {
                mockFs.CreateRoot(new LPath(@"C:\"));

                var path = new LPath(mockFs, @"C:\temp\hello.txt");
                path.EnsureParentDirectoryExists();

                // write file
                var data = "hello, world";
                path.WriteAllText(data);

                // read file
                Assert.AreEqual(data, path.ReadAllText());

                // delete file
                path.EnsureFileNotExists();
                Assert.IsFalse(path.Exists);
            }
            //! [Usage]
        }

        [Test]
        public void UseMock()
        {
            using (var fs = new FileSystem())
            using (Sidi.IO.FileSystem.SetCurrent(fs))
            {
                var drive = LPath.GetDriveRoot('C');
                fs.CreateRoot(drive);
                fs.CreateRoot(LPath.GetUncRoot("server", "share"));
                fs.CurrentDirectory = drive;
                
                log.Info(fs.CurrentDirectory);

                var p = fs.CurrentDirectory.CatDir(@"temp\test\hello.txt");
                Assert.IsTrue(fs == p.FileSystem);

                var message = "hello";
                p.WriteAllText(message);
                Assert.AreEqual(message, p.ReadAllText());

                var pCopy = p.CatName(".copy");
                p.CopyFile(pCopy);
                Assert.AreEqual(message, pCopy.ReadAllText());

                var pMove = Sidi.IO.FileSystem.Current.GetDrives().First().CatDir(Enumerable.Range(0, 100).Select(x => String.Format("Directory {0}", x)));
                pMove.EnsureParentDirectoryExists();
                p.Move(pMove);
                Assert.AreEqual(message, pMove.ReadAllText());
                Assert.IsFalse(p.Exists);

                Assert.AreEqual(2, Find.AllFiles(@"C:\").Count());

                p.Lineage.Reverse().Skip(1).First().EnsureNotExists();

                log.Info(fs.GetAvailableDrives().ListFormat());
            }
        }
    }
}
