// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            Recurse(Sidi.IO.Path.GetFileSystemInfo(path), handler);
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
