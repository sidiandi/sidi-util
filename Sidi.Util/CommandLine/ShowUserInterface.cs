using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.Util;
using Sidi.Forms;
using System.Reflection;
using System.ComponentModel;

namespace Sidi.CommandLine
{
    public class ShowUserInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser parser;

        public ShowUserInterface(Parser parser)
        {
            this.parser = parser;
        }

        int margin = 4;

        void Stack(Control parent, Control child, ref int y)
        {
            child.Top = y;
            child.Left = margin;
            child.Width = parent.Width - child.Left - margin;
            child.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            parent.Controls.Add(child);
            y = child.Bottom + margin;
        }

        class ActionTag
        {
            public List<TextBox> ParameterTextBoxes = new List<TextBox>();
            public Action Action;
        }

        [Usage("Show an interactive user interface")]
        [Category(Parser.categoryUserInterface)]
        public void UserInterface()
        {
            Form main = new Form();
            main.Width = 640;
            main.Height = 480;
            main.Text = parser.ApplicationName;

            var t = new TabControl();
            t.Dock = DockStyle.Fill;

            foreach (var category in parser.Categories)
            {
                var page = new TabPage(String.IsNullOrEmpty(category) ? "General" : category);

                var c = new ScrollableControl();
                c.Dock = DockStyle.Fill;

                int y = 0;

                foreach (var item in parser.Items.Where(x => x.Categories.Contains(category)))
                {
                    y += 3 * margin;
                    if (item.Application is ShowHelp || item.Application is ShowUserInterface)
                    {
                        continue;
                    }

                    if (item is Action)
                    {
                        Action action = (Action)item;

                        var at = new ActionTag();
                        at.Action = action;

                        foreach (var p in action.MethodInfo.GetParameters())
                        {
                            var paramLabel = new Label();
                            paramLabel.Text = "{0} [{1}]".F(p.Name, p.ParameterType.GetInfo());
                            paramLabel.AutoSize = true;
                            int y1 = y;
                            Stack(c, paramLabel, ref y1);

                            var paramInput = new TextBox();
                            paramInput.TextChanged += new EventHandler(paramInput_TextChanged);
                            paramInput.Tag = p;
                            Stack(c, paramInput, ref y);
                            paramInput.Left = paramLabel.Right + margin;
                            at.ParameterTextBoxes.Add(paramInput);
                        }

                        var button = new Button();
                        button.Text = item.Name;
                        button.Top = y;
                        button.Tag = at;
                        button.Click += new EventHandler(button_Click);
                        {
                            int y2 = y;
                            Stack(c, button, ref y);
                            y = y2;
                        }
                        button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        button.AutoSize = true;

                        var label = new Label();
                        label.Text = item.Usage;
                        label.AutoSize = true;
                        Stack(c, label, ref y);
                        label.Left = button.Right + margin;
                    }

                    if (item is Option)
                    {
                        var label = new Label();
                        label.Text = item.Usage;
                        label.AutoSize = true;
                        Stack(c, label, ref y);

                        var option = item as Option;
                        
                        var paramLabel = new Label();
                        paramLabel.Text = option.Syntax;
                        paramLabel.AutoSize = true;
                        int y1 = y;
                        Stack(c, paramLabel, ref y1);

                        var paramInput = new TextBox();
                        inputs.Add(paramInput);
                        if (option.IsPassword)
                        {
                            paramInput.PasswordChar = '*';
                        }
                        Stack(c, paramInput, ref y);
                        paramInput.Left = paramLabel.Right + margin;

                        paramInput.Text = Support.SafeToString(option.GetValue());
                        paramInput.Tag = option;
                        paramInput.TextChanged += new EventHandler(paramInput_Leave);
                    }
                }

                c.AutoScroll = true; 
                
                page.Controls.Add(c);
                t.TabPages.Add(page);
            }
            
            main.Controls.Add(t);

            main.ShowDialog();
        }

        List<TextBox> inputs = new List<TextBox>();

        void Refresh()
        {
            foreach (var i in inputs)
            {
                if (i.Tag is Option)
                {
                    var option = ((Option)i.Tag);
                    i.Text = Support.SafeToString(option.GetValue());
                }
            }
        }

        void paramInput_TextChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            var p = (ParameterInfo)textBox.Tag;
            try
            {
                Parser.ParseValue(textBox.Text, p.ParameterType);
                ClearError(textBox);
            }
            catch (Exception ex)
            {
                SetError(textBox, ex.Message);
            }
        }

        void button_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var at = (ActionTag)button.Tag;
            try
            {
                var p = at.ParameterTextBoxes.Select(x => x.Text).ToList();
                at.Action.Handle(p, true);
                ClearError(button);
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                SetError(button, msg);
            }
            Refresh();
        }

        void SetError(Control c, string message)
        {
            c.BackColor = errorColor;
            tooltip.SetToolTip(c, message);
        }

        void ClearError(Control c)
        {
            if (c is TextBox)
            {
                c.BackColor = Color.FromKnownColor(KnownColor.Window); 
            }

            if (c is Button)
            {
                c.BackColor = Color.FromKnownColor(KnownColor.ButtonFace);
            }
            tooltip.SetToolTip(c, null);
        }

        ToolTip tooltip = new ToolTip();
        Color errorColor = Color.Pink;

        void paramInput_Leave(object sender, EventArgs e)
        {
            var textBox = (TextBox) sender;
            var option = (Option)textBox.Tag;
            try
            {
                option.Handle(new string[] { textBox.Text }.ToList(), true);
                ClearError(textBox);
            }
            catch (Exception ex)
            {
                SetError(textBox, ex.Message);
            }
        }
    }

}
