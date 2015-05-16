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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Sidi.CommandLine
{
    public class ActionDialog : Form
    {
        // private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute()
        {
            ActionTag.Execute();
        }

        public void ShowAndExecute()
        {
            for (; ; )
            {
                if (ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Execute();
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        public ActionTag ActionTag { get; set; }
    }

    public class ActionTag
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionTag(Action action)
        {
            Action = action;
        }
        public List<TextBox> ParameterTextBoxes = new List<TextBox>();
        public Action Action { get; private set; }
        public Button Button { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        public void Execute()
        {
            try
            {
                var p = ParameterTextBoxes.Select(x => x.Text).ToList();
                Action.Handle(p, true);
                UserInterface.ClearError(Button);
            }                   
            catch (Exception ex)
            {
                log.Error(String.Format("{0} failed", this.Action), ex);
                var msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                UserInterface.SetError(Button, msg);
            }
        }
    }

}
