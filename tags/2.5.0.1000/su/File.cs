using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.Extensions;
using Sidi.Persistence;
using Sidi.Visualization;
using Sidi.Util;
using System.Windows.Forms;
using System.Diagnostics;
using Sidi.Forms;
using System.Data;
using Sidi.IO;
using System.Reflection;

namespace Sidi.Tool
{
    [Usage("Operations on long file names")]
    public class Files
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Usage("Rename specified files interactively")]
        public void Rename(PathList files)
        {
            var renameDialog = new RenameDialog();
            renameDialog.Files = Sidi.IO.Find.AllFiles(files).Select(x => x.FullName).ToList();
            renameDialog.ShowDialog();
        }

        [Usage("Move a list of files to a directory")]
        public void Move(PathList files, LPath destinationDirectory)
        {
            var op = FileOperation;

            foreach (var i in files)
            {
                var d = destinationDirectory.CatDir(i.FileName);
                op.Move(i, d);
            }
        }

        [Usage("Overwrite files in subsequent operations")]
        public void Overwrite()
        {
            FileOperation.Overwrite = true;   
        }

        Operation FileOperation
        {
            get
            {
                if (_FileOperation == null)
                {
                    _FileOperation = new Operation()
                    {
                        Fast = true
                    };
                }
                return _FileOperation;
            }
        }
        Operation _FileOperation;

        [Usage("Copy a list of files to a directory. Preserves hard links.")]
        public void Copy(PathList files, LPath destinationDirectory)
        {
            var op = new HardLinkPreservingCopyOperation();

            foreach (var i in files)
            {
                var d = destinationDirectory.CatDir(i.FileName);
                op.Copy(i, d);
            }
        }

        [Usage("Hardlink a list of files to a directory.")]
        public void Link(PathList files, LPath destinationDirectory)
        {
            var op = FileOperation;

            foreach (var i in files)
            {
                var d = destinationDirectory.CatDir(i.FileName);
                op.Link(i, d);
            }
        }

        [Usage("Remove empty directories")]
        public void RemoveEmptyDirectories(PathList files)
        {
            foreach (var path in files)
            {
                RemoveEmptyDirectories(path);
            }
        }

        [Usage("Removes directories with only one entry")]
        public void Flatten(PathList files)
        {
            foreach (var i in files)
            {
                Flatten(i);
            }
        }

        LPath Flatten(LPath path)
        {
            if (!path.IsDirectory)
            {
                return path;
            }

            foreach (var c in path.Children)
            {
                Flatten(c);
            }

            var children = path.Children;

            if (children.Count == 0)
            {
                log.InfoFormat("Delete empty directory {0}", path);
                LDirectory.Delete(path);
                return null;
            }
            else if (children.Count == 1)
            {
                try
                {
                    var op = new Operation();
                    var c = children.First();

                    if (c.FileName.Equals(path.FileName))
                    {
                        var newPath = path.Sibling(LPath.GetTempFileName().FileName);
                        op.Move(path, newPath);
                        path = newPath;
                        c = path.Children.First();
                    }

                    var cNew = path.Parent.CatDir(c.FileName);
                    if (!cNew.Exists)
                    {
                        log.InfoFormat("Flatten {0} to {1}", c, cNew);
                        op.Move(c, cNew);
                        op.DeleteEmptyDirectories(path);
                        return cNew;
                    }
                    else
                    {
                        return path;
                    }
                }
                catch (Exception ex)
                {
                    log.Warn(path, ex);
                }
                return path;
            }
            else
            {
                return path;
            }
        }

        void RemoveEmptyDirectories(LPath path)
        {
            if (LDirectory.Exists(path))
            {
                var thumbs = path.CatDir("Thumbs.db");
                if (thumbs.Exists)
                {
                    LFile.Delete(thumbs);
                }
                foreach (var d in path.GetDirectories())
                {
                    RemoveEmptyDirectories(d);
                }

                try
                {
                    LDirectory.Delete(path);
                    log.InfoFormat("Delete {0}", path);
                }
                catch
                {
                }
            }
        }

        [Usage("Deletes recursively")]
        public void Delete(PathList files)
        {
            foreach (var path in files)
            {
                try
                {
                    if (path.IsDirectory)
                    {
                        Delete(new PathList(path.Children));
                        LDirectory.Delete(path);
                    }
                    else
                    {
                        LFile.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    log.Warn(path, ex);
                }
            }
        }

        public class Item
        {
            [RowId]
            public int ID;

            [Data, Indexed, Unique]
            public string LPath;

            [Data, Indexed]
            public long Length;

            [Data, Indexed]
            public DateTime LastWriteTimeUtc;

            [Data]
            public string Digest;
        }

        IEnumerable<LFileSystemInfo> AllFilesRecursive(PathList fileList)
        {
            return fileList.SelectMany(root => Find.AllFiles(root));
        }

        [Usage("Show a directory as a cushion tree map")]
        public void TreeMap(PathList files)
        {
            var cache = Sidi.Caching.Cache.Local(MethodBase.GetCurrentMethod());
            cache.MaxAge = TimeSpan.FromDays(1);
            var items = cache.GetCached(files, () => Find.AllFiles(files).ToList());

            var c = items.CreateTreeMap();
            c.GetLineage = i => i.FullName.Parts;
            c.GetSize = i => i.Length;
            c.SetPercentileColorScale(i => i.LastWriteTime, ColorScale.GetBlueRed());
            c.GetText = i => i.Name;
            c.Activate = i => Process.Start(i.FullName);
            c.ContextMenuStrip.Opening += (s, e) =>
                {
                    var strip = c.ContextMenuStrip;
                    foreach (var tree in c.GetLineageAtMouse())
                    {
                        strip.Items.Add(
                            String.Format("Open {0}", tree.SafeToString()), null, (s1, e1) =>
                            {
                                var p = LPath.Join(tree.Lineage.Select(x => (string)x).ToArray());
                                Process.Start(p);
                            });
                    }
                };

            c.RunFullScreen();
        }

        public class NameTree : ITree
        {
            public NameTree(NameTree parent, string name)
            {
                this.parent = parent;
                this.name = name;
                if (parent != null)
                {
                    parent.children.Add(name, this);
                }
            }

            public NameTree ProvideChild(string name)
            {
                NameTree c;
                if (!children.TryGetValue(name, out c))
                {
                    c = new NameTree(this, name);
                }
                return c;
            }

            public IEnumerable<ITree> Children
            {
                get { return children.Values.OrderByDescending(x => x.Size); }
            }

            public ITree Parent
            {
                get { return parent; }
            }

            public float Size
            {
                get { return size; }
                set { size = value; }
            }

            public void UpdateSize()
            {
                if (children.Count == 0)
                {
                }
                else
                {
                    size = 0.0f;
                    foreach (var c in children.Values)
                    {
                        c.UpdateSize();
                        size += c.Size;
                    }
                }
            }

            public IEnumerable<NameTree> Lineage
            {
                get
                {
                    return ReverseLineage.Reverse();
                }
            }

            IEnumerable<NameTree> ReverseLineage
            {
                get
                {
                    for (NameTree t = this; t != null; t = t.parent)
                    {
                        yield return t;
                    }
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }


            NameTree parent;
            public string name;
            public float size;
            System.Collections.Generic.Dictionary<string, NameTree> children = new System.Collections.Generic.Dictionary<string, NameTree>();
        }
    }
}
