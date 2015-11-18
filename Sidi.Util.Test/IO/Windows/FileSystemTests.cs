using NUnit.Framework;
using Sidi.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO.Windows
{
    [TestFixture]
    public class FileSystemTests : TestBase
    {
        [Test]
        public void given_an_already_existing_file_EnsureDirectoryExists_must_throw()
        {
            var fs = new Sidi.IO.Windows.FileSystem();
            var p = TestFile("a_file");
            p.EnsureNotExists();
            p.WriteAllText("hello");
            Assert.Throws<System.IO.IOException>(() => fs.EnsureDirectoryExists(p));
        }

        [Test]
        public void given_an_already_existing_file_Open_CreateNew_must_throw()
        {
            var fs = new Sidi.IO.Windows.FileSystem();

            var p = TestFile("a_file");
            p.EnsureNotExists();
            p.WriteAllText("hello");

            Assert.Throws<System.IO.IOException>(() =>
            {
                fs.Open(p, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write, System.IO.FileShare.None);
            });
        }

        [Test]
        public void GetLongPathApiParameter()
        {
            var fs = new Sidi.IO.Windows.FileSystem();
            Assert.AreEqual("\\\\?\\C:\\", fs.GetLongPathApiParameter(new LPath(@"C:\")));
        }
    }
}
