using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO;

namespace Sidi.Tool
{
    public class RenameOperation
    {
        public LPath From;
        public LPath To;

        public void Rename()
        {
            if (From.Equals(To))
            {
                return;
            }
            if (To.IsFile)
            {
                return;
            }

            To.EnsureParentDirectoryExists();
            LFile.Move(From, To);
        }

        public static void Rename(IList<RenameOperation> operations)
        {
            foreach (var i in operations)
            {
                i.Rename();
            }
        }
    }
}
