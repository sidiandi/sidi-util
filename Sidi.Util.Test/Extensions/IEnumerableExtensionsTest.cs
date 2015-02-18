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
using Sidi.Extensions;
using System.Threading;

namespace Sidi.Extensions
{
    [TestFixture]
    public class IEnumerableExtensionsTest : Sidi.Test.TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Best()
        {
            var r = Enumerable.Range(0, 25).Select(x => x.ToString());
            Assert.AreEqual("24", r.Best(x => Int32.Parse(x)));
        }

        [Test]
        public void SafeSelect()
        {
            var x = Enumerable.Range(0, 10).ToList();
            var y = x.SafeSelect(i => 100 / i);
            Assert.AreEqual(x.Count() - 1, y.Count());
        }

        [Test]
        public void LogProgress()
        {
            var e = Enumerable.Range(0, 5);

            var result = e
                .LogProgress(log.Info)
                .Select(i =>
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.5));
                        return i.ToString();
                    })
                .ToList();
        }

        [Test]
        public void LogProgressForAList()
        {
            var e = Enumerable.Range(0, 5);

            var result = e
                .ToList()
                .LogProgress(log.Info)
                .Select(i =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                    return i.ToString();
                })
                .ToList();
        }
    }
}
