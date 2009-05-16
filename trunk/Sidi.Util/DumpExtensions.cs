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
using System.Text;
using System.IO;
using System.Reflection;

namespace Sidi.Util
{
    public static class DumpExtensions
    {
        public static void DumpProperties(this object x, TextWriter o)
        {
            foreach (PropertyInfo i in x.GetType().GetProperties())
            {
                o.WriteLine(String.Format("{0} = {1}", i.Name, i.GetValue(x, new object[] { })));
            }

            foreach (FieldInfo i in x.GetType().GetFields())
            {
                o.WriteLine(String.Format("{0} = {1}", i.Name, i.GetValue(x)));
            }
        }
    }
}
