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
using Sidi.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Sidi.IMAP
{
    [CLSCompliant(false)]
    public class EmlMailbox : IMailbox
    {
        public EmlMailbox(LPath directory)
        {
            this.directory = directory;
            if (!directory.IsDirectory)
            {
                throw new System.IO.IOException(directory.ToString());
            }
            mails = directory.Info.GetFiles("*.eml");
        }
        LPath directory;
        IList<LFileSystemInfo> mails;

        public IList<IMail> Items
        {
            get { return new Sidi.Collections.SelectList<LFileSystemInfo, IMail>(mails, i => new EmlFile(i)); }
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

        public EmlFile(LFileSystemInfo i)
        {
            fileInfo = i;
            log.Info(i);
        }

        LFileSystemInfo fileInfo;

        Rfc822Message _Message;
        Rfc822Message Message
        {
            get
            {
                if (_Message == null)
                {
                    using (var r = LFile.Open(fileInfo.FullName, System.IO.FileMode.Open))
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
        public EmlDirectory(LPath directory)
        {
            this.directory = directory;
        }

        LPath directory;

        public IList<string> MailboxNames
        {
            get
            {
                var rootLength = directory.NoPrefix.Length;
                var e = new Sidi.IO.Find()
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
            return new EmlMailbox(directory.CatDir(name.Replace(Delimiter, Sidi.IO.LPath.DirectorySeparator)));
        }
    }
}
