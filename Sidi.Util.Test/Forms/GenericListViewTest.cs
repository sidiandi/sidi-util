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
        [Test, Explicit("interactive")]
        public void Show()
        {
            var v = new GenericListView();
            v.SetListFormat(Process.GetProcesses().ListFormat().Property("Id", "ProcessName"));
            v.UpdateDisplay();
            var f = v.AsForm("Test");
            System.Windows.Forms.Application.Run(f);
        }

        [Test, Explicit("interactive")]
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
