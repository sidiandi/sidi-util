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
using System.Web;
using System.Diagnostics;

namespace Sidi.Util
{
    public class ArchiveHandler
    {
        string m_rootDirectory;

        public ArchiveHandler(string cache)
        {
            m_rootDirectory = cache;
        }

        class Info
        {
            public bool Error = false;
            public int ExtractedFilesCount = 0;
        }

        Sidi.Collections.LruCache<string, Info> m_extracted = new Sidi.Collections.LruCache<string, Info>(
            500, 
            new Sidi.Collections.LruCache<string, Info>.ProvideValue( delegate { return new Info(); } ));

        public string Extract(Uri uri, string archive)
        {
            lock (this)
            {
                string cacheDir = Path.Combine(m_rootDirectory, archive.Replace(":", ""));
                string archiveFile = HttpUtility.ParseQueryString(uri.Query)["f"];
                string cachePath = Path.Combine(cacheDir, archiveFile);

                ++m_extracted[archive].ExtractedFilesCount;

                if (File.Exists(cachePath))
                {
                    return cachePath;
                }

                if (m_extracted[archive].Error)
                {
                    return null;
                }

                IArchive extractor = ProvideArchive(archive);
                if (extractor is Rar)
                {
                    Rar rar = (Rar)extractor;
                    rar.Password = GetPassword(archive);
                }
                extractor.ExtractAll(cacheDir);

                if (File.Exists(cachePath))
                {
                    return cachePath;
                }

                m_extracted[archive].Error = true;

                return null;
            }
        }

        public static IArchive ProvideArchive(string path)
        {
            string ext = new FileInfo(path).Extension.ToLower();
            if (ext == ".zip")
            {
                return new Zip(path);
            }
            if (ext == ".rar")
            {
                return new Rar(path);
            }
            throw new ArgumentOutOfRangeException(path);
        }

        public System.IO.Stream Get(Uri uri, string archive)
        {
            lock (this)
            {
                string cachePath = Extract(uri, archive);
                if (cachePath == null)
                {
                    return null;
                }
                else
                {
                    return File.OpenRead(cachePath);
                }
            }
        }

        private string GetPassword(string archive)
        {
            string passwordFile = Path.Combine(
                Path.GetDirectoryName(archive),
                "pass.txt");
            
            if (File.Exists(passwordFile))
            {
                string[] content = File.ReadAllLines(passwordFile);
                foreach (string i in content)
                {
                    string[] data = i.Split(new string[]{"\t"}, StringSplitOptions.None);
                    if (data.Length == 1)
                    {
                        return data[0];
                    }
                    if (data.Length == 2)
                    {
                        string filter = data[0];
                        if (archive.ToLower().Contains(filter.ToLower()))
                        {
                            return data[1];
                        }
                    }
                }
            }
            return null;
        }
    }
}
