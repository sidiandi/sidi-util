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

        [Test]
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

            Assert.Throws<AggregateException>(() => copyTask.Wait());
        }

        [Test, Ignore("reading raw disk. Requires elevation")]
        public void ReadDisk()
        {
            var disk = new LPath(@"\\.\PhysicalDrive1");
            using (var f = disk.OpenRead())
            {
                var b = new byte[0x200];
                f.Read(b, 0, b.Length);
                Sidi.Util.HexDump.Write(b, Console.Out);
            }
        }

        void progress_ProgressChanged(object sender, CopyFileProgress e)
        {
            Console.WriteLine(e);
        }
    }
}
