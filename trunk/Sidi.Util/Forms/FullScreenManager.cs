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
    public class FullscreenManager
    {
        public FullscreenManager(Form form)
        {
            m_form = form;
        }

        public void Toggle()
        {
            Fullscreen = !Fullscreen;
        }

        public bool Fullscreen
        {
            set
            {
                if (m_fullscreen == value)
                {
                    return;
                }

                m_fullscreen = value;

                if (m_fullscreen)
                {
                    m_bounds = m_form.Bounds;
                    m_formBorderStyle = m_form.FormBorderStyle;
                    m_windowState = m_form.WindowState;

                    if (m_form.MainMenuStrip != null)
                    {
                        m_form.MainMenuStrip.Visible = false;
                    }

                    m_form.FormBorderStyle = FormBorderStyle.None;
                    m_form.Bounds = Screen.PrimaryScreen.Bounds;
                    m_form.WindowState = FormWindowState.Normal;
                }
                else
                {
                    m_form.TopMost = false;
                    m_form.WindowState = m_windowState;
                    m_form.Bounds = m_bounds;
                    m_form.FormBorderStyle = m_formBorderStyle;
                    if (m_form.MainMenuStrip != null)
                    {
                        m_form.MainMenuStrip.Visible = true;
                    }
                }
            }
            get
            {
                return m_fullscreen;
            }

        }

        Form m_form = null;
        bool m_fullscreen = false;
        Rectangle m_bounds;
        FormBorderStyle m_formBorderStyle;
        FormWindowState m_windowState;
    }
}
