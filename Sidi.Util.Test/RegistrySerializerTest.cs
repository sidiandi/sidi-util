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

namespace Sidi.Util.Test
{
    [TestFixture]
    public class RegistrySerializerTest
    {
        public class Example
        {
            public string AStringVariable;
            public bool ABoolVariable;
            public int AIntVariable;
        }

        [Test]
        public void ReadWrite()
        {
            var x = new Example();
            x.AStringVariable = "Hello";
            x.ABoolVariable = true;
            x.AIntVariable = 123;

            string key = @"HKEY_CURRENT_USER\Software\sidi-util\Test\Example";
            RegistrySerializer.Write(key, x);

            var y = new Example();
            RegistrySerializer.Read(key, y);
            Assert.AreEqual(x.AStringVariable, y.AStringVariable);
            Assert.AreEqual(x.AIntVariable, y.AIntVariable);
            Assert.AreEqual(x.ABoolVariable, y.ABoolVariable);
        }
    }
}
