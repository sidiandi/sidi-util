using Sidi.CommandLine;
using Sidi.IO;
using System;
using System.IO;

namespace Sidi.np
{

    [Usage("Show text received on stdin in Notepad++")]
    class np : IArgumentHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static object Process { get; private set; }

        public static int Main(string[] args)
        {
            return GetOpt.Run(new np(), args);
        }

        [Usage("Write received input to stdout.")]
        public bool Passthrough { get; set; }

        int OpenStdinInNotepad()
        {
            if (!Console.IsInputRedirected)
            {
                OpenNotepad();
                return 0;
            }

            var textFile = Paths.GetLocalApplicationDataDirectory(this.GetType())
                .CatDir(LPath.GetValidFilename(DateTime.UtcNow.ToString("o")) + ".txt");

            log.InfoFormat("write to file {0}", textFile);

            using (var w = textFile.WriteText())
            {
                if (Passthrough)
                {
                    Copy(Console.In, Console.Out, w);
                }
                else
                {
                    Copy(Console.In, w);
                }
            }

            OpenInNotepad(textFile);

            return 0;
        }

        static LPath GetNotepadPlusPlusExe()
        {
            return Paths.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                .CatDir(@"Notepad++\notepad++.exe");
        }

        static LPath GetNotepadExe()
        {
            return Paths.GetFolderPath(Environment.SpecialFolder.System).CatDir(@"notepad.exe");
        }

        static void OpenInNotepad(LPath textFile)
        {
            var notepadExe = GetNotepadPlusPlusExe();
            if (notepadExe.IsFile)
            {
                System.Diagnostics.Process.Start(notepadExe, String.Format("-ro {0}", textFile.Quote()));
                return;
            }

            notepadExe = GetNotepadExe();
            if (notepadExe.IsFile)
            {
                System.Diagnostics.Process.Start(notepadExe, String.Format("{0}", textFile.Quote()));
                return;
            }
        }

        static void OpenNotepad()
        {
            var notepadExe = GetNotepadPlusPlusExe();
            if (notepadExe.IsFile)
            {
                System.Diagnostics.Process.Start(notepadExe);
                return;
            }

            notepadExe = GetNotepadExe();
            if (notepadExe.IsFile)
            {
                System.Diagnostics.Process.Start(notepadExe);
                return;
            }
        }

        static void Copy(TextReader r, params TextWriter[] w)
        {
            for (; ; )
            {
                var line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                foreach (var i in w)
                {
                    i.WriteLine(line);
                }
            }
        }

        static void Copy(TextReader r, TextWriter w)
        {
            for (; ; )
            {
                var line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                w.WriteLine(line);
            }
        }

        public void ProcessArguments(string[] args)
        {
            OpenStdinInNotepad();
        }
    }
}