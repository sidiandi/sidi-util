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

        public static ITree GetTestTree()
        {
            var t = GetTestTree(null, 10, 4);
            return t;
        }

        static Random rnd = new Random();

        public static ITree GetSimpleTestTree()
        {
            var t = new Tree();
            var count = 3;
            t.Children = Enumerable.Range(1, count).Select(i =>
                {
                    var c = new Tree();
                    c.Size = i;
                    c.Data = i;
                    return (ITree) c;
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

        static Tree GetTestTree(ITree parent, int childCount, int levels)
        {
            var t = new Tree();
            t.Parent = parent;
            if (levels > 0)
            {
                t.Children = Enumerable.Range(0, childCount)
                    .Select(c =>
                        {
                            var ct = GetTestTree(t, childCount, levels - 1);
                            ct.Data = c;
                            return (ITree)ct;
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

        public static Tree GetDirTree(string dir)
        {
            return GetDirTreeRec(null, new Sidi.IO.Long.FileSystemInfo(new IO.Long.LongName(dir)));
        }

        static Tree GetDirTreeRec(ITree parent, Sidi.IO.Long.FileSystemInfo i)
        {
            var t = new Tree();
            t.Parent = parent;
            t.Data = i;
            if (i.IsDirectory)
            {
                t.Children = i.GetChilds()
                    .Select(x => (ITree) GetDirTreeRec(t, x))
                    .OrderByDescending(x => x.Size)
                    .ToList();
                t.Size = t.ChildSize;
            }
            else
            {
                t.Size = i.Length;
            }
            return t;
        }
    }
}
