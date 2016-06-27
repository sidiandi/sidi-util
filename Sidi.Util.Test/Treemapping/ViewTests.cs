using NUnit.Framework;
using Sidi.IO;
using Sidi.Treemapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping.Tests
{
    [TestFixture, Apartment(System.Threading.ApartmentState.STA)]
    public class ViewTests
    {
        internal static ITree CreateTestTree()
        {
            var random = new Random(0);
            var root = new Tree<TreeLayout>();
            AddRandomChilds(random, root, 3);
            return root;
        }

        static void AddRandomChilds(Random random, Tree<TreeLayout> parent, int level)
        {
            var iEnd = random.Next(5, 10);
            for (int i = 0; i < iEnd; ++i)
            {
                new Tree<TreeLayout>
                {
                    Parent = parent,
                    Data = new TreeLayout
                    {
                    }
                };
            }

            if (level > 0)
            {
                foreach (var c in parent.Children)
                {
                    AddRandomChilds(random, c, level - 1);
                }
            }
            else
            {
                foreach (var c in parent.Children)
                {
                    c.Data.Size = random.Next(1, 10);
                }
            }
        }

        [Test]
        public void ViewTest()
        {
            var view = new View
            {
                Tree = CreateTestTree()
            };

            Sidi.Forms.Util.RunFullScreen(view);
        }
    }
}