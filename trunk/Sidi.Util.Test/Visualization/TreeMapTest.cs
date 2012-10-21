using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.Visualization
{
    [TestFixture]
    public class TreeMapTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Render()
        {
        }

        public static Tree GetTestTree()
        {
            var t = GetTestTree(null, 10, 5);
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
                {
                    var ct = GetTestTree(t, childCount, levels - 1);
                    ct.Object = c;
                }
                t.Size = t.Children.Aggregate(0.0f, (s, x) => s + x.Size);
            }
            else
            {
                t.Size = rnd.Next(10, 100);
            }

            return t;
        }
    }
}
