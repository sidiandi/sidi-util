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
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;
using Sidi.Extensions;
using Sidi.Test;

namespace Sidi.Forms
{
    [TestFixture, RequiresSTA]
    public class PromptTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test, Explicit("interactive")]
        public void Edit()
        {
            var text = "Hello, World";
            var result = Prompt.EditInteractive(text);
            log.Info(result);
        }

        [Test, Explicit("interactive")]
        public void Choose()
        {
            var lf = Process.GetProcesses()
                .ListFormat()
                .AddColumn("Name", x => x.ProcessName)
                .AddColumn("id", x => x.Id);

            var selectedProcess = Prompt.ChooseOne(lf, "Process");
            log.Info(selectedProcess);
        }

        [Test, Explicit("interactive")]
        public void ChooseMany()
        {
            var lf = Process.GetProcesses()
                .ListFormat()
                .AddColumn("Name", x => x.ProcessName)
                .AddColumn("id", x => x.Id);

            var selectedProcess = Prompt.ChooseMany(lf);
            selectedProcess.ListFormat().RenderText();
        }

        [Test, Explicit("interactive")]
        public void Activate()
        {
            var lf = Process.GetProcesses()
                .ListFormat()
                .AddColumn("Name", x => x.ProcessName)
                .AddColumn("id", x => x.Id);

            Prompt.OnActivate(lf, p => log.Info(p));
        }
    }
}
