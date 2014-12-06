// Copyright (c) 2013, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using Sidi.Test;
using Sidi.Extensions;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Sidi.Extensions
{
    [TestFixture]
    public class TypeExtensionsTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void AnonymousType()
        {
            var d = new { A = 1, B = 2, C = 3 };
            Assert.IsTrue(d.GetType().IsAnonymousType());
            Assert.IsFalse(this.GetType().IsAnonymousType());
            Assert.IsFalse(typeof(string).IsAnonymousType());
        }
    }
}
