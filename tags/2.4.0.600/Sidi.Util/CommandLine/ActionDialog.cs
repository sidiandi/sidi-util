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

        public ActionTag ActionTag;
    }

    public class ActionTag
    {
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
                ShowUserInterface.ClearError(Button);
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                ShowUserInterface.SetError(Button, msg);
                throw;
            }
        }
    }

}
