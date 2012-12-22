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
using Sidi.Util;
using System.IO;
using Sidi.CommandLine;
using System.Text.RegularExpressions;

namespace Sidi.IO
{
    /// <summary>
    /// Command line parser interface for Sidi.IO.Enum
    /// </summary>
    public class FindConfig
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Find Enumerator = new Find();
        List<Regex> exclude = new List<Regex>();
        
        public FindConfig()
        {
            Enumerator.Output = x => !exclude.Any(e => e.IsMatch(x.Name));
            Enumerator.Follow = x => !exclude.Any(e => e.IsMatch(x.Name));
        }

        [Usage("Include a path")]
        public void Include(string p)
        {
            Enumerator.Roots.Add(p);
        }

        [Usage("Exclude a file name pattern")]
        public void Exclude(string p)
        {
            exclude.Add(new Regex(p));
        }
    }
}
