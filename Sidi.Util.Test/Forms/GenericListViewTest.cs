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
using System.Diagnostics;
using Sidi.Extensions;

namespace Sidi.Forms
{
    [TestFixture, RequiresSTA]
    public class GenericListViewTest
    {
        [Test, Ignore("interactive")]
        public void Show()
        {
            var v = new GenericListView();
            v.SetListFormat(Process.GetProcesses().ListFormat().Add("Id", "ProcessName"));
            v.UpdateDisplay();
            var f = v.AsForm("Test");
            System.Windows.Forms.Application.Run(f);
        }

        [Test, Ignore("interactive")]
        public void DragDrop()
        {
            var v = new GenericListView();
            v.Items = Enumerable.Range(1, 100).ToList();
            v.UpdateDisplay();
            v.DragDropOnItem += new EventHandler<GenericListView.DragDropOnItemHandlerEventArgs>((s, e) =>
                {
                    Console.WriteLine(e.Item.ToString());
                });
            var f = v.AsForm("Test");
            System.Windows.Forms.Application.Run(f);
        }
    }
}
