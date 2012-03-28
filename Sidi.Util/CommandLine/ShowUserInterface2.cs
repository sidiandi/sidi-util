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
using Sidi.Extensions;

namespace Sidi.CommandLine
{
    public class ShowUserInterface2
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser parser;

        public ShowUserInterface2(Parser parser)
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

        void StackNoXAlign(Control parent, Control child, ref int y)
        {
            child.Top = y;
            child.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            parent.Controls.Add(child);
            y = child.Bottom + margin;
        }

        public Form ToDialog(Action action)
        {
            var at = new ActionTag(action);

            var control = new Form()
            {
                Text = action.Name,
                Width = 640,
                AutoSize = true,
            };

            var layout = new TableLayoutPanel()
            {
                ColumnCount = 1,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                // CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            };
            control.Controls.Add(layout);

            var usage = new Label()
            {
                Text = action.Usage,
                Dock = DockStyle.Top,
                AutoSize = true,
            };

            layout.Controls.Add(usage, 0, 0);

            var paramsPanel = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
            };

            int row = 0;
            foreach (var p in action.MethodInfo.GetParameters())
            {
                var paramLabel = new Label()
                {
                    Text = "{0} [{1}]".F(p.Name, p.ParameterType.GetInfo()),
                    AutoSize = true
                };

                paramsPanel.Controls.Add(paramLabel, 0, row);

                var paramInput = new TextBox()
                {
                    Dock = DockStyle.Fill,
                };
                paramInput.TextChanged += new EventHandler(paramInput_TextChanged);
                paramInput.Tag = p;
                paramsPanel.Controls.Add(paramInput, 1, row);

                at.ParameterTextBoxes.Add(paramInput);

                ++row;
            }

            layout.Controls.Add(paramsPanel,0,1);

            var buttonRow = new FlowLayoutPanel()
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Dock = DockStyle.Fill,
            };
            
            var button = new Button()
            {
                Text = action.Name,
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Tag = at,
            };
            at.Button = button;
            button.Click += new EventHandler((s, e) =>
                {
                    if (at.Execute())
                    {
                        control.Close();
                    }
                });
            buttonRow.Controls.Add(button);

            var cancelButton = new Button()
            {
                Text = "Cancel",
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            buttonRow.Controls.Add(cancelButton);
            cancelButton.Click += new EventHandler((s, e) =>
            {
                control.Close();
            });

            layout.Controls.Add(buttonRow, 0, 2);
            control.AcceptButton = button;
            control.CancelButton = cancelButton;
            return control;
        }

        public Control ToControl3(Action action)
        {
            var at = new ActionTag(action);
            var link = new LinkLabel()
            {
                Text = action.Name + " - " + action.Usage,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Tag = at,
            };
            link.LinkArea = new LinkArea(0, action.Name.Length);
            return link;
        }

        public Control ToControl(Action action)
        {
            var at = new ActionTag(action);
            var link = new Button()
            {
                Text = "&" + action.Name + " - " + action.Usage,
                // AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Tag = at,
            };
            link.Click += new EventHandler((s, e) =>
                {
                    var actionTag = (ActionTag) ((Button)s).Tag;
                    var d = ToDialog(actionTag.Action);
                    d.ShowDialog();
                });
            return link;
        }

        public Control ToControl(SubCommand subCommand)
        {
            var link = new LinkLabel()
            {
                Text = subCommand.Name + " - " + subCommand.Usage,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
            };
            link.LinkArea = new LinkArea(0, subCommand.Name.Length);

            link.Click += new EventHandler((s, e) =>
            {
                var p = new Parser(subCommand.CommandInstance);
                var subCommandDialog = ToDialog(p);
                subCommandDialog.ShowDialog();
            });

            return link;
        }

        Control ToControl(Option option)
        {
            var panel = new TableLayoutPanel()
            {
                ColumnCount = 1,
                AutoSize = true,
                Dock = DockStyle.Fill,
            };

            var label = new Label()
            {
                Text = "&" + option.Syntax + " - " + option.Usage,
                AutoSize = true,
            };
            panel.Controls.Add(label,0,0);

            var paramInput = new TextBox()
            {
                Dock = DockStyle.Fill,
            };
            inputs.Add(paramInput);
            if (option.IsPassword)
            {
                paramInput.PasswordChar = '*';
            }
            panel.Controls.Add(paramInput, 0, 1);

            paramInput.Text = Support.SafeToString(option.GetValue());
            paramInput.Tag = option;
            paramInput.TextChanged += new EventHandler(paramInput_Leave);

            return panel;
        }

        class ActionTag
        {
            public ActionTag(Action action)
            {
                Action = action;
            }
            public List<TextBox> ParameterTextBoxes = new List<TextBox>();
            public Action Action { get; private set; }
            public Button Button { get; set; }

            public bool Execute()
            {
                try
                {
                    var p = ParameterTextBoxes.Select(x => x.Text).ToList();
                    Action.Handle(p, true);
                    ClearError(Button);
                    return true;
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    SetError(Button, msg);
                    return false;
                }
            }
        }

        Form ToDialog(Parser parser)
        {
            Form main = new Form();
            var mouseWheelSupport = new MouseWheelSupport(main);
            main.Width = 640;
            main.Height = 480;
            main.Text = parser.ApplicationName;
            var t = ToControl(parser);
            main.Controls.Add(t);
            return main;
        }

        Control ToControl(Parser parser)
        {
            var t = new TabControl()
            {
                Padding = new Point(8,8),
                Dock = DockStyle.Fill,
            };

            var items = parser.Items
                .Where(item => !((item is SubCommand) || (item.Application is ShowHelp || item.Application is ShowUserInterface || item.Application is ShowWebServer)));
            var categories = items.SelectMany(i => i.Categories).Distinct().ToList();

            foreach (var category in categories)
            {
                var page = new TabPage(String.IsNullOrEmpty(category) ? "General" : category)
                {
                    Padding = new Padding(8),
                };

                var c = new TableLayoutPanel()
                {
                    ColumnCount = 1,
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                };
                c.ColumnStyles.Add(new ColumnStyle()
                {
                    SizeType = SizeType.Percent,
                    Width = 100,
                });
                page.Controls.Add(c);

                var order = new[]
                    {
                        typeof(Action),
                        typeof(SubCommand),
                        typeof(Option)
                    }.Counted().ToDictionary(i => i.Value, i => i.Key);

                foreach (var item in items
                    .Where(x => x.Categories.Contains(category))
                    .OrderBy(x => order[x.GetType()]))
                {
                    AddControl(c, item);
                }

                t.TabPages.Add(page);
            }

            foreach (var subCommand in parser.SubCommands
                .Where(item => !((item.Application is ShowHelp || item.Application is ShowUserInterface || item.Application is ShowWebServer))))
            {
                var page = new TabPage(subCommand.Name)
                {
                    Padding = new Padding(8),
                };

                var c = ToControl(new Parser(subCommand.CommandInstance));
                page.Controls.Add(c);
                t.TabPages.Add(page);
            }

            return t;
        }

        Control ToControl(IParserItem item)
        {
            Control childControl = null;
            if (item is Action)
            {
                childControl = ToControl((Action)item);
            }
            else if (item is Option)
            {
                childControl = ToControl((Option)item);
            }
            else if (item is SubCommand)
            {
                childControl = ToControl((SubCommand)item);
            }

            return childControl;
        }

        void AddControl(Control c, IParserItem item)
        {
            var childControl = ToControl(item);
            if (childControl != null)
            {
                childControl.Width = 200;
                c.Controls.Add(childControl);
            }
        }
        
        [Usage("Show an interactive user interface")]
        [Category(Parser.categoryUserInterface)]
        public void UserInterface()
        {
            var main = ToDialog(parser);
            main.ShowDialog();
        }

        [Usage("Show dialog for the specified command")]
        [Category(Parser.categoryUserInterface)]
        public void ShowDialog(string command)
        {
            var action = (Action) parser.LookupParserItem(command);
            var dialog = ToDialog(action);
            System.Windows.Forms.Application.Run(dialog);
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
            var at = (ActionTag) button.Tag;
            at.Execute();
            Refresh();
        }

        public static void SetError(Control c, string message)
        {
            c.BackColor = errorColor;
            tooltip.SetToolTip(c, message);
        }

        public static void ClearError(Control c)
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

        static ToolTip tooltip = new ToolTip();
        static Color errorColor = Color.Pink;

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
