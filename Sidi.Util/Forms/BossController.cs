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
using System.Windows.Forms;

namespace Sidi.Forms
{
    public class BossController : Controller
    {
        public BossController(Control c)
        {
            this.KeyDown += new KeyEventHandler(BossController_KeyDown);
            this.MouseDown += new MouseEventHandler(BossController_MouseDown);
            if (c is Form)
            {
                Ignore(((Form)c).MainMenuStrip);
            }
        }

        void BossController_MouseDown(object sender, MouseEventArgs e)
        {
            if (Control.MouseButtons == MouseButtons.Middle ||
                (Control.MouseButtons == (MouseButtons.Left | MouseButtons.Right)))
            {
                HideApplication();
            }
        }

        void BossController_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad0:
                    HideApplication();
                    break;
                default:
                    break;
            }
        }

        public static void HideApplication()
        {
            List<Form> forms = new List<Form>();
            foreach (Form f in Application.OpenForms)
            {
                if (f.TopLevel)
                {
                    forms.Add(f);
                }
            }
            foreach (Form f in forms)
            {
                f.Visible = false;
            }

            foreach (Form f in forms)
            {
                f.WindowState = FormWindowState.Minimized;
                f.Visible = true;
                f.Text = "";
            }
        }
    }
}
