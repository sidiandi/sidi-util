// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Text.RegularExpressions;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Imap
{
    [CLSCompliant(false)]
    public interface IRepository
    {
        IList<string> MailboxNames { get; }
        string Delimiter { get; }
        string Root { get; }
        IMailbox GetMailbox(string name);
    }

    [CLSCompliant(false)]
    public interface IMailbox
    {
        IList<IMail> Items { get; }
        /// <summary>
        /// Returns the message sequence number for a uid, or 0 if UID is not found
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        System.UInt32 GetByUid(System.UInt32 uid);
        System.UInt32 UidValidity { get; }
        System.UInt32 NextUid { get; }
    }

    public enum Flags
    {
        Seen,
        Answered,
        Flagged,
        Deleted,
        Draft,
        Recent
    };

    [CLSCompliant(false)]
    public interface IMail
    {
        string BODY { get; }
        string BODYSTRUCTURE { get; }
        string ENVELOPE { get; }
        Flags[] FLAGS { get; }
        Rfc822DateTime INTERNALDATE { get; }
        Rfc822Message RFC822 { get; }
        System.UInt32 UID { get; }
    }

    [CLSCompliant(false)]
    public static class IMailExtensions
    {
        public static IMail BySequenceNumber(this IMailbox mailbox, UInt32 sequenceNumber)
        {
            return mailbox.Items[(int)(sequenceNumber - 1)];
        }

        public static object[] RetrieveDataItem(this IMail mail, string dataItem)
        {
            try
            {
                var parser = new ArgParser(dataItem);

                if (parser.ConsumeIfNext("BODY"))
                {
                    if (parser.ConsumeIfNext(".PEEK"))
                    {
                    }
                    return HandleBody(mail, parser);
                }
                else
                {
                    object value = mail;
                    var key = new List<string>();
                    for (; ; )
                    {
                        var propertyName = parser.Get(new Regex(@"[\w]+")).ToUpper();
                        key.Add(propertyName);
                        var property = value.GetType().GetProperty(propertyName,
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.IgnoreCase);
                        value = property.GetValue(value, new object[] { });
                        if (!parser.ConsumeIfNext("."))
                        {
                            break;
                        }
                    }
                    return new object[] { new Session.AtomString(key.Join(".")), value };
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Cannot retrieve data item {0}", dataItem), e);
            }
        }

        static object[] HandleBody(IMail mail, ArgParser parser)
        {
            if (parser.IsEnd)
            {
                return new object[] { new Session.AtomString("BODY[]"), mail.BODY };
            }

            parser.Consume("[");
            object[] result = null;
            if (parser.IsNext("]"))
            {
                result = new object[] { new Session.AtomString("BODY[]"), mail.BODY };
            }
            else
            {
                var section = parser.GetAtom().ToUpper();
                switch (section)
                {
                    case "TEXT":
                        result = new object[] { new Session.AtomString("BODY[TEXT]"), mail.RFC822.Text };
                        break;
                    case "HEADER":
                        {
                            if (parser.ConsumeIfNext(".FIELDS"))
                            {
                                Func<string[], string> f = null;
                                if (parser.ConsumeIfNext(".NOT"))
                                {
                                    f = fn => mail.RFC822.HeaderFieldsNot(fn);
                                }
                                else
                                {
                                    f = fn => mail.RFC822.HeaderFields(fn);
                                }

                                parser.ArgSep();

                                var fieldNames = parser.GetList().Cast<string>().ToArray();

                                result = new object[] { new Session.AtomString("BODY[HEADER]"), f(fieldNames) };
                            }
                            else
                            {
                                result = new object[] { new Session.AtomString("BODY[HEADER]"), mail.RFC822.Header };
                            }
                        }
                        break;
                    case "MIME":
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }
            parser.Consume("]");
            if (parser.ConsumeIfNext("<"))
            {
                var begin = parser.GetNumber();
                parser.Consume(".");
                var end = parser.GetNumber();
                parser.Consume(">");

                result[1] = Partial(((string)result[1]), begin, end);
            }

            return result;
        }

        static string Partial(string x, int begin, int end)
        {
            if (begin >= x.Length)
            {
                return String.Empty;
            }
            if (end >= x.Length)
            {
                end = x.Length;
            }

            return x.Substring(begin, end - begin);
        }
    }
}
