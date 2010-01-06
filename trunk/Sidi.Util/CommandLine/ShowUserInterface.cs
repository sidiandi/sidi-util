using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Sidi.Util;
using Sidi.Forms;

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

        int y = 8;
        int margin = 8;

        void Stack(Control parent, Control child, ref int y)
        {
            child.Top = y;
            child.Left = margin;
            child.Width = parent.ClientRectangle.Width - child.Left - margin;
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
                page.Width = 640;
                page.Height = 480;

                var c = new ScrollableControl();
                c.Dock = DockStyle.Fill;

                int gy = margin;

                foreach (var item in parser.Items.Where(x => x.Categories.Contains(category)))
                {
                    if (item.Application is ShowHelp || item.Application is ShowUserInterface)
                    {
                        continue;
                    }

                    int y = 2*margin;
                    var groupBox = new GroupBox();
                    groupBox.Text = item.Name;
                    Stack(c, groupBox, ref gy);
                    groupBox.Width = c.Width - groupBox.Left - margin;

                    var label = new Label();
                    label.Text = item.Usage;
                    label.AutoSize = true;
                    Stack(groupBox, label, ref y);

                    if (item is Action)
                    {
                        Action action = (Action) item;

                        var at = new ActionTag();
                        at.Action = action;
                        
                        foreach (var p in action.MethodInfo.GetParameters())
                        {
                            var paramLabel = new Label();
                            paramLabel.Text = "{0} [{1}]".F(p.Name, p.ParameterType.GetInfo());
                            paramLabel.AutoSize = true;
                            int y1 = y;
                            Stack(groupBox, paramLabel, ref y1);

                            var paramInput = new TextBox();
                            Stack(groupBox, paramInput, ref y);
                            paramInput.Left = paramLabel.Right + margin;
                            at.ParameterTextBoxes.Add(paramInput);
                        }

                        var button = new Button();
                        button.Text = item.Name;
                        button.Top = y;
                        button.Tag = at; 
                        button.Click += new EventHandler(button_Click);
                        Stack(groupBox, button, ref y);
                        button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        button.AutoSize = true;
                    }

                    if (item is Option)
                    {
                        var option = item as Option;
                        
                        var paramLabel = new Label();
                        paramLabel.Text = option.Syntax;
                        paramLabel.AutoSize = true;
                        int y1 = y;
                        Stack(groupBox, paramLabel, ref y1);

                        var paramInput = new TextBox();
                        Stack(groupBox, paramInput, ref y);
                        paramInput.Left = paramLabel.Right + margin;

                        paramInput.Text = Support.SafeToString(option.GetValue());
                        paramInput.Tag = option;
                        paramInput.TextChanged += new EventHandler(paramInput_Leave);
                    }

                    groupBox.Height = y + margin;
                    gy = groupBox.Bottom + margin;
                }

                c.AutoScroll = true; 
                
                page.Controls.Add(c);
                t.TabPages.Add(page);
            }
            
            main.Controls.Add(t);

            main.ShowDialog();
        }

        void button_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var at = (ActionTag)button.Tag;
            try
            {
                var p = at.ParameterTextBoxes.Select(x => x.Text.Quote()).ToList();
                at.Action.Handle(p);
                tooltip.SetToolTip(button, null);
                button.BackColor = Color.FromKnownColor(KnownColor.ButtonFace);
            }
            catch (Exception ex)
            {
                tooltip.SetToolTip(button, ex.InnerException.Message);
                button.BackColor = errorColor;
            }
        }

        ToolTip tooltip = new ToolTip();
        Color errorColor = Color.Pink;

        void paramInput_Leave(object sender, EventArgs e)
        {
            var textBox = (TextBox) sender;
            var option = (Option)textBox.Tag;
            try
            {
                option.Handle(new string[] { textBox.Text }.ToList());
                textBox.BackColor = Color.FromKnownColor(KnownColor.Window);
            }
            catch (Exception ex)
            {
                tooltip.SetToolTip(textBox, ex.Message);
                textBox.BackColor = errorColor;
            }
        }
    }

}
