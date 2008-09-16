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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Sidi.Forms
{
    public class Util
    {
        static public Form AsForm(Control c)
        {
            return AsForm(c, c.ToString());
        }
        
        static public Form AsForm(Control c, string caption)
        {
            Form f = new Form();
            c.Dock = DockStyle.Fill;
            c.Visible = true;
            f.Controls.Add(c);
            f.Text = caption;
            f.Show();
            return f;
        }

        static public void Run(Control c)
        {
            Form f = AsForm(c, string.Empty);
            Application.Run(f);
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
            TextPrompt dlg = new TextPrompt();
            dlg.Prompt = question;
            dlg.Value = defaultValue;
            bool result = dlg.ShowDialog() == DialogResult.OK;
            answer = dlg.Value;
            return result;
        }
    }
}
