﻿using Sidi.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sidi.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Sidi.CommandLine
{
    public class UserInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser parser;

        public UserInterface(Parser parser)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ActionDialog ToDialog(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var control = new ActionDialog()
            {
                Text = action.Name,
                Width = 640,
                AutoSize = true,
                ActionTag = new ActionTag(action)
            };

            var at = control.ActionTag;

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

                var textBoxParameter = new TextBox()
                {
                    Dock = DockStyle.Fill,
                };
                textBoxParameter.Tag = p;
                textBoxParameter.TextChanged += new EventHandler(textBoxParameter_TextChanged);
                paramsPanel.Controls.Add(textBoxParameter, 1, row);

                at.ParameterTextBoxes.Add(textBoxParameter);

                ++row;
            }

            layout.Controls.Add(paramsPanel,0,1);

            var buttonRow = new FlowLayoutPanel()
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Dock = DockStyle.Fill,
            };
            
            var acceptButton = new Button()
            {
                Text = "OK",
                DialogResult = System.Windows.Forms.DialogResult.OK,
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Tag = at,
            };
            at.Button = acceptButton;
            buttonRow.Controls.Add(acceptButton);

            var cancelButton = new Button()
            {
                Text = "Cancel",
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                DialogResult = DialogResult.Cancel
            };
            buttonRow.Controls.Add(cancelButton);
            cancelButton.Click += new EventHandler((s, e) =>
            {
                control.Close();
            });

            layout.Controls.Add(buttonRow, 0, 2);
            control.AcceptButton = acceptButton;
            control.CancelButton = cancelButton;
            return control;
        }

        void textBoxParameter_TextChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            var parameter = (ParameterInfo)textBox.Tag;
            try
            {
                var args = new List<String>() { textBox.Text };
                this.parser.ParseValue(args, parameter.ParameterType);
                ClearError(textBox);
            }
            catch (Exception ex)
            {
                SetError(textBox, ex.ToString());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Control ToControl(Action action)
        {
            var at = new ActionTag(action);

            var control = new Panel()
            {
                Text = action.Name,
                Dock = DockStyle.Fill,
                AutoSize = true,
            };

            var layout = new TableLayoutPanel()
            {
                ColumnCount = 1,
                AutoSize = true,
                Dock = DockStyle.Fill,
            };
            control.Controls.Add(layout);
            int layoutRow = 0;

            var usage = new Label()
            {
                Text = action.Usage,
                Dock = DockStyle.Top,
                AutoSize = true,
            };

            layout.Controls.Add(usage, 0, layoutRow++);

            var paramsPanel = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
            };

            var button = new Button()
            {
                Text = action.Name,
                AutoSize = true,
                Tag = at,
            };
            at.Button = button;
            button.Click += new EventHandler(button_Click);

            int row = 0;
            foreach (var p in action.MethodInfo.GetParameters())
            {
                var parameter = p;
                var paramLabel = new Label()
                {
                    Text = "{0} [{1}]".F(p.Name, p.ParameterType.GetInfo()),
                    AutoSize = true
                };

                paramsPanel.Controls.Add(paramLabel, 0, row);

                var textBoxParameter = new TextBox()
                {
                    Dock = DockStyle.Fill,
                };
                textBoxParameter.TextChanged +=new EventHandler(textBoxParameter_TextChanged);
                textBoxParameter.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.Enter)
                        {
                            button.PerformClick();        
                        }
                    };
                textBoxParameter.Tag = p;
                paramsPanel.Controls.Add(textBoxParameter, 1, row);
                refreshActions.Add(() =>
                    {
                        if (parameterCache.ContainsKey(parameter.Name))
                        {
                            textBoxParameter.Text = parameterCache[parameter.Name];
                        }
                    });

                at.ParameterTextBoxes.Add(textBoxParameter);

                ++row;
            }

            layout.Controls.Add(paramsPanel, 0, layoutRow++);

            layout.Controls.Add(button, 0, layoutRow++);
            return control;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Control ToControl(Option option)
        {
            var control = new Panel()
            {
                Text = option.Name,
                Dock = DockStyle.Fill,
                AutoSize = true,
            };

            var layout = new TableLayoutPanel()
            {
                ColumnCount = 1,
                AutoSize = true,
                Dock = DockStyle.Fill,
            };
            control.Controls.Add(layout);

            var usage = new Label()
            {
                Text = option.Usage,
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
            var paramLabel = new Label()
            {
                Text = "{0} [{1}]".F(option.Name, option.Type.GetInfo()),
                AutoSize = true
            };

            paramsPanel.Controls.Add(paramLabel, 0, row);

            var textBoxOption = new TextBox()
            {
                Dock = DockStyle.Fill,
            };

            if (option.IsPassword)
            {
                textBoxOption.PasswordChar = '*';
            }
            textBoxOption.Tag = option;

            System.Action refreshAction = () =>
                {
                    textBoxOption.Text = option.GetValue().SafeToString();
                };
            refreshAction();
            refreshActions.Add(refreshAction);

            textBoxOption.TextChanged +=new EventHandler(textBoxOption_TextChanged);
            textBoxOption.Leave += new EventHandler(textBoxOption_Leave);
            paramsPanel.Controls.Add(textBoxOption, 1, row);

            layout.Controls.Add(paramsPanel, 0, 1);

            return control;
        }

        void textBoxOption_TextChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Control ToControl(SubCommand subcommand)
        {
            var control = new GroupBox()
            {
                Text = subcommand.Name,
                Dock = DockStyle.Fill,
                AutoSize = true,
            };

            var layout = new TableLayoutPanel()
            {
                ColumnCount = 1,
                AutoSize = true,
                Dock = DockStyle.Fill,
            };
            control.Controls.Add(layout);

            var usage = new Label()
            {
                Text = subcommand.Usage,
                Dock = DockStyle.Top,
                AutoSize = true,
            };

            layout.Controls.Add(usage, 0, 0);

            var button = new Button()
            {
                Text = subcommand.Name,
                AutoSize = true,
            };

            button.Click += new EventHandler((s, e) =>
            {
                var subCommandDialog = ToDialog(subcommand.Parser);
                subCommandDialog.ShowDialog();
            });

            layout.Controls.Add(button, 0, 2);
            return control;
        }

        bool ExcludedFromUi(IParserItem item)
        {
            var o = item.Source.Instance;
            return ((
                    o is ShowHelp ||
                    o is ShowUserInterface ||
                    o is ShowWebServer ||
                    o is LogOptions ||
                    false));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        Form ToDialog(Parser parser)
        {
            Form main = new Form();

            var mouseWheelSupport = new MouseWheelSupport(main);

            main.Width = 800;
            main.Height = 600;
            main.Text = parser.ApplicationName;

            var t = ToTabControl(parser);

            main.Controls.Add(t);

            return main;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        Control ToTabControl(Parser parser)
        {
            var t = new ControlTree();
            t.Dock = DockStyle.Fill;
            AddToControlTree(t, parser, null);
            t.treeView.ExpandAll();
            return t;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        void AddToControlTree(ControlTree t, Parser parser, Control parentPage)
        {
            AddToControlTree(t, parser.MainSource.Instance.GetType().Name, parser, parentPage);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        void AddToControlTree(ControlTree t, string title, Parser parser, Control parentPage)
        {
            var items = parser.Items
                .Where(item => !(ExcludedFromUi(item) || item is SubCommand));
            var categories = items.SelectMany(i => i.Categories).Distinct().ToList();

            // main page
            var mainPage = new Control()
            {
                Text = title
            };
            AddToPage(mainPage, items.Where(x => x.Categories.Contains(String.Empty)));
            t.AddPage(mainPage, parentPage);

            foreach (var category in categories.Where(x => !String.IsNullOrEmpty(x)))
            {
                var categoryPage = new Control()
                {
                    Text = String.IsNullOrEmpty(category) ? "General" : category
                };
                AddToPage(categoryPage, items.Where(x => x.Categories.Contains(category)));
                t.AddPage(categoryPage, mainPage);
            }

            foreach (var subCommand in parser.SubCommands.Where(x => !ExcludedFromUi(x)))
            {
                AddToControlTree(t, subCommand.Name, subCommand.Parser, mainPage);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000: DisposeObjectsBeforeLosingScope")]
        void AddToPage(Control page, IEnumerable<IParserItem> items)
        {
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

                bool first = true;
                foreach (var item in items)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        var separator = new Label()
                        {
                            AutoSize = false,
                            Height = 2,
                            Width = c.Width,
                            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                            BorderStyle = BorderStyle.Fixed3D
                        };
                        c.Controls.Add(separator);
                    }
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
                        continue;
                    }

                    if (childControl != null)
                    {
                        childControl.Width = 200;
                        c.Controls.Add(childControl);
                    }
                }
            }

        public void GraphicalUserInterface()
        {
            var main = ToDialog(parser);
            main.ShowDialog();
        }

        public void Dialog(string command)
        {
            var action = (Action) parser.LookupParserItem(command);
            var dialog = ToDialog(action);
            dialog.ShowAndExecute();
        }

        List<System.Action> refreshActions = new List<System.Action>();
        Dictionary<string, string> parameterCache = new Dictionary<string, string>();

        void Refresh()
        {
            foreach (var i in refreshActions)
            {
                i();
            }
        }

        void button_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var at = (ActionTag) button.Tag;

            foreach (var textBox in at.ParameterTextBoxes)
            {
                var p = (ParameterInfo) textBox.Tag;
                parameterCache[p.Name] = textBox.Text;
            }

            at.Execute();
            Refresh();
        }

        public static void SetError(Control c, string message)
        {
            c.BackColor = errorColor;
            tooltip.SetToolTip(c, message);
            tooltip.ShowAlways = true;
            tooltip.Show(message, c);
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

        [SuppressMessage("Microsoft.Design", "CA1031")]
        void textBoxOption_Leave(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
