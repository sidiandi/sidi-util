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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Sidi.IO;

namespace Sidi.Build.Test
{
    /// <summary>
    /// Test for SourceIndex
    /// </summary>
    [TestFixture]
    public class SourceIndexTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        SourceIndex instance = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SourceIndexTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        /// <summary>
        /// Test setup. Creates a fresh SourceIndex instance.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            instance = new SourceIndex();
        }

        /// <summary>
        /// Add information to a PDB under which URLs the source files can be found.
        /// </summary>
        [Test, Explicit("requires source path")]
        public void Test()
        {
            instance.Directory = @"E:\work\sidi-util";
            instance.Url = "http://sidi-util.googlecode.com/svn/trunk";

            TaskItem t = new TaskItem(TestFile("Sidi.Util.dll"));
            instance.Modules = new ITaskItem[] { t };

            instance.Execute();

            Srctool s = new Srctool();
            s.Extract(new Sidi.IO.LPath(t.ItemSpec).ChangeExtension("pdb"));
        }
    }
}
