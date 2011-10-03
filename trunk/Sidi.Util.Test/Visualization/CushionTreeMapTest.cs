using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sidi.Visualization
{
    [TestFixture]
    public class CushionTreeMapTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Render()
        {
        }

        public static ITree<int> GetTestTree()
        {
            var t = GetTestTree(null, 10, 4);
            return t;
        }

        static Random rnd = new Random();

        public static ITree<int> GetSimpleTestTree()
        {
            var t = new Tree<int>();
            var count = 3;
            t.Children = Enumerable.Range(1, count).Select(i =>
                {
                    var c = new Tree<int>();
                    c.Size = i;
                    c.Data = i;
                    return (ITree<int>) c;
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

        static Tree<int> GetTestTree(ITree<int> parent, int childCount, int levels)
        {
            var t = new Tree<int>();
            t.Parent = parent;
            if (levels > 0)
            {
                t.Children = Enumerable.Range(0, childCount)
                    .Select(c =>
                        {
                            var ct = GetTestTree(t, childCount, levels - 1);
                            ct.Data = c;
                            return (ITree<int>)ct;
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
