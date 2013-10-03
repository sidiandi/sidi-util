using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Extensions;

namespace Sidi.Util
{
    [TestFixture]
    public class ShellTest
    {
        [Test, Explicit("interacts with Desktop")]
        public void ActiveWindow()
        {
            var s = new Shell();
            var w = s.GetForegroundWindow();
            Assert.NotNull(w);
            Console.WriteLine();
        }

        [Test, Explicit("interacts with Desktop")]
        public void SelectedFiles()
        {
            var s = new Shell();
            Console.WriteLine(s.SelectedFiles.Join());
        }

        [Test, Explicit("interacts with Desktop")]
        public void GetOpen()
        {
            var s = new Shell();
            Console.WriteLine(s.GetOpenDirectory());
        }

    }
}
