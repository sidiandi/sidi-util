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
using System.IO;
using Sidi.Util;

namespace Sidi.Test.Util
{
    [TestFixture]
    public class StringExTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        [Test]
        public void Printable()
        {
            foreach (var i in Directory.GetFiles(@"I:\1_MR\1_HQMR\Teams\ExamFramework\arfa\cache\N4_VD11A_LATEST_20090905\Debug\examdb\MriProduct\examdb\Root\Test_Region\Test_Exam\Test_I18n"))
            {
                log.Info(i);
            }
        }

        [Test]
        public void GetSection()
        {
            StringWriter w = new StringWriter();
            w.WriteLine("[SectionA]");
            w.WriteLine("hello");
            w.WriteLine("[SectionB]");
            w.WriteLine("world");

            string t = w.ToString();

            Assert.AreEqual("hello", t.GetSection("SectionA"));
            Assert.AreEqual("world", t.GetSection("SectionB"));
        }

        [Test, Explicit("interactive")]
        public void EditInteractive()
        {
            log.Info("Hello, world".EditInteractive());
        }
    }
}