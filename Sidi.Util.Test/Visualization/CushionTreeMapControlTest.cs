using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Forms;

namespace Sidi.Visualization
{
    [TestFixture]
    public class CushionTreeMapControlTest : TestBase
    {
        [Test, Explicit("interactive")]
        public void Display()
        {
            Display(CushionTreeMapTest.GetDirTree(Sidi.IO.FileUtil.BinFile(".")));
            // Display(CushionTreeMapTest.GetTestTree());
        }

        public void Display(ITree t)
        {
            var tm = new CushionTreeMap(t);
            var c = new CushionTreeMapControl();
            c.TreeMap = tm;
            var f = c.AsForm("test Cushion Tree Map");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
