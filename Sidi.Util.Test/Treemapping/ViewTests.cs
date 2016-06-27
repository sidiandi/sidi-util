using NUnit.Framework;
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
        [Test]
        public void ViewTest()
        {
            var view = new View
            {
                Tree = TreeNodeTest.CreateTestTree()
            };

            Sidi.Forms.Util.RunFullScreen(view);
        }
    }
}