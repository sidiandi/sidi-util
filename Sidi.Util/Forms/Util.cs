// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Windows.Forms;
using System.Drawing;

namespace Sidi.Forms
{
    public static class Util
    {
        static public Form AsForm(Control c)
        {
            return AsForm(c, c.ToString());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static public Form AsForm(this Control c, string caption)
        {
            Form f = new Form();
            c.Dock = DockStyle.Fill;
            c.Visible = true;
            f.Controls.Add(c);
            f.Text = caption;
            f.Show();
            return f;
        }

        static public void Run(this Control c)
        {
            Form f = AsForm(c, string.Empty);
            Application.Run(f);
        }

        static public void Invoke(this Control c, Action a)
        {
            c.Invoke((Delegate)a);
        }

        static public TabPage AddTabPage(TabControl tabControl, Control c, string caption)
        {
            TabPage tabPage = new TabPage();
            tabPage.Text = caption;
            c.Dock = DockStyle.Fill;
            tabPage.Controls.Add(c);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
            return tabPage;
        }

        static public void CreateSplitter(Control parent, Control[] childs)
        {
            parent.Controls.Add(CreateSplitter(childs));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static public Control CreateSplitter(Control[] childs)
        {
            foreach (Control c in childs)
            {
                c.Dock = DockStyle.Fill;
            }

            SplitContainer s = new SplitContainer();
            s.Dock = DockStyle.Fill;
            s.Orientation = Orientation.Vertical;
            s.Panel1.Controls.Add(childs[0]);
            s.Panel2.Controls.Add(childs[1]);
            return s;
        }

        static public bool ParseDirectionKey(Keys key, out Point direction)
        {
            int pageStep = 10;

            switch (key)
            {
                case Keys.A:
                    direction = new Point(-1, 0);
                    break;
                case Keys.D:
                    direction = new Point(+1, 0);
                    break;
                case Keys.W:
                    direction = new Point(0, -1);
                    break;
                case Keys.S:
                    direction = new Point(0, 1);
                    break;

                case Keys.Left:
                    direction = new Point(-1, 0);
                    break;
                case Keys.Right:
                    direction = new Point(+1, 0);
                    break;
                case Keys.Up:
                    direction = new Point(0, -1);
                    break;
                case Keys.Down:
                    direction = new Point(0, 1);
                    break;
                case Keys.PageUp:
                    direction = new Point(0,-pageStep);
                    break;
                case Keys.PageDown:
                    direction = new Point(0, pageStep);
                    break;
                default:
                    direction = new Point(0, 0);
                    return false;
            }

            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                direction = Sidi.Util.GraMath.Mul(direction, pageStep);
            }
            
            return true;
        }

        public static bool Prompt(string question, out string answer)
        {
            return Prompt(question, String.Empty, out answer);
        }

        public static bool Prompt(string question, string defaultValue, out string answer)
        {
            using (var dlg = new TextPrompt())
            {
                dlg.Prompt = question;
                dlg.Value = defaultValue;
                bool result = dlg.ShowDialog() == DialogResult.OK;
                answer = dlg.Value;
                return result;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static void RunFullScreen(this Control c)
        {
            using (var f = c.AsForm(c.GetType().Name))
            {
                f.Visible = false;
                var cancelButton = new Button() { DialogResult = DialogResult.Cancel };
                f.Controls.Add(cancelButton);
                f.CancelButton = cancelButton;
                var fsm = new FullscreenManager(f);
                fsm.Fullscreen = true;
                f.ShowDialog();
            }
        }
    }
}
