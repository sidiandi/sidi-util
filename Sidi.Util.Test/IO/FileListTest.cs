﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.Extensions;

namespace Sidi.IO
{
    [TestFixture]
    public class FileListTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void Parse()
        {
            var text = @"C:\temp;D:\docs;E:\something";
            var fl = PathList.Parse(text);
            Assert.AreEqual(3, fl.Count);
            Assert.AreEqual(new LPath(@"C:\temp"), fl[0]);
        }

        [Test]
        public void Network()
        {
            var p = new LPath(TestFile(".")).Parts;
            var p1 = new []{ @"\", Environment.MachineName, p[0].Replace(":", "$")}.Concat(p.Skip(1));
            log.Info(p1.Join());
            var nwPath = new LPath(p1);
            log.Info(nwPath);

            var i = nwPath.Info;
            Assert.IsTrue(i.GetFiles().Any());
        }
    }
}
