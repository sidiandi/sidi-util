using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.IO
{
    [TestFixture]
    public class FileSystemTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Drives()
        {
            var available = FS.GetAvailableDrives().ToList();
            log.Info(available.ListFormat());
            Assert.IsTrue(available.Any());

            var drives = FS.GetDrives();
            log.Info(drives.ListFormat());
            Assert.IsTrue(drives.Any());
        }

        IFileSystem FS = FileSystem.Current;

        [Test]
        public void RemoveDirectory()
        {
            var d = TestFile("ReadOnlyDirectory");
            d.EnsureDirectoryExists();
            d.RemoveDirectory();
            Assert.IsFalse(d.Exists);
        }

        [Test, ExpectedException(typeof(AggregateException))]
        public void CopyFileCancel()
        {
            // create big file
            var from = TestFile("from");
            from.EnsureFileNotExists();
            long size = 1 * 1024 * 1024 * 1024;
            using (var w = from.OpenWrite())
            {
                w.Seek(size-1, System.IO.SeekOrigin.Begin);
                w.WriteByte(0);
            }
            Assert.AreEqual(size, from.Info.Length);

            var to = TestFile("to");
            to.EnsureFileNotExists();
            
            // start copy
            var progress = new Progress<CopyFileProgress>();
            progress.ProgressChanged += progress_ProgressChanged;
            var cts = new CancellationTokenSource();
            var copyTask = Task.Factory.StartNew(() => from.CopyFile(to, progress, cts.Token));

            // cancel
            Thread.Sleep(50);
            cts.Cancel();

            copyTask.Wait();
            Assert.IsTrue(copyTask.IsCanceled);
        }

        /*
        [Test, Explicit("experimental program")]
        public void WipeDisk()
        {
            var disk = new LPath(@"\\.\PhysicalDrive1");
            using (var f = FileSystem.Current.Open(disk, System.IO.FileMode.Open))
            {
                var b = new byte[0x1000];
                f.Seek(0xF8AEA01700L, System.IO.SeekOrigin.Begin);
                Console.WriteLine(f.Position);
                f.Write(b, 0, b.Length);
                f.Read(b, 0, b.Length);
                Console.WriteLine(b.HexString());
            }
        }
         */

        void progress_ProgressChanged(object sender, CopyFileProgress e)
        {
            Console.WriteLine(e);
        }
    }
}
