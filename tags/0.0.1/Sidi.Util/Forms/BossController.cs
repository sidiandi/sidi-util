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
