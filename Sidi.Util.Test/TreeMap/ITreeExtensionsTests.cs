using NUnit.Framework;
using Sidi.Test;
using Sidi.TreeMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sidi.Extensions;
using Sidi.Forms;

namespace Sidi.TreeMap.Tests
{
    [TestFixture()]
    public class ITreeExtensionsTests : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test(), Explicit]
        public void FromLeavesTest()
        {
            var words =
                Regex.Split(
                    TestFile(@"mail\message-1-1456.eml").ReadText().ReadToEnd(),
                    @"\s+");

            log.Info(() => words.Count());

            var tree = ITreeExtensions.FromLeaves(words, _ => _);

            var view = new View { Tree = tree };
            var a = view.GetAdapter(tree);
            a.GetLabel = _ => _.Data.Leaf == null ? _.Data.Name.ToString() : _.Data.Leaf.ToString();

            view.RunFullScreen();
        }
    }
}