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
using System.Text;
using System.Diagnostics;
using NUnit.Framework;

//Test Specific Imports
using Sidi.Forms;
using System.Windows.Forms;
using Sidi.Test;

namespace Sidi.Forms
{
    [TestFixture, Ignore("interactive")]
    public class ItemViewTest : TestBase
    {
        ItemView<string> itemView;

        [SetUp()]
        public void SetUp()
        {
            itemView = new ItemView<string>();
            List<string> data = new List<string>();
            for (int i = 0; i < 1000; ++i)
            {
                data.Add(String.Format("Item {0} Lorem ipsum dolor sit amet, consectetuer sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua.", i));
            }
            itemView.List = data;
            itemView.ItemLayout = new ItemLayoutRows(32);
        }

        [Test, Ignore("interactive")]
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

        [Test, Ignore("interactive")]
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

        [Test, Ignore("interactive")]
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
