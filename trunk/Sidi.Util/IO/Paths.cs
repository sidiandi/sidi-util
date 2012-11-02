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
