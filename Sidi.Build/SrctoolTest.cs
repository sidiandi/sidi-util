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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.IO;
using System.IO;
using Sidi.Build;
using L = Sidi.IO.Long;

namespace Sidi.Build.Test
{
    /// <summary>
    /// Test for SrcTool
    /// </summary>
    [TestFixture]
    public class SrctoolTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Srctool instance = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SrctoolTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }
        
        /// <summary>
        /// Creates a fresh SrcTool instance
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            instance = new Srctool();
        }

        string pdbFile = L.Paths.BinDir.CatDir("Sidi.Util.pdb");

        /// <summary>
        /// Dump source information of a PDB file
        /// </summary>
        [Test]
        public void Test()
        {
            foreach (var i in instance.DumpRaw(pdbFile))
            {
                log.Info(i);
            }
        }

        /// <summary>
        /// Write source server information to a PDB file
        /// </summary>
        [Test]
        public void Instrument()
        {
            if (!File.Exists(pdbFile))
            {
                throw new FileNotFoundException(pdbFile);
            }

            string pdbFileIndexed = TestFile(Path.GetFileName(pdbFile));

            File.Copy(pdbFile, pdbFileIndexed, true);

            StringWriter w = new StringWriter();

            Srctool srctool = new Srctool();

            w.WriteLine("SRCSRV: ini ------------------------------------------------");
            w.WriteLine("VERSION=2");
            w.WriteLine("INDEXVERSION=2");
            w.WriteLine("VERCTRL=http");
            w.WriteLine("DATETIME={0}", DateTime.Now.ToString());
            w.WriteLine("SRCSRV: variables ------------------------------------------");
            w.WriteLine("SRCSRVVERCTRL=http");
            w.WriteLine("HTTP_ALIAS=http://sidi-util.googlecode.com/svn/trunk");
            w.WriteLine("HTTP_EXTRACT_TARGET=%HTTP_ALIAS%/%var2%");
            w.WriteLine("SRCSRVTRG=%http_extract_target%");
            w.WriteLine("SRCSRVCMD=");
            w.WriteLine("SRCSRV: source files ---------------------------------------");
            
            foreach (string i in srctool.DumpRaw(pdbFileIndexed))
            {
                string url = i.Substring(@"D:\work\Sidi.Util\".Length);
                url = url.Replace(@"\", "/");

                w.WriteLine(
                    String.Join("*", new string[]
                    {
                        i, url,
                    }));
            }

            w.WriteLine("SRCSRV: end ------------------------------------------------");

            Pdbstr pdbstr = new Pdbstr();
            pdbstr.Write(pdbFileIndexed, SourceIndex.SrcsrvStream, w.ToString());

            srctool.Extract(pdbFileIndexed);
        }
    }
}
