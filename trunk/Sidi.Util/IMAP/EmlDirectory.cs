using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sidi.IO.Long;
using System.Text.RegularExpressions;
using System.Globalization;
using Sidi.IO.Long.Extensions;

namespace Sidi.IMAP
{
    [CLSCompliant(false)]
    public class EmlMailbox : IMailbox
    {
        public EmlMailbox(Path directory)
        {
            this.directory = directory;
            if (!Directory.Exists(directory))
            {
                throw new System.IO.IOException(directory.ToString());
            }
            mails = Directory.GetChilds(directory)
                .Where(x => !x.IsDirectory && x.Name.EndsWith(".eml", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }
        Path directory;
        IList<FileSystemInfo> mails;

        public IList<IMail> Items
        {
            get { return new Sidi.Collections.SelectList<FileSystemInfo, IMail>(mails, i => new EmlFile(i)); }
        }

        public System.UInt32 GetByUid(System.UInt32 uid)
        {
            var uidString = uid.ToString();
            for (int i = 0; i < mails.Count; ++i)
            {
                if (mails[i].Name.Contains(uidString))
                {
                    return (System.UInt32)(i + 1);
                }
            }
            return 0;
        }

        public uint UidValidity { get { return (System.UInt32)directory.GetHashCode(); } }
        public uint NextUid { get { return 10000; } }
    }

    [CLSCompliant(false)]
    public class EmlFile : IMail
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EmlFile(FileSystemInfo i)
        {
            fileInfo = i;
            log.Info(i);
        }

        FileSystemInfo fileInfo;

        Rfc822Message _Message;
        Rfc822Message Message
        {
            get
            {
                if (_Message == null)
                {
                    using (var r = File.Open(fileInfo.FullName, System.IO.FileMode.Open))
                    {
                        _Message = Rfc822Message.Parse(new System.IO.StreamReader(r));
                    }
                }
                return _Message;
            }
        }

        public System.UInt32 UID { get { return System.UInt32.Parse(Regex.Match(fileInfo.Name, @"\d\d\d\d").Value); } }

        public string BODY
        {
            get { return Message.Text; }
        }

        public string BODYSTRUCTURE
        {
            get { return Message.Text; // todo
            }
        }

        public string ENVELOPE
        {
            get { throw new NotSupportedException(); }
        }

        public Flags[] FLAGS
        {
            get { return new Flags[] { Flags.Seen }; }
        }

        public Rfc822DateTime INTERNALDATE
        {
            get
            {
                return Message.Date; 
            }
        }

        public Rfc822Message RFC822
        {
            get { return Message; }
        }
    }

    [CLSCompliant(false)]
    public class EmlDirectory : IRepository
    {
        public EmlDirectory(Path directory)
        {
            this.directory = directory;
        }

        Path directory;

        public IList<string> MailboxNames
        {
            get
            {
                var rootLength = directory.NoPrefix.Length;
                var e = new Sidi.IO.Long.FileEnumerator()
                {
                    Root = directory,
                    Output = x => x.IsDirectory,
                };
                return e.Depth()
                    .Select(x => x.FullName.NoPrefix.Substring(rootLength).Replace(@"\", Delimiter))
                    .Where(x => !String.IsNullOrEmpty(x))
                    .ToList();
            }
        }

        public string Delimiter
        {
            get { return "/"; }
        }

        public string Root
        {
            get { return Delimiter; }
        }

        public IMailbox GetMailbox(string name)
        {
            return new EmlMailbox(directory.CatDir(name.Replace(Delimiter, Sidi.IO.Long.Path.DirectorySeparator)));
        }
    }
}
