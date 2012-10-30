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

        public static Path LocalPath(this Assembly assembly)
        {
            return new Path(new Uri(assembly.CodeBase).LocalPath);
        }

        public static Path UserSetting(this Type type, string name)
        {
            var root = new Path(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
            var assemblyName = type.Assembly.GetName().Name;
            var path = root.CatDir(assemblyName, name);
            return path;
        }

        public static Path BinDir
        {
            get
            {
                return LocalPath(Assembly.GetExecutingAssembly()).Parent;
            }
        }

        public static Path Temp
        {
            get
            {
                return new Path(System.IO.Path.GetTempPath());
            }
        }
    }
}
