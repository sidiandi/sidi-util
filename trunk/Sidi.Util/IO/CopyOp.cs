using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sidi.IO
{
    public class CopyOp
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Move(LPath from, LPath to)
        {
            if (LFile.Exists(from))
            {
                to.EnsureParentDirectoryExists();
                LFile.Move(from, to);
            }
            else if (LDirectory.Exists(from))
            {
                to.EnsureParentDirectoryExists();
                LDirectory.Move(from, to);
            }
            log.InfoFormat("moved {0} to {1}", from, to);
        }
        
        public CopyOp()
        {
            CopyRequired = (s, d) => true;
            DoCopy = (s, d) =>
                {
                    d.EnsureParentDirectoryExists();
                    LFile.Copy(s, d);
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

        public Func<FileSystemInfo, FileSystemInfo, bool> CopyRequired;
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
                                LFile.CreateHardLink(d, s);
                            }
                            catch (System.IO.IOException)
                            {
                                LFile.Copy(s, d);
                            }
                        }
                        else
                        {
                            LFile.Copy(s, d, Overwrite);
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
