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
using System.Threading;

namespace Sidi.Util.Test
{
    [TestFixture]
    public class AsyncCalculationTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AsyncCalculationTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        IList<string> TestQuery(string search)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 100; ++i)
            {
                list.Add(i.ToString());
            }
            Thread.Sleep(1000);
            return list;
        }

        [Test]
        public void Test()
        {
            var a = new AsyncCalculation<string, IList<string>>(TestQuery);
            Assert.IsNull(a.Result);
            a.Query = "a";
            a.Wait();
            Assert.IsNotNull(a.Result);
        }

        [Test]
        public void Test2()
        {
            var a = new AsyncCalculation<string, IList<string>>(TestQuery);
            Assert.IsNull(a.Result);
            for (int i = 0; i < 10; ++i)
            {
                a.Query = i.ToString();
                Thread.Sleep(20);
            }

            a.Wait();
            Assert.IsNotNull(a.Result);
            Assert.AreEqual(100, a.Result.Count);
        }
    }
}
