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
using System.Reflection;
using Sidi.Util;

namespace Sidi.Build.Test
{
    [TestFixture]
    public class CreateUpdateInfoTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CreateUpdateInfo instance = null;

        [SetUp]
        public void SetUp()
        {
            instance = new CreateUpdateInfo();
        }

        [Test]
        public void Writer()
        {
            string outFile = FileUtil.BinFile(@"Test\generated.xml");
            {
                CreateUpdateInfo cui = new CreateUpdateInfo();
                cui.OutputFile = outFile;
                cui.AssemblyFile = Assembly.GetExecutingAssembly().Location;
                cui.Execute();
            }

            UpdateCheck c = new UpdateCheck(Assembly.GetExecutingAssembly(), new Uri(outFile));
            c.Check();
            Assert.IsTrue(!c.IsUpdateRequired);
        }

    }
}
