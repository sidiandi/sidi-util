﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sidi.IO;
using System.IO;
using Sidi.Build;

namespace Sidi.Build.Test
{
    [TestFixture]
    public class SrctoolTestTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Srctool instance = null;

        public SrctoolTestTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }
        
        [SetUp]
        public void SetUp()
        {
            instance = new Srctool();
        }

        string pdbFile = @"D:\work\lib\Sidi.Util\bin\Debug\Sidi.Util.pdb";

        [Test]
        public void Test()
        {
            foreach (var i in instance.DumpRaw(pdbFile))
            {
                log.Info(i);
            }
        }

        [Test]
        public void Instrument()
        {
            if (!File.Exists(pdbFile))
            {
                throw new FileNotFoundException(pdbFile);
            }

            string pdbFileIndexed = FileUtil.CatDir(@"D:\test\ConsoleApplication3\lib\Sidi.Util\bin\Debug", Path.GetFileName(pdbFile));

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
