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
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Imap
{
    [TestFixture]
    public class ArgParserTest
    {
        public void GetString()
        {
            Assert.AreEqual("Hello", P("Hello").GetAtom());
            Assert.AreEqual("Hello", P("\"Hello\"").GetQuotedString());
            Assert.AreEqual("Hello", P("Hello").GetAtom());
            var o = P("(Hello 1 2 3 (more in this list))").Get();
            Console.WriteLine(((object[])o).Join());
        }

        ArgParser P(string argString)
        {
            return new ArgParser(argString, null, null);
        }

        public void List()
        {
            Assert.IsNotNull(P("(UID)").GetList());
        }

    }
}
