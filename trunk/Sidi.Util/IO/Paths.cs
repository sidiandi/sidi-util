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
using Sidi.Extensions;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Reflection;

namespace Sidi.IO
{
    public static class Paths
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static LPath LocalPath(this Assembly assembly)
        {
            return new LPath(new Uri(assembly.CodeBase).LocalPath);
        }

        public static LPath UserSetting(this Type type, string name)
        {
            var root = new LPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
            var assemblyName = type.Assembly.GetName().Name;
            var path = root.CatDir(assemblyName, name);
            return path;
        }

        public static LPath BinDir
        {
            get
            {
                return LocalPath(Assembly.GetExecutingAssembly()).Parent;
            }
        }

        public static LPath Temp
        {
            get
            {
                return new LPath(System.IO.Path.GetTempPath());
            }
        }

        public static LPath GetFolderPath(Environment.SpecialFolder sf)
        {
            return new LPath(System.Environment.GetFolderPath(sf));
        }
    }
}
