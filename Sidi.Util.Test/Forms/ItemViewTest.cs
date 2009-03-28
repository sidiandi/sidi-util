// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region "Mandatory NUnit Imports"
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
#endregion

//Test Specific Imports
using Sidi.Forms;
using System.Windows.Forms;

namespace Sidi.Forms.Test
{

    [TestFixture, Ignore]
    public class ItemViewTest
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

        ItemView<string> itemView;

        [SetUp()]
        public void SetUp()
        {
            //Setup our custom trace listener
            if (!Trace.Listeners.Contains(listener))
            {
                Trace.Listeners.Add(listener);
            }

            itemView = new ItemView<string>();
            List<string> data = new List<string>();
            for (int i = 0; i < 1000; ++i)
            {
                data.Add(String.Format("Item {0} Lorem ipsum dolor sit amet, consectetuer sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua.", i));
            }
            itemView.List = data;
            itemView.ItemLayout = new ItemLayoutRows(32);
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
        public void ItemView()
        {
            Form f = Sidi.Forms.Util.AsForm(itemView);
            Application.Run(f);

            //TODO - Do something

            //Then test its ok
            //TODO - Write your assertion statements
            //Assert.AreEqual(ValueItShouldBe, TestObjectsValue, "Error message to display if not");            
        }

        bool itemActivated = false;

        [Test()]
        public void ItemsActivated()
        {
            itemActivated = false;
            itemView.ItemsActivated += new EventHandler(itemView_ItemsActivated);
            Form f = Sidi.Forms.Util.AsForm(itemView);
            Application.Run(f);
            Assert.IsTrue(itemActivated);
        }

        void itemView_ItemsActivated(object sender, EventArgs e)
        {
            itemActivated = true;
        }

        [Test()]
        public void ItemViewOneItem()
        {
            itemView.List.Clear();
            itemView.List.Add("1");

            Form f = Sidi.Forms.Util.AsForm(itemView);
            Application.Run(f);

            //TODO - Do something

            //Then test its ok
            //TODO - Write your assertion statements
            //Assert.AreEqual(ValueItShouldBe, TestObjectsValue, "Error message to display if not");            
        }
    }

}