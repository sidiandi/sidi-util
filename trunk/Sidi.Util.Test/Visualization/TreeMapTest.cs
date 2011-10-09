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

        public static Tree<int> GetTestTree()
        {
            var t = GetTestTree(null, 10, 5);
            return t;
        }

        static Random rnd = new Random();

        public static Tree<int> GetSimpleTestTree()
        {
            var t = new Tree<int>(null);
            var count = 3;
            t.Children = Enumerable.Range(1, count).Select(i =>
                {
                    var c = new Tree<int>(t);
                    c.Size = i;
                    c.Data = i;
                    return c;
                }).ToList();

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

        static Tree<int> GetTestTree(Tree<int> parent, int childCount, int levels)
        {
            var t = new Tree<int>(parent);
            if (levels > 0)
            {
                t.Children = Enumerable.Range(0, rnd.Next(0, childCount))
                    .Select(c =>
                        {
                            var ct = GetTestTree(t, childCount, levels - 1);
                            ct.Data = c;
                            return ct;
                        })
                    .ToList();

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
