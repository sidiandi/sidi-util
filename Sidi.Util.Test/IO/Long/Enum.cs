using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.IO;
using NUnit.Framework;
using Sidi.CommandLine;

namespace Sidi.IO.Long
{
    public class FileTypeTest
    {
    }

        [TestFixture]
        public class EnumTest : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test]
            public void Depth()
            {
                var e = new Enum();
                e.AddRoot(TestTree);
                foreach (var i in e.Depth())
                {
                    log.Info(i);
                }
            }

            [Test]
            public void Breadth()
            {
                var e = new Enum();
                e.AddRoot(TestTree);
                var d = e.Depth();
                var b = e.Depth();
                Assert.IsFalse(d.Except(b).Any());
            }

            LongName TestTree = Sidi.IO.FileUtil.BinFile(".").Long();

            [Test]
            public void Output()
            {
                var e = new Enum();
                e.Output = Enum.OnlyFiles;
                e.AddRoot(TestTree);
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => !x.IsDirectory));
            }

            [Test]
            public void FileType()
            {
                var e = new Enum();
                var fileType = new FileType("exe");
                e.Output = x => Enum.OnlyFiles(x) && fileType.Is(x.Name);
                e.Follow = x => Enum.NoDotNoHidden(x) && x.Name != "test";

                e.AddRoot(TestTree);
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => x.Extension == ".exe"));
            }

            [Test]
            public void Unique()
            {
                var e = new Enum();
                e.Output = Enum.OnlyFiles;
                e.AddRoot(TestTree);
                var maybeIdentical = e.Depth()
                    .GroupBy(x => x.Length)
                    .Where(x => x.Count() >= 2)
                    .ToList();

                foreach (var g in maybeIdentical)
                {
                    log.Info(g.Join(", "));
                }

                var sc = new Sidi.Tool.SizeCount();
                foreach (var g in maybeIdentical)
                {
                    sc.Add(g.First().Length);
                }

                log.Info(sc);
            }
        }
}
