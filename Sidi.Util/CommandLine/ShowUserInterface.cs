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
using System.Drawing;
using Sidi.Util;
using Sidi.Forms;
using System.Reflection;
using System.ComponentModel;
using Sidi.Extensions;
using System.Diagnostics.CodeAnalysis;
using log4net.Repository.Hierarchy;
using log4net;
using Sidi.Collections;

namespace Sidi.CommandLine
{
    public class ShowUserInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser parser;

        public ShowUserInterface(Parser parser)
        {
            LogLevel = log4net.Core.Level.Error;
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
                paramInput.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.Enter)
                        {
                            button.PerformClick();        
                        }
                    };
                paramInput.Tag = p;
                paramsPanel.Controls.Add(paramInput, 1, row);

                at.ParameterTextBoxes.Add(paramInput);

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

            var paramInput = new TextBox()
            {
                Dock = DockStyle.Fill,
            };
            paramInput.TextChanged += (s,e) =>
                {
                };

            if (option.IsPassword)
            {
                paramInput.PasswordChar = '*';
            }
            paramInput.Tag = option;
            paramInput.TextChanged += new EventHandler(paramInput_Leave);
            paramsPanel.Controls.Add(paramInput, 1, row);

            layout.Controls.Add(paramsPanel, 0, 1);

            return control;
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
                var p = new Parser(subcommand.CommandInstance);
                var subCommandDialog = ToDialog(p);
                subCommandDialog.ShowDialog();
            });

            layout.Controls.Add(button, 0, 2);
            return control;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        Control ToControl_old(Option option)
        {
            var panel = new TableLayoutPanel()
            {
                ColumnCount = 2,
                AutoSize = true,
            };

            var label = new Label()
            {
                Text = option.Usage,
                AutoSize = true
            };
            panel.Controls.Add(label,0,0);

            var paramLabel = new Label();
            paramLabel.Text = option.Syntax;
            paramLabel.AutoSize = true;
            panel.Controls.Add(paramLabel,0,1);

            var paramInput = new TextBox();
            inputs.Add(paramInput);
            if (option.IsPassword)
            {
                paramInput.PasswordChar = '*';
            }
            panel.Controls.Add(paramInput,1,1);
            paramInput.Left = paramLabel.Right + margin;

            paramInput.Text = Support.SafeToString(option.GetValue());
            paramInput.Tag = option;
            paramInput.TextChanged += new EventHandler(paramInput_Leave);

            return panel;
        }

        bool ExcludedFromUi(IParserItem item)
        {
            return ((
                    item.Application is ShowHelp ||
                    item.Application is ShowUserInterface ||
                    item.Application is ShowWebServer ||
                    false));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        Form ToDialog(Parser parser)
        {
            Form main = new Form();

            var mouseWheelSupport = new MouseWheelSupport(main);

            main.Width = 640;
            main.Height = 480;
            main.Text = parser.ApplicationName;

            var t = ToTabControl(parser);

            main.Controls.Add(t);

            return main;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        TabControl ToTabControl(Parser parser)
        {
            var t = new TabControl();
            t.Dock = DockStyle.Fill;

            var items = parser.Items
                .Where(item => !(ExcludedFromUi(item) || item is SubCommand));
            var categories = items.SelectMany(i => i.Categories).Distinct().ToList();

            foreach (var category in categories)
            {
                var page = new TabPage(String.IsNullOrEmpty(category) ? "General" : category);

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
                foreach (var item in items.Where(x => x.Categories.Contains(category)))
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

                t.TabPages.Add(page);
            }

            foreach (var subCommand in parser.SubCommands.Where(x => !ExcludedFromUi(x)))
            {
                var scPage = new TabPage(subCommand.Name);
                var scControl = ToTabControl(new Parser(subCommand.CommandInstance));
                scControl.Dock = DockStyle.Fill;
                scPage.Controls.Add(scControl);
                t.TabPages.Add(scPage);
            }

            return t;
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
            dialog.ShowAndExecute();
        }

        public static log4net.Core.Level ParseLevel(string stringRepresentation)
        {
            var levels = typeof(log4net.Core.Level).GetFields(BindingFlags.Static | BindingFlags.Public);
            var selected = levels.First(i => Parser.IsMatch(stringRepresentation, i.Name));
            return (log4net.Core.Level) selected.GetValue(null);
        }

        [Usage("Log level (off, error, warn, info, debug)"), Persistent]
        [Category(Parser.categoryUserInterface)]
        public log4net.Core.Level LogLevel
        {
            get { return logLevel; }
            set
            {
                logLevel = value;

                var hierarchy = (Hierarchy)LogManager.GetRepository();
                hierarchy.Root.Level = value;
            }
        }
        log4net.Core.Level logLevel;

        [Usage("Console to type commands")]
        [Category(Parser.categoryUserInterface)]
        public void Shell()
        {
            Console.WriteLine("type 'exit' to quit");
            var tokenizer = new Tokenizer(new ReadAheadTextReader(Console.In));
            var args = new EnumList<string>(tokenizer.Tokens.GetEnumerator());
            while (args.Any())
            {
                if (Parser.IsMatch(args[0], "exit"))
                {
                    break;
                }

                try
                {
                    parser.Parse(args);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    Console.WriteLine(ex.Message);
                    args.Clear();
                }
            }
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

        [SuppressMessage("Microsoft.Design", "CA1031")]
        void paramInput_TextChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            var p = (ParameterInfo)textBox.Tag;
            try
            {
                parser.ParseValue(Tokenizer.ToList(textBox.Text), p.ParameterType);
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

        [SuppressMessage("Microsoft.Design", "CA1031")]
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
