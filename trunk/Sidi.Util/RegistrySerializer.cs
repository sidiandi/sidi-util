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
using Microsoft.Win32;

namespace Sidi.Util
{
    public class RegistrySerializer
    {
        public static void Read(string key, object x)
        {
            foreach (var f in x.GetType().GetFields())
            {
                var value = Registry.GetValue(key, f.Name, null);
                if (value == null)
                {
                    continue;
                }

                if (f.FieldType == typeof(bool))
                {
                    value = ((int)value) != 0;
                }
                else if (f.FieldType == typeof(int))
                {
                }
                else if (f.FieldType == typeof(string))
                {
                }
                else
                {
                    throw new Exception(String.Format("{0} type is not supported", f.FieldType.ToString()));
                }
                f.SetValue(x, value);
            }
        }

        public static void Write(string key, object x)
        {
            foreach (var f in x.GetType().GetFields())
            {
                object value = f.GetValue(x);
                if (value is bool)
                {
                    value = (bool)value ? 1 : 0;
                }
                else if (value is int)
                {
                }
                else if (value is string)
                {
                }
                else
                {
                    throw new Exception(String.Format("{0} type is not supported", f.FieldType.ToString()));
                }
                Registry.SetValue(key, f.Name, value);
            }
        }
    }
}
