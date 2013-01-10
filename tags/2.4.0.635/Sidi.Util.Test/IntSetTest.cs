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

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
#endregion

//Test Specific Imports
//TODO - Add imports your going to test here
using Sidi.Collections;
using Sidi.Util;

namespace Sidi.Util
{

    [TestFixture]
    public class IntSetTest
    {
        #region "Custom Trace Listener"
        MyListener listener = new MyListener();

        internal class MyListener : TraceListener
        {
            public override void Write(string message)
            {
                Console.Write(message);
            }


            public override void WriteLine(string message)
            {
                Console.WriteLine(message);
            }
        }
        #endregion

        IntSet set;

        [SetUp()]
        public void SetUp()
        {
            //Setup our custom trace listener
            if (!Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Add(listener);
            }

            //TODO - Setup your test objects here
            set = new IntSet();
        }

        [TearDown()]
        public void TearDown()
        {
            //Remove our custom trace listener
            if (Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Remove(listener);
            }

            //TODO - Tidy up your test objects here
        }

        [Test()]
        public void IsEmpty()
        {
            Assert.IsTrue(set.IsEmpty);
        }

        [Test()]
        public void IsEmpty2()
        {
            set.Add(new Interval(0, 10));
            Assert.IsFalse(set.IsEmpty);
        }

        [Test()]
        public void Contains()
        {
            set.Add(new Interval(0, 10));
            Assert.IsTrue(set.Contains(5));
            Assert.IsFalse(set.Contains(10));
        }

        [Test()]
        public void Contains2()
        {
            set.Add(new Interval(0, 10));
            set.Add(new Interval(5, 15));
            Assert.IsTrue(set.Contains(5));
            Assert.IsTrue(set.Contains(10));
            Assert.IsFalse(set.Contains(15));
        }

        [Test()]
        public void Contains3()
        {
            set.Add(new Interval(0, 10));
            set.Add(new Interval(20, 30));
            set.Add(new Interval(10, 20));
            Assert.IsTrue(set.Contains(5));
            Assert.IsTrue(set.Contains(10));
            Assert.IsFalse(set.Contains(30));
        }

        [Test()]
        public void Intersect()
        {
            set.Add(new Interval(0, 3));
            set.Intersect(new IntSet(new Interval(1, 2)));
            Assert.IsFalse(set.Contains(0));
            Assert.IsTrue(set.Contains(1));
            Assert.IsFalse(set.Contains(2));
            Assert.IsFalse(set.Contains(3));
        }
    }

}
