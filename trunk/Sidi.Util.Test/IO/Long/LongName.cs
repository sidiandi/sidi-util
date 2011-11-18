using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.Util;
using NUnit.Framework;

namespace Sidi.IO.Long
{
    [TestFixture]
    public static class LongNameExTest
    {
    }

    [TestFixture]
    public class LongNameTest
    {
        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check()
        {
            var ln = new LongName(Enumerable.Range(0, 4000).Select(x => "0000000000").Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
        }

        [Test, ExpectedException(ExpectedException = typeof(System.IO.PathTooLongException))]
        public void Check2()
        {
            var ln = new LongName(new string('0', 256));
        }

        [Test]
        public void Parts()
        {
            var pCount = 40;
            var part = "0000000000";
            var ln = new LongName(Enumerable.Range(0, pCount).Select(x => part).Join(new string(System.IO.Path.DirectorySeparatorChar, 1)));
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

            Assert.IsTrue(validName.IsValidFilename());
            Assert.IsFalse(invalidName.IsValidFilename());

            var f = invalidName.MakeFilename();
            Assert.IsTrue(f.IsValidFilename());
            Assert.AreNotEqual(invalidName, f);
            Assert.AreEqual(System.IO.Path.GetInvalidFileNameChars().Select(c => "_").Join(" "), f);
            Assert.AreEqual(validName, validName.MakeFilename());
        }
    }
}
