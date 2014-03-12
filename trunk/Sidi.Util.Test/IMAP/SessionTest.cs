// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of sidi-util.
// 
// sidi-util is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// sidi-util is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with sidi-util. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Util;
using Sidi.IO;
using System.IO;
using Sidi.Extensions;
using Sidi.Test;

namespace Sidi.Imap
{
    [TestFixture]
    public class SessionTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void SequenceSet()
        {
            var e = Session.EnumerateSequenceSet("1,2,3", 1000).ToList();
            Assert.AreEqual(3, e.Count);

            e = Session.EnumerateSequenceSet("1:100", 1000).ToList();
            Assert.AreEqual(100, e.Count);

            e = Session.EnumerateSequenceSet("1,2,3:100", 1000).ToList();
            Assert.AreEqual(100, e.Count);

            e = Session.EnumerateSequenceSet("1:*", 1000).ToList();
            Assert.AreEqual(1000, e.Count);
        }

        [Test, Explicit("unstable")]
        public void RetrieveDataItem()
        {
            var mailbox = new EmlMailbox(this.TestFile(@"mail"));
            var mail = mailbox.Items.First();
            var dataItems = new string[] { "UID", "FLAGS", "RFC822.SIZE", "BODY.PEEK[HEADER]", "INTERNALDATE" };
            foreach (var i in dataItems.Select(x => (string)x))
            {
                log.InfoFormat("{0}={1}", i, ((object[])mail.RetrieveDataItem(i)).Join(", "));
            }
        }

        [Test]
        public void ArgString()
        {
            var m = new MemoryStream();
            var s = @"\";
            using (var w = new StreamWriter(m))
            {
                var a = Session.ArgString(s);
                w.Write(a);
            }
            log.Info(m.GetBuffer().Select(b => String.Format("{0} - {1}", b, (char)b)).Join());
        }
    }
}
