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
using Sidi.IO;
using L = Sidi.IO;
using Sidi.Util;
using System.Reflection;
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Appender;
using log4net.Layout;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Sidi.Test
{
    [TestFixture]
    public class TestBaseTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void LoggingTest()
        {
            log.Debug("before");
            using (new LogScope(log.Info, "someTask"))
            {
                log.Info("next step");
                log.Info("task complete");
            }
            log.Debug("after");
        }
    }
}
