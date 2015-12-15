// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using System.IO;
using NUnit.Framework;
using Sidi.CommandLine;
using Sidi.Extensions;
using Sidi.Test;

namespace Sidi.IO
{
        [TestFixture]
        public class FindTest : TestBase
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            [Test, Explicit]
            public void Depth()
            {
                var e = new Find()
                {
                    Root = _testTree,
                };

                foreach (var i in e.Depth())
                {
                    log.Info(i);
                }
            }

            readonly LPath _testTree = Paths.BinDir;

            [Test]
            public void Output()
            {
                var e = new Find()
                {
                    Output = Find.OnlyFiles,
                    Root = _testTree
                };
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => !x.IsDirectory));
            }

            [Test]
            public void GetChildren()
            {
                var e = new Find()
                {
                    Output = Find.OnlyFiles,
                    Root = _testTree,
                    GetChildren = i => i.GetChildren().OrderByDescending(x => x.LastWriteTimeUtc)
                };
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => !x.IsDirectory));
            }

            [Test]
            public void NotExists()
            {
                var files = Find.AllFiles(new LPath(@"C:\does_not_exist_4352345234234")).ToList();
                Assert.AreEqual(0, files.Count);
            }

            [Test]
            public void Dump()
            {
                var e = new Find()
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
                var fileType = new FileType("dll");
                var e = new Find()
                {
                    Output = x => Find.OnlyFiles(x) && fileType.Is(x.Name),
                    Follow = x => Find.NoDotNoHidden(x),
                    Root = _testTree
                };
                
                var files = e.Depth().ToList();
                Assert.IsTrue(files.Any());
                Assert.IsTrue(files.All(x => x.Extension == ".dll"));
            }

            [Test]
            public void Unique()
            {
                var e = new Find()
                {
                    Output = Find.OnlyFiles,
                    Root = _testTree,
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
