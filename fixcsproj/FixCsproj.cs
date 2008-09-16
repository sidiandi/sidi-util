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
using Sidi.CommandLine;
using System.IO;
using System.Xml;

namespace fixcsproj
{
    public static class Extensions
    {
        public static string Basename(FileSystemInfo f)
        {
            return System.IO.Path.GetFileNameWithoutExtension(f.Name);
        }
    }
    
    [Usage("Fixes the directory structure of Microsoft Visual Studio *.csproj files.")]
    public class FixCsproj
    {
        static void Main(string[] args)
        {
            Sidi.CommandLine.Parser.Run(new FixCsproj(), args);
        }

        bool bak = false;

        DirectoryInfo m_bin = new DirectoryInfo(@".\bin");

        [Usage("bin directory")]
        public DirectoryInfo Bin
        {
            get { return m_bin; }
            set { m_bin = value; }
        }

        bool m_save = true;

        [Usage("Commit changes")]
        public bool Commit
        {
            get { return m_save; }
            set { m_save = value; }
        }

        string m_csProjPath;

        [Usage("Fixes all csproj files in the given directory tree.")]
        public void Fix(string path)
        {
            Sidi.IO.Find find = new Sidi.IO.Find();
            find.Recurse(path, new Sidi.IO.Find.FileHandler(delegate(FileInfo f)
            {
                if (f.Extension == ".csproj")
                {
                    try
                    {
                        Info(f.FullName);
                        XmlDocument doc = new XmlDocument();
                        m_csProjPath = f.FullName;
                        doc.Load(f.FullName);

                        if (bak)
                        {
                            string bakPath = f.FullName + ".fixcsproj";
                            if (!File.Exists(bakPath))
                            {
                                doc.Save(bakPath);
                            }
                        }

                        HandleCsproj(doc);
                        
                        if (Commit)
                        {
                            Info("Writing " + f.FullName);
                            doc.Save(f.FullName);
                        }

                        try
                        {
                            Directory.Delete(Sidi.IO.Path.Sibling(f.FullName, "bin"), true);
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            Directory.Delete(Sidi.IO.Path.Sibling(f.FullName, "obj"), true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    catch (Exception e)
                    {
                        Info(e.ToString());
                    }
                }
            }));
        }

        [Usage("Checks source files for correct comment headers.")]
        public void Header(string directory)
        {
            HeaderRecurse(new DirectoryInfo(directory), new HeaderInfo());
        }

        class HeaderInfo : Dictionary<string, string>
        {
            public HeaderInfo(HeaderInfo src)
                : base(src)
            {
            }

            public HeaderInfo()
            {
            }
        }

        void HeaderRecurse(DirectoryInfo directory, HeaderInfo headers)
        {
            FileInfo[] files = directory.GetFiles();
            HeaderInfo newHeaders = new HeaderInfo(headers);
            foreach (FileInfo i in files)
            {
                if (i.Extension == ".header")
                {
                    string ext = "." + Extensions.Basename(i);
                    newHeaders[ext] = File.ReadAllText(i.FullName);
                    Console.WriteLine(newHeaders[ext]);
                }
            }

            foreach (FileInfo i in files)
            {
                if (newHeaders.ContainsKey(i.Extension))
                {
                    string header = newHeaders[i.Extension];
                    string c = File.ReadAllText(i.FullName);
                    if (c.StartsWith(header))
                    {
                        Console.WriteLine(String.Format("{0}: header is ok.", i.FullName));
                    }
                    else
                    {
                        Console.WriteLine(String.Format("{0}: Add header\r\n{1}", i.FullName, header));
                        c = header + c;
                        if (Commit)
                        {
                            File.WriteAllText(i.FullName, c);
                        }
                    }
                }
            }

            foreach (DirectoryInfo i in directory.GetDirectories())
            {
                HeaderRecurse(i, newHeaders);
            }
        }

        string csprojNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public void HandleCsproj(XmlDocument doc)
        {
           string OutputPath = "OutputPath";
           string IntermediatePath = "IntermediatePath";

           XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
           namespaceManager.AddNamespace("ms", csprojNamespace);

            foreach (XmlNode i in doc.SelectNodes("//ms:Project/ms:PropertyGroup", namespaceManager))
            {
                PropertyGroup pg = new PropertyGroup(i);
                string outputPath = @"$(SolutionDir)..\build\$(SolutionName)\bin\$(Configuration)";
                Info(outputPath);
                pg[OutputPath] = outputPath;
                pg[IntermediatePath] = @"$(SolutionDir)..\build\$(SolutionName)\obj\$(Configuration)\$(AssemblyName)";
            }
        }

        void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}
