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
