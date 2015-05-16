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
using log4net;
using Sidi.Collections;
using Sidi.IO;

namespace Sidi.CommandLine
{
    public class ShowUserInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Parser parser;
        Lazy<UserInterface> userInterface;

        UserInterface UserInterface { get { return userInterface.Value; } }

        public ShowUserInterface(Parser parser)
        {
            this.parser = parser;
            this.userInterface = new Lazy<UserInterface>(() => new UserInterface(this.parser));
        }

        [Usage("Show an interactive user interface")]
        [Category(Parser.categoryUserInterface)]
        public void GraphicalUserInterface()
        {
            UserInterface.GraphicalUserInterface();
        }

        [Usage("Show dialog for the specified command")]
        [Category(Parser.categoryUserInterface)]
        public void Dialog(string command)
        {
            UserInterface.Dialog(command);
        }

        [Usage("Console to type commands")]
        [Category(Parser.categoryUserInterface)]
        public void Shell()
        {
            var shellSupport = new ShellSupport();
            Console.WriteLine("type 'exit' to quit");
            shellSupport.Shell(parser);

        }

        [Usage("Runs commands from a script file")]
        [Category(Parser.categoryUserInterface)]
        public void Script(LPath scriptFile)
        {
            string[] parameters = Tokenizer.FromFile(scriptFile);
            parser.Parse(parameters);
        }
    }
}
