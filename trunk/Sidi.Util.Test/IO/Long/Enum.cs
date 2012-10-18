using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.IO;
using NUnit.Framework;
using Sidi.CommandLine;
using Sidi.IO.Long.Extensions;
using Sidi.Extensions;

namespace Sidi.IO.Long
{
        [TestFixture]
        public class EnumTest : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test, Explicit]
            public void Depth()
            {
                var e = new FileEnum()
                {
                    Root = TestTree,
                };

                foreach (var i in e.Depth())
                {
                    log.Info(i);
                }
            }

            [Test, Explicit]
            public void Breadth()
            {
                var e = new FileEnum()
                {
                    Root = TestTree
                };

                var d = e.Depth();
                var b = e.Depth();
                Assert.IsFalse(d.Except(b).Any());
            }

            Path TestTree = Paths.BinDir.CatDir(".");

            [Test]
            public void Output()
            {
                var e = new FileEnum()
                {
                    Output = FileEnum.OnlyFiles,
                    Root = TestTree
                };
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => !x.IsDirectory));
            }

            [Test]
            public void NotExists()
            {
                var files = FileEnum.AllFiles(new Path(@"C:\does_not_exist_4352345234234")).ToList();
                Assert.AreEqual(0, files.Count);
            }

            [Test]
            public void Dump()
            {
                var e = new FileEnum()
                {
                    Root = TestFile("."),
                };

                foreach (var i in e.Depth())
                {
                    log.Info(i.Name);
                }
            }

            [Test]
            public void FileType()
            {
                var fileType = new FileType("exe");
                var e = new FileEnum()
                {
                    Output = x => FileEnum.OnlyFiles(x) && fileType.Is(x.Name),
                    Follow = x => FileEnum.NoDotNoHidden(x) && x.Name != "test",
                    Root = TestTree
                };
                
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => x.Extension == ".exe"));
            }

            [Test]
            public void Unique()
            {
                var e = new FileEnum()
                {
                    Output = FileEnum.OnlyFiles,
                    Root = TestTree,
                };

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
