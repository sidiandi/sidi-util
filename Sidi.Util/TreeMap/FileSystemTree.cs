using Sidi.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.TreeMap
{
    public class FileSystemTree
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Tree<Sidi.IO.IFileSystemInfo> Create(LPath root)
        {
            return new Tree<Sidi.IO.IFileSystemInfo>(
                root.Info,
                root.Info.GetChildren().Select(_ => Create(_.FullName)));
        }
    }
}
