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
using System.Text;
using System.IO;

namespace Sidi.IO
{
    public class Find
    {
        public void Recurse(string path, FileHandler handler)
        {
            Recurse(Sidi.IO.FileUtil.GetFileSystemInfo(path), handler);
        }

        int m_maxLevel = Int32.MaxValue;

        public int MaxLevel
        {
            get { return m_maxLevel; }
            set { m_maxLevel = value; }
        }

        int m_level = 0;

        public int Level
        {
            get { return m_level; }
        }

        public void Recurse(FileSystemInfo file, FileHandler handler)
        {
            OnFile = handler;
            MaxLevel = Int32.MaxValue;
            Recurse(file, 0);
        }

        public void Recurse(FileSystemInfo file)
        {
            Recurse(file, 0);
        }

        void Recurse(FileSystemInfo file, int level)
        {
            m_level = level;

            DirectoryInfo d = file as DirectoryInfo;
            if (d == null)
            {
                FileInfo f = file as FileInfo;
                if (f != null)
                {
                    if (OnFile != null) OnFile(f);
                }
            }
            else
            {
                if (OnDirectory != null) OnDirectory(d);

                if (level < m_maxLevel)
                {
                    ++level;
                    foreach (FileSystemInfo i in d.GetFileSystemInfos())
                    {
                        Recurse(i, level);
                    }
                }
            }
        }

        public delegate void DirectoryHandler(DirectoryInfo directory);
        public DirectoryHandler OnDirectory = null;

        public delegate void FileHandler(FileInfo file);
        public FileHandler OnFile = null;
    }
}
