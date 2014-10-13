using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sidi.IO
{
    [TestFixture]
    public class FileSystemTest : TestBase
    {
        [Test]
        public void RemoveDirectory()
        {
            var d = TestFile("ReadOnlyDirectory");
            var f = FileSystem.Current;
            f.EnsureDirectoryExists(d);
            f.RemoveDirectory(d);
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
            var copyTask = Task.Factory.StartNew(() => FileSystem.Current.CopyFile(from, to, progress, cts.Token));

            // cancel
            Thread.Sleep(50);
            cts.Cancel();

            copyTask.Wait();
            Assert.IsTrue(copyTask.IsCanceled);
        }

        void progress_ProgressChanged(object sender, CopyFileProgress e)
        {
            Console.WriteLine(e);
        }
    }
}
