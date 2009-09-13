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

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
using Sidi.IO;
using System.IO;
#endregion

//Test Specific Imports
//TODO - Add imports your going to test here

namespace Sidi.Util
{

    [TestFixture]
    public class FileUtilTest : TestBase
    {
        #region "Custom Trace Listener"
        MyListener listener = new MyListener();

        internal class MyListener : TraceListener
        {
            public override void Write(string message)
            {
                Console.Write(message);
            }


            public override void WriteLine(string message)
            {
                Console.WriteLine(message);
            }
        }
        #endregion


        [SetUp()]
        public void SetUp()
        {
            //Setup our custom trace listener
            if (!Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Add(listener);
            }

            //TODO - Setup your test objects here
        }

        [TearDown()]
        public void TearDown()
        {
            //Remove our custom trace listener
            if (Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Remove(listener);
            }

            //TODO - Tidy up your test objects here
        }

        [Test()]
        public void TestGetRelativePath()
        {
            Assert.AreEqual(FileUtil.GetRelativePath(@"a\b\c", @"a"), @"b\c");
            Assert.AreEqual(FileUtil.GetRelativePath(@"a", @"a\b\c"), @"..\..");
            Assert.AreEqual(FileUtil.GetRelativePath(@".\a", @"."), @"a");
            Assert.AreEqual(FileUtil.GetRelativePath(@".", @".\a"), @"..");

            Assert.AreEqual(@"a\b\c".GetRelativePath(@"a"), @"b\c");
            Assert.AreEqual(@"a".GetRelativePath(@"a\b\c"), @"..\..");
            Assert.AreEqual(@".\a".GetRelativePath(@"."), @"a");
            Assert.AreEqual(@".".GetRelativePath(@".\a"), @"..");
        }

        [Test]
        public void Sibling()
        {
            Assert.IsTrue(@"a\b".Sibling("c").EndsWith(@"a\c"));
        }

        [Test]
        public void GetFileSystemInfo()
        {
            string p = FileUtil.BinFile("test");
            FileSystemInfo fi = p.GetFileSystemInfo();
            Assert.AreEqual(fi.FullName, p);
        }

        [Test]
        public void CatDir()
        {
            Assert.AreEqual(FileUtil.CatDir("a", "b", "c"), @"a\b\c");
        }
    }

}
