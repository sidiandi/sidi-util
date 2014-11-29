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

namespace Sidi.IO
{
    public class CopyOp
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IFileSystem fs = FileSystem.Current;

        public void Move(LPath from, LPath to)
        {
            if (from.IsFile)
            {
                to.EnsureParentDirectoryExists();
                fs.Move(from, to);
            }
            else if (from.IsDirectory)
            {
                to.EnsureParentDirectoryExists();
                fs.Move(from, to);
            }
            log.InfoFormat("moved {0} to {1}", from, to);
        }
        
        public CopyOp()
        {
            CopyRequired = (s, d) => true;
            DoCopy = (s, d) =>
                {
                    d.EnsureParentDirectoryExists();
                    fs.CopyFile(s, d);
                };
        }

        public void Copy(LPath source, LPath destination)
        {
            CopyRooted(source, source, destination);
        }

        public void CopyRooted(LPath source, LPath sourceRoot, LPath destinationRoot)
        {
            var e = new Find()
            {
                Root = source,
                Output = Find.OnlyFiles,
            };

            foreach (var s in e.Depth())
            {
                var d = destinationRoot.CatDir(s.FullName.RelativeTo(sourceRoot));
                if (CopyRequired(s, d.Info))
                {
                    DoCopy(s.FullName, d);
                }
            }
        }

        public Func<IFileSystemInfo, IFileSystemInfo, bool> CopyRequired;
        public Action<LPath, LPath> DoCopy;
    }

    public class SimpleCopyOp : CopyOp
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Simulate { get; set; }
        public bool Overwrite { get; set; }
        public bool UseHardlinks { get; set; }
        public enum SkipMode
        {
            TargetExists,
            LastWriteTimeAndLengthEqual,
            Never
        }
        public SkipMode Skip { get; set; }
        public bool ContinueOnError { get; set; }

        public SimpleCopyOp()
        {
            DoCopy = (s, d) =>
                {
                    log.InfoFormat("{0} -> {1}", s, d);
                    if (!Simulate)
                    {
                        d.EnsureParentDirectoryExists();
                        if (UseHardlinks)
                        {
                            try
                            {
                                fs.CreateHardLink(d, s);
                            }
                            catch (System.IO.IOException)
                            {
                                fs.CopyFile(s, d);
                            }
                        }
                        else
                        {
                            fs.CopyFile(s, d);
                        }
                    }
                };

            this.CopyRequired = (s, d) =>
                {
                    switch (Skip)
                    {
                        case SkipMode.Never:
                            return true;
                        case SkipMode.LastWriteTimeAndLengthEqual:
                            return !d.Exists || !(s.LastWriteTimeUtc.Equals(d.LastWriteTimeUtc) || s.Length.Equals(d.Length));
                        case SkipMode.TargetExists:
                            return !d.Exists;
                        default:
                            throw new System.InvalidOperationException();
                    }
                };
        }
    }
}
