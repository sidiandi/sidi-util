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

namespace Sidi.Visualization
{
    [TestFixture]
    public class TreeMapTestData : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Tree GetTestTree()
        {
            var t = GetTestTree(null, 10, 7);
            t.UpdateSize();
            return t;
        }

        static Random rnd = new Random();

        public static Tree GetSimpleTestTree()
        {
            var t = new Tree(null);
            var count = 3;
            foreach (var i in Enumerable.Range(1, count))
            {
                var c = new Tree(t);
                c.Size = i;
                c.Object = i;
            }

            if (t.Children.Any())
            {
                t.Size = 1.0f;
            }
            else
            {
                t.Size = t.ChildSize;
            }
            
            return t;
        }

        static Tree GetTestTree(Tree parent, int childCount, int levels)
        {
            var t = new Tree(parent);
            if (levels > 0)
            {
                foreach (var c in Enumerable.Range(0, rnd.Next(0, childCount)))
                // foreach (var c in Enumerable.Range(0, childCount))
                {
                    var ct = GetTestTree(t, childCount, levels - 1);
                    ct.Object = c;
                }
                t.Size = t.Children.Aggregate(0.0, (s, x) => s + x.Size);
            }
            else
            {
                t.Size = rnd.Next(10, 100);
            }

            return t;
        }
    }
}
