﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using NUnit.Framework;
using Sidi.IO.Long.Extensions;

namespace Sidi.IO.Long
{
    [TestFixture]
    public static class LongNameExTest
    {
    }

    [TestFixture]
    public class LongNameTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check()
        {
            var ln = new Path(Enumerable.Range(0, 4000).Select(x => "0000000000").Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
        }

        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check2()
        {
            var ln = new Path(new string('0', 256));
        }

        [Test]
        public void Parts()
        {
            var pCount = 40;
            var part = "0000000000";
            var ln = new Path(Enumerable.Range(0, pCount).Select(x => part).Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
            var p = ln.Parts;
            Assert.AreEqual(pCount, p.Count());
            Assert.AreEqual(part, p[0]);
        }

        [Test]
        public void SpecialPaths()
        {
            for (var p = System.Environment.SystemDirectory.Long(); p != null; p = p.ParentDirectory)
            {
                Console.WriteLine(p);
                Assert.IsTrue(Directory.Exists(p));
                Console.WriteLine(Directory.GetChilds(p).Join());
            }
        }

        [Test]
        public void UncPaths()
        {
            var tempDir = System.IO.Path.GetTempPath();
            Assert.IsTrue(Directory.Exists(tempDir.Long()));

            var unc = @"\\" + System.Environment.MachineName + @"\" + tempDir.Substring(0, 1) + "$" + tempDir.Substring(2);
            Assert.IsTrue(System.IO.Directory.Exists(unc));

            var longNameUnc = unc.Long();
            Assert.IsTrue(Directory.Exists(longNameUnc));
            Assert.IsTrue(longNameUnc.IsUnc);

            tempDir = longNameUnc.NoPrefix;
            Assert.IsTrue(System.IO.Directory.Exists(tempDir));

            for (var i = longNameUnc; i != null; i = i.ParentDirectory)
            {
                Console.WriteLine(i);
                Console.WriteLine(i.Parts.Join("|"));
                Console.WriteLine(Directory.GetChilds(i).Join());
                Assert.IsTrue(Directory.Exists(i));
            }
        }

        [Test]
        public void MakeFileName()
        {
            var validName = "I am a valid filename";
            var invalidName = System.IO.Path.GetInvalidFileNameChars().Join(" ");

            Assert.IsTrue(Path.IsValidFilename(validName));
            Assert.IsFalse(Path.IsValidFilename(invalidName));

            var f = Path.GetValidFilename(invalidName);
            Assert.IsTrue(Path.IsValidFilename(f));
            Assert.AreNotEqual(invalidName, f);
            Assert.AreEqual(System.IO.Path.GetInvalidFileNameChars().Select(c => "_").Join(" "), f);
            Assert.AreEqual(validName, Path.GetValidFilename(validName));
        }

        [Test]
        public void PathRoot()
        {
            var n = System.IO.Path.GetTempPath().Long();
            Assert.AreEqual(System.IO.Path.GetPathRoot(n.NoPrefix), n.PathRoot.NoPrefix + @"\");
        }

        [Test]
        public void Relative()
        {
            var n = new Path(@"C:\temp\abc.txt");
            var root = new Path(@"C:\Temp");
            Assert.AreEqual(new Path("abc.txt"), n.RelativeTo(root));
            Assert.AreEqual(new Path(), n.RelativeTo(n));
        }

        [Test]
        public void Dots()
        {
            var name = "Wir können auch anders...";
            var p = TestFile("LongName").Long();
            var d = p.CatDir(name);
            log.Info(d);
            d.EnsureNotExists();
            Assert.IsFalse(d.Exists);
            d.EnsureDirectoryExists();
            Assert.IsTrue(Directory.Exists(d));
        }
    }
}
