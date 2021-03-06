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

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
#endregion

//Test Specific Imports
//TODO - Add imports your going to test here
using Sidi.Util;
using Sidi.Collections;
using Sidi.Test;

namespace Sidi.Util
{
    [TestFixture]
    public class LruBackgroundCache : TestBase
    {
        [Test()]
        public void Simple1()
        {
            LruCacheBackground<int, int> cache = new LruCacheBackground<int, int>(10, new LruCache<int, int>.ProvideValue(delegate(int key)
            {
                Console.WriteLine("Load " + key.ToString());
                System.Threading.Thread.Sleep(10);
                return key;
            }));

            for (int i = 0; i < 1000; ++i)
            {
                int value = cache[i];
            }

            System.Threading.Thread.Sleep(1000);
        }
    }

}
