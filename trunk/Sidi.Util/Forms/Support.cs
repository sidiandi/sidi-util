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
    public class Support
    {
        static public Form AsForm(Control c, string caption)
        {
            Form f = new Form();
            c.Dock = DockStyle.Fill;
            c.Visible = true;
            f.Controls.Add(c);
            f.Text = caption;
            return f;
        }

        public static string SafeToString(object x)
        {
            if (x == null)
            {
                return "";
            }
            else
            {
                return x.ToString();
            }
        }
    }
}
