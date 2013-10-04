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
using NUnit.Framework;
using Sidi.IO;
using L = Sidi.IO;
using System.IO;
using Sidi.Util;
using Sidi.Test;

namespace Sidi.Build.Test
{
    [TestFixture]
    public class PdbstrTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Pdbstr instance = null;
        Sidi.IO.LPath pdbFile;

        public PdbstrTest()
        {
            pdbFile = L.Paths.BinDir.CatDir("Sidi.Util.pdb");
        }

        [SetUp]
        public void SetUp()
        {
            instance = new Pdbstr();
        }


        [Test]
        public void Write()
        {
            var pdbFileCopy = pdbFile.ChangeExtension(".modified.pdb");
            Sidi.IO.LFile.Copy(pdbFile, pdbFileCopy, true);

            string content = "Hello, Test";
            string streamName = "srcsrv";

            instance.Write(pdbFileCopy, streamName, content);

            Assert.IsTrue(instance.Read(pdbFileCopy, streamName).StartsWith(content));
        }
    }
}
