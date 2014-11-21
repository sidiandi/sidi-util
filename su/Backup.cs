using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.CommandLine;
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Sidi.Test;

namespace Sidi.Tool
{
    [Usage("Simple backup tool based on hard links")]
    public class Backup
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Backup()
        {
            VerifyFraction = 1000;
        }

        [Usage("Path to the backup store"), Persistent]
        public LPath Store
        {
            get
            {
                return store;
            }
            
            set
            {
                store = value;
            }
        }
        LPath store;
        
        LPath ConfigFile
        {
            get
            {
                return store.CatDir("backup.config");
            }
        }

        public LPath GetLastBackupSetPath()
        {
            var sets = 
                store.GetDirectories()
                .Where(x => Regex.IsMatch(x.FileName, @"^\d{8}\d{6}$"))
                .Select(x => x.FileName)
                .OrderBy(x => x)
                .ToList();

            if (!sets.Any())
            {
                return null;
            }

            return store.CatDir(sets.Last());
        }

        public string NewBackupSetName()
        {
            return BackupSetName(DateTime.Now.ToUniversalTime());
        }

        public string BackupSetName(DateTime d)
        {
            return d.ToString("yyyyMMddHHmmss");
        }

        FindConfig GetFileList()
        {
            var e = new Sidi.IO.FindConfig();
            new Parser(e).Parse(Tokenizer.FromFile(ConfigFile));
            return e;
        }

        [Usage("Create a new backup directory")]
        public void Create()
        {
            if (LDirectory.Exists(Store))
            {
                throw new Exception("{0} already exists".F(Store));
            }

            ConfigFile.EnsureParentDirectoryExists();

            using (var c = LFile.Open(ConfigFile, System.IO.FileMode.Create))
            {
                using (var w = new System.IO.StreamWriter(c))
                {
                    w.WriteLine(@"Include {0}
", System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Quote());

                }
            }
        }

        const string incompleteExtension = ".incomplete";
        const string deltaExtension = ".delta";

        [Usage("Create a new backup set")]
        public void Full()
        {
            CreateBackupSet(false);
        }
        
        void CreateBackupSet(bool delta)
        {
            var e = GetFileList();

            LPath backupSet;
            if (delta)
            {
                backupSet = store.CatDir(NewBackupSetName() + deltaExtension);
            }
            else
            {
                backupSet = store.CatDir(NewBackupSetName());
            }
            var incompleteBackupSet = backupSet.CatName(incompleteExtension);
            var lastBackupSet = GetLastBackupSetPath();

            var copied = new SizeCount();
            var linked = new SizeCount();
            var verified = new SizeCount();

            LDirectory.Create(incompleteBackupSet);

            var errors = new List<Exception>();
            
            foreach (var sourceItem in e.Enumerator.Depth())
            {
                var source = sourceItem.FullName;
                try
                {
                    var parts = source.Parts.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                    parts[0] = parts[0].Replace(":", String.Empty);
                    var bakPath = LPath.CreateRelative(parts);
                    var destination = incompleteBackupSet.CatDir(bakPath);

                    if (LDirectory.Exists(source))
                    {
                        log.Info(source);
                        if (!delta)
                        {
                            LDirectory.Create(destination);
                        }
                    }
                    else
                    {
                        if (lastBackupSet != null)
                        {
                            var existing = lastBackupSet.CatDir(bakPath);

                            if (LFile.EqualByTimeAndLength(source, existing))
                            {
                                if (delta)
                                {
                                    log.DebugFormat("Exists: {0}", existing);
                                    destination = existing;
                                }
                                else
                                {
                                    log.DebugFormat("Link {0} -> {1}", destination, existing);
                                    LFile.CreateHardLink(destination, existing);
                                    linked.Add(existing.Info.Length);
                                }
                                goto copied;
                            }
                        }

                        // not existing - do the copy
                        {
                            log.DebugFormat("Copy {0} -> {1}", source, destination);
                            if (delta)
                            {
                                destination.EnsureParentDirectoryExists();
                            }
                            LFile.Copy(source, destination);
                            copied.Add(destination.Info.Length);
                        }

                    copied:
                        Verify(source, destination, verified);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(source, ex);
                    errors.Add(ex);
                }
            }
            LDirectory.Move(incompleteBackupSet, backupSet.UniqueFileName());

            log.InfoFormat("Copied: {0}", copied);
            log.InfoFormat("Linked: {0}", linked);
            log.InfoFormat("Verified: {0}", verified);

            if (errors.Any())
            {
                log.ErrorFormat("{0} errors occured during backup:\r\n{1}",
                    errors.Count,
                    errors.Join());
            }
        }

        Random random = new Random();

        public double VerifyFraction
        {
            get
            {
                return verifyFraction;
            }

            set
            {
                verifyFraction = value;
                if (verifyFraction <= 1.0)
                {
                    verifyThreshold = 1.0;
                }
                else
                {
                    verifyThreshold = 1 / verifyFraction;
                }
            }
        }
        double verifyFraction;
        double verifyThreshold;
        
        void Verify(LPath source, LPath destination, SizeCount verified)
        {
            if (random.NextDouble() < verifyThreshold)
            {
                log.InfoFormat("Verify: {0} == {1}", source, destination);
                verified.Add(source.Info.Length);
                if (!LFile.EqualByContent(source, destination))
                {
                    throw new System.IO.IOException(String.Format("{0} and {1} are not equal", source, destination));
                }
            }
        }

        [Usage("Delete incomplete backup sets")]
        public void DeleteIncomplete()
        {
            var incompleteBackups = 
                store.Children
                .Where(c => c.EndsWith(incompleteExtension))
                .ToList();
            foreach (var i in incompleteBackups)
            {
                i.EnsureNotExists();
            }
        }

        [TestFixture]
        public class Test : TestBase
        {
            [Test]
            public void Run()
            {
                var root = TestFile("Backup-Root");
                root.EnsureNotExists();
                Assert.IsFalse(root.Exists);

                Backup b = new Backup();
                b.Store = root;
                b.Create();
                Assert.IsTrue(root.Exists);

                b.Store = root;

                using (var w = LFile.TextWriter(b.ConfigFile))
                {
                    w.WriteLine("Include {0}", Sidi.IO.Paths.BinDir);
                    w.WriteLine("Exclude test");
                }

                b.Full();
                b.Full();

                root.EnsureNotExists();
            }
        }
    }
}
