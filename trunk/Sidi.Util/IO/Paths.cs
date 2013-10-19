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

        public static LPath GetLocalPath(this Assembly assembly)
        {
            return new LPath(new Uri(assembly.CodeBase).LocalPath);
        }

        /// <summary>
        /// Returns the local application directory sub directory for a Type:
        /// [LocalApplicationData]\[company name]\[assembly name]\[version]\[full type name]
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LPath GetLocalApplicationDataDirectory(Type type)
        {
            return GetFolderPath(type, System.Environment.SpecialFolder.LocalApplicationData);
        }

        /// <summary>
        /// Returns the sub directory of a special folder for a Type:
        /// [special folder]\[company name]\[assembly name]\[version]\[full type name]
        /// </summary>
        /// <param name="type">Type for the sub directory</param>
        /// <param name="folder">root folder</param>
        /// <returns></returns>
        public static LPath GetFolderPath(this Type type, System.Environment.SpecialFolder folder)
        {
            var assembly = type.Assembly;
            var dir = Paths.GetFolderPath(folder).CatDir(Get(type));
            return dir;
        }

        /// <summary>
        /// Returns a relative path for an assembly. Can be used to determine the sub-directory
        /// for the AppData directories.
        /// [company name]\[assembly name]\[version].
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static LPath Get(this Assembly a)
        {
            var v = a.GetName().Version;
            return LPath.Join(
                LPath.GetValidFilename(a.GetCustomAttribute<AssemblyCompanyAttribute>().Company),
                LPath.GetValidFilename(a.GetName().Name),
                new []{ v.Major, v.Minor }.Join(LPath.ExtensionSeparator)
                );
        }

        /// <summary>
        /// Returns a relative path for type. Can be used to determine the sub-directory
        /// for the AppData directories.
        /// [company name]\[assembly name]\[version]\[type name].
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static LPath Get(this Type type)
        {
            return LPath.Join(Get(type.Assembly), type.FullName);
        }

        public static LPath BinDir
        {
            get
            {
                return GetLocalPath(Assembly.GetExecutingAssembly()).Parent;
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
