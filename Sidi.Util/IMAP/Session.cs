using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Sidi.Util;
using Sidi.Extensions;
using System.Globalization;

namespace Sidi.IMAP
{
    [CLSCompliant(false)]
    public class Session : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        enum State
        {
            NotAuthenticated,
            Authenticated,
            Selected,
            Logout,
        }

        enum ResponseCode
        {
            ALERT,
            BADCHARSET,
            CAPABILITY,
            PARSE,
            PERMANENTFLAGS,
            READ_ONLY,
            READ_WRITE,
            TRYCREATE,
            UIDNEXT,
            UIDVALIDITY,
            UNSEEN,
        };

        TcpClient connection;
        NetworkStream stream;
        StreamWriter Out;
        StreamReader In;

        State state = State.NotAuthenticated;

        public Session(TcpClient tcpClient)
        {
            connection = tcpClient;
            connection = tcpClient;
            stream = tcpClient.GetStream();
            Out = new StreamWriter(stream, System.Text.ASCIIEncoding.ASCII);
            Out.AutoFlush = true;
            In = new StreamReader(stream);
        }

        void CAPABILITY()
        {
            Respond(MethodBase.GetCurrentMethod(), null, null, "IMAP4rev1");
        }

        void LOGIN(string user, string password)
        {
        }

        void NOOP()
        {
        }

        void STARTTLS()
        {
            throw new NotImplementedException();
        }

        void AUTHENTICATE(string authenticationMechanismName)
        {
            throw new NotImplementedException();
        }

        void LOGOUT()
        {
            BYE(null, null, "terminating");
            state = State.Logout;
        }

        IMailbox currentMailbox;

        static IEnumerable<object> PList(params object[] p)
        {
            return p;
        }

        object[] SELECT(string mailboxName)
        {
            currentMailbox = repository.GetMailbox(mailboxName);
            Respond(null, "FLAGS", PList(Flags.Answered, Flags.Flagged, Flags.Deleted, Flags.Seen, Flags.Draft));
            Respond(null, "{0} EXISTS".F(currentMailbox.Items.Count));
            Respond(null, "{0} RECENT".F(currentMailbox.Items.Count));
            OK(null, RC(ResponseCode.UNSEEN, currentMailbox.Items.Count), String.Empty);
            OK(null, RC(ResponseCode.PERMANENTFLAGS, PList()), String.Empty);
            OK(null, RC(ResponseCode.UIDNEXT, currentMailbox.NextUid), String.Empty);
            OK(null, RC(ResponseCode.UIDVALIDITY, currentMailbox.UidValidity), String.Empty);
            return RC(ResponseCode.READ_ONLY);
        }

        static object[] RC(params object[] p)
        {
            return p;
        }

        void EXAMINE(string mailboxName)
        {
            throw new NotImplementedException();
        }

        void DELETE(string mailboxName)
        {
            throw new NotImplementedException();
        }

        void RENAME(string existingMailboxName, string newMailboxName)
        {
            throw new NotImplementedException();
        }

        void SUBSCRIBE(string mailboxName)
        {
            throw new NotImplementedException();
        }

        void UNSUBSCRIBE(string mailboxName)
        {
            throw new NotImplementedException();
        }

        void LIST(string referenceName, string mailboxNameWithPossibleWildcards)
        {
            var regex = MakeMailboxNameRegex(mailboxNameWithPossibleWildcards);

            if (String.IsNullOrEmpty(mailboxNameWithPossibleWildcards))
            {
                Respond(null, MethodBase.GetCurrentMethod().Name, new Flags[] { }, Quoted(repository.Delimiter), Quoted(repository.Root));
            }
            else
            {
                foreach (var i in repository.MailboxNames.Where(x => regex.IsMatch(x)))
                {
                    Respond(null, MethodBase.GetCurrentMethod().Name, new object[] { }, Quoted(repository.Delimiter), Quoted(i));
                }
            }
        }

        void LIST_Response(string tag, IEnumerable<object> nameAttributes, string hierarchyDelimiter, string name)
        {
            Respond(tag, "LIST", nameAttributes, hierarchyDelimiter, name);
        }

        void LSUB(string referenceName, string mailboxNameWithPossibleWildcards)
        {
            var regex = MakeMailboxNameRegex(mailboxNameWithPossibleWildcards);

            if (String.IsNullOrEmpty(mailboxNameWithPossibleWildcards))
            {
                Respond(null, MethodBase.GetCurrentMethod().Name, new object[] { @"\NoSelect" },
                    Quoted(repository.Delimiter), Quoted(repository.Root));
            }
            else
            {
                foreach (var i in repository.MailboxNames.Where(x => regex.IsMatch(x)))
                {
                    // debug
                    Respond(null, MethodBase.GetCurrentMethod().Name, new object[] { },
                        Quoted(repository.Delimiter),
                        Quoted(i));
                }
            }
        }

        void STATUS(string mailboxName, NestedList statusDataItemNames)
        {
            throw new NotImplementedException();
        }

        void APPEND(string mailboxName, NestedList flagList, DateTime time, string messageLiteral)
        {
            throw new NotImplementedException();
        }

        const string noTag = "*";
        const string argSep = " ";

        void Respond(string tag, string command, params object[] args)
        {
            var w = new StringWriter();
            w.Write(tag == null ? noTag : tag);
            w.Write(argSep);
            w.Write(command);
            foreach (var i in args)
            {
                w.Write(argSep);
                w.Write(ArgString(i));
            }

            log.InfoFormat("S: {0}", w.ToString());
            Out.WriteLine(w.ToString());
        }

        static readonly char[] quoteSpecials = new char[] { '\\', '/', '"' };

        public static string ArgString(object x)
        {
            if (x is String)
            {
                var s = (String)x;
                if (s.Length > 100 || s.IndexOfAny(quoteSpecials) >= 0)
                {
                    return ArgString(new LiteralString(s));
                }
                else
                {
                    // quoted string
                    return new QuotedString(s).Encode();
                }
            }
            else if (x is QuotedString)
            {
                return ((QuotedString)x).Encode();
            }
            else if (x is LiteralString)
            {
                return ((LiteralString)x).Encode();
            }
            else if (x is AtomString)
            {
                return ((AtomString)x).Encode();
            }
            else if (x is Flags)
            {
                return @"\" + x.ToString();
            }
            else if (x is Rfc822DateTime)
            {
                var d = (Rfc822DateTime)x;
                var t = d.Date;
                return ArgString(String.Format(
                    "{0:D2}-{1}-{2:D4} {3:D2}:{4:D2}:{5:D2} {6}{7:D2}{8:D2}",
                    t.Day,
                    Rfc822DateTime.months[t.Month-1],
                    t.Year,
                    t.Hour,
                    t.Minute,
                    t.Second,
                    d.TimeZone.TotalSeconds < 0 ? "-" : "+",
                    Math.Abs(d.TimeZone.Hours),
                    Math.Abs(d.TimeZone.Minutes)));
            }
            else if (x.GetType().IsEnum)
            {
                return x.ToString().Replace("_", "-");
            }
            else if (x is System.Collections.IEnumerable)
            {
                var e = ((System.Collections.IEnumerable)x).Cast<object>();
                return "(" + e.Select(i => ArgString(i)).Join(argSep) + ")";
            }
            else if (x is System.Int32 || x is System.UInt32)
            {
                return x.ToString();
            }
            else
            {
                throw new NotImplementedException(x.GetType().ToString());
            }
        }

        IRepository repository = new EmlDirectory(@"C:\temp\eml");

        Regex MakeMailboxNameRegex(string nameWithWildcards)
        {
            var p = Regex.Escape(nameWithWildcards);
            p = Regex.Replace(nameWithWildcards, @"\*", ".*");
            p = Regex.Replace(p, @"\%", "[^" + repository.Delimiter + "]*");
            return new Regex("^{0}$".F(p), RegexOptions.IgnoreCase);
        }

        class StateRestrictionAttribute : Attribute
        {
            public StateRestrictionAttribute(State state)
            {
                this.state = state;
            }

            State state;
        }

        [StateRestriction(State.Selected)]
        void CHECK()
        {
            throw new NotImplementedException();
        }

        [StateRestriction(State.Selected)]
        void CLOSE()
        {
            state = State.Authenticated;
        }

        [StateRestriction(State.Selected)]
        void EXPUNGE()
        {
            throw new NotImplementedException();
        }

        [StateRestriction(State.Selected)]
        void SEARCH(string charsetSpec, string searchCriteria)
        {
            throw new NotImplementedException();
        }

        IEnumerable<KeyValuePair<UInt32, IMail>> EnumerateMails(string sequenceSet)
        {
            switch (Mode)
            {
                case IdMode.Index:
                    return EnumerateSequenceSet(sequenceSet, (UInt32)currentMailbox.Items.Count)
                        .Select(id => new KeyValuePair<UInt32, IMail>(id, currentMailbox.Items[(int)(id - 1)]));
                case IdMode.UID:
                    return EnumerateSequenceSet(sequenceSet, (UInt32)currentMailbox.Items.Count)
                        .Select(id => currentMailbox.GetByUid(id))
                        .Where(s => s > 0)
                        .Select(sequenceNumber =>
                            {
                                return new KeyValuePair<UInt32, IMail>(sequenceNumber, currentMailbox.BySequenceNumber(sequenceNumber));
                            });
                default:
                    throw new InvalidDataException("unknown mode");
            }
        }

        [StateRestriction(State.Selected)]
        void FETCH(string sequenceSet, ArgParser parser)
        {
            var fetchAtt = new List < Func<IMail, object[]>>();
            parser.Consume("(");

            /*
            fetch-att       = "ENVELOPE" / "FLAGS" / "INTERNALDATE" /
                  "RFC822" [".HEADER" / ".SIZE" / ".TEXT"] /
                  "BODY" ["STRUCTURE"] / "UID" /
                  "BODY" section ["<" number "." nz-number ">"] /
                  "BODY.PEEK" section ["<" number "." nz-number ">"]
             */
            for (; !parser.IsNext(")"); parser.SkipSpace())
            {
                var sectionSpec = parser.Get(new Regex("[A-Z0-9]+", RegexOptions.IgnoreCase)).ToUpper();
                log.Info(sectionSpec);
                switch (sectionSpec)
                {
                    case "ENVELOPE":
                        throw new NotImplementedException();
                    case "FLAGS":
                        fetchAtt.Add(mail =>
                        {
                            return new object[] { A("FLAGS"), mail.FLAGS };
                        });
                        break;
                    case "INTERNALDATE":
                        fetchAtt.Add(mail =>
                        {
                            return new object[] { A("INTERNALDATE"), mail.INTERNALDATE };
                        });
                        break;
                    case "UID":
                        fetchAtt.Add(mail =>
                        {
                            return new object[] { A("UID"), mail.UID };
                        });
                        break;
                    case "RFC822":
                        if (parser.ConsumeIfNext(".HEADER"))
                        {
                            fetchAtt.Add(mail => new object[] { A("RFC822.HEADER"), mail.RFC822.Header });
                        }
                        else if (parser.ConsumeIfNext(".SIZE"))
                        {
                            fetchAtt.Add(mail => new object[] { A("RFC822.SIZE"), mail.RFC822.Size });
                        }
                        else if (parser.ConsumeIfNext(".TEXT"))
                        {
                            fetchAtt.Add(mail => new object[] { A("RFC822.TEXT"), mail.RFC822.Text });
                        }
                        else
                        {
                            fetchAtt.Add(mail => new object[] { A("RFC822"), mail.RFC822.Text });
                        }
                        break;
                    case "BODY":
                        {
                            if (parser.ConsumeIfNext(".PEEK"))
                            {
                            }
                            if (parser.ConsumeIfNext("["))
                            {
                                // section
                                if (parser.IsNext("]"))
                                {
                                    fetchAtt.Add(mail => new object[] { A("BODY[]"), mail.BODY });
                                }
                                else
                                {
                                    // section-spec
                                    if (parser.ConsumeIfNext("HEADER.FIELDS"))
                                    {
                                        if (parser.ConsumeIfNext(".NOT"))
                                        {
                                            var headerList = parser.GetList().Cast<string>().ToArray();
                                            fetchAtt.Add(mail => new object[] { A("BODY[HEADER]"), mail.RFC822.HeaderFieldsNot(headerList) });
                                        }
                                        else
                                        {
                                            var headerList = parser.GetList().Cast<string>().ToArray();
                                            fetchAtt.Add(mail => new object[] { A("BODY[HEADER]"), mail.RFC822.HeaderFields(headerList) });
                                        }
                                    }
                                    else if (parser.ConsumeIfNext("HEADER"))
                                    {
                                        fetchAtt.Add(mail => new object[] { A("BODY[HEADER]"), mail.RFC822.Header });
                                    }
                                    else if (parser.ConsumeIfNext("TEXT"))
                                    {
                                        fetchAtt.Add(mail => new object[] { A("BODY[TEXT]"), mail.RFC822.Text });
                                    }
                                }
                                parser.Consume("]");
                            }
                            else
                            {
                                fetchAtt.Add(mail => new object[] { A("BODY"), mail.BODY });
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            parser.Consume(")");
            
            foreach (var i in EnumerateMails(sequenceSet))
            {
                try
                {
                    var result = fetchAtt.SelectMany(f => f(i.Value)).ToList();
                    Respond(null, String.Format("{0} {1}", i.Key, MethodBase.GetCurrentMethod().Name), result);
                }
                catch (Exception e)
                {
                    log.Warn(i, e);
                }
            }
        }

        static AtomString A(string atomText)
        {
            return new AtomString(atomText);
        }

        public class AtomString
        {
            public AtomString(string text)
            {
                this.text = text;
            }
            string text;

            public string Encode()
            {
                if (text == null)
                {
                    return "NIL";
                }
                else
                {
                    return text;
                }
            }

            public override string ToString()
            {
                return text;
            }

            public override int GetHashCode()
            {
                return text.GetHashCode();
            }
        }

        static QuotedString Quoted(string x)
        {
            return new QuotedString(x);
        }

        public class QuotedString
        {
            public QuotedString(string x)
            {
                this.value = x;
            }
            string value;

            public string Encode()
            {
                return "\"" + value + "\"";
            }
        }

        public class LiteralString
        {
            public LiteralString(string value)
            {
                this.value = value;
            }

            string value;

            public string Encode()
            {
                var w = new StringWriter();
                w.WriteLine("{" + value.Length.ToString() + "}");
                w.Write(value);
                return w.ToString();
            }
        }

        [StateRestriction(State.Selected)]
        void STORE(string sequenceset, string messagedataitemname, string valueformessagedataitem)
        {
            throw new NotImplementedException();
        }

        class NestedList
        {
        }

        enum IdMode
        {
            Index,
            UID,
        }

        IdMode Mode;

        public void Run()
        {
            OK(null, null, "IMAP4rev1 server ready");
            state = State.Authenticated;

            try
            {
                while (state != State.Logout)
                {
                    var c = In.ReadLine();
                    if (c == null)
                    {
                        break;
                    }

                    log.InfoFormat("C: {0}", c);

                    var parser = new ArgParser(c, In, Out);
                    var tag = parser.GetAtom();
                    parser.ArgSep();
                    var command = parser.GetAtom().ToUpper();
                    if (command == "UID")
                    {
                        Mode = IdMode.UID;
                        parser.ArgSep();
                        command = parser.GetAtom().ToUpper();
                    }
                    else
                    {
                        Mode = IdMode.Index;
                    }

                    try
                    {
                        var m = GetType().GetMethod(command, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (m == null)
                        {
                            throw new ArgumentOutOfRangeException("Unknown command {0}".F(command));
                        }
                        var arguments = m.GetParameters().Select(p => { parser.ArgSep(); return parser.GetArgument(p); }).ToArray();
                        var responseCode = m.Invoke(this, arguments) as object[];
                        OK(tag, responseCode, m.Name + " completed");
                    }
                    catch (Exception e)
                    {
                        var m = e is TargetInvocationException ? e.InnerException.Message : e.Message;
                        log.Error(c, e);
                        BAD(tag, null, m);
                    }
                }
            }
            catch
            {
                log.WarnFormat("connection {0} terminated with exception", connection.Client.RemoteEndPoint);
            }
            connection.Close();
        }

        static UInt32 GetSequenceNumber(ArgParser p, UInt32 largestNumber)
        {
            if (p.ConsumeIfNext("*"))
            {
                return largestNumber;
            }
            else
            {
                return p.GetUInt32();
            }
        }

        public static IEnumerable<UInt32> EnumerateSequenceSet(string sequenceSet, UInt32 largestNumber)
        {
            for (var a = new ArgParser(sequenceSet, null, null); !a.IsEnd; a.Consume(","))
            {
                UInt32 begin = GetSequenceNumber(a, largestNumber);
                UInt32 end = begin;
                if (a.ConsumeIfNext(":"))
                {
                    end = GetSequenceNumber(a, largestNumber);
                }

                if (end < begin)
                {
                    var temp = end;
                    end = begin;
                    begin = temp;
                }

                for (var i = begin; i <= end; ++i)
                {
                    yield return i;
                }

                if (a.IsEnd)
                {
                    break;
                }
            }
        }

        void OK(string tag, object[] responseCode, string message) { Respond(MethodBase.GetCurrentMethod(), tag, responseCode, message); }
        void NO(string tag, object[] responseCode, string message) { Respond(MethodBase.GetCurrentMethod(), tag, responseCode, message); }
        void BAD(string tag, object[] responseCode, string message) { Respond(MethodBase.GetCurrentMethod(), tag, responseCode, message); }
        void PREAUTH(string tag, object[] responseCode, string message) { Respond(MethodBase.GetCurrentMethod(), tag, responseCode, message); }
        void BYE(string tag, object[] responseCode, string message) { Respond(MethodBase.GetCurrentMethod(), tag, responseCode, message); }

        void Respond(MethodBase response, string tag, object[] responseCode, string message)
        {
            var r = String.Format("{0} {1}{2}{3}",
                tag == null ? "*" : tag,
                response.Name,
                responseCode == null ? String.Empty : " [" + responseCode.Select(x => ArgString(x)).Join(argSep) + "]",
                " " + message);
            log.InfoFormat("S: {0}", r);
            Out.WriteLine(r);
        }

        private bool disposed = false;
            
        //Implement IDisposable.
        public void Dispose()
        {
          Dispose(true);
          GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
          if (!disposed)
          {
            if (disposing)
            {
                this.stream.Dispose();
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            disposed = true;
          }
        }

        // Use C# destructor syntax for finalization code.
        ~Session()
        {
          // Simply call Dispose(false).
          Dispose(false);
        }    
    
    }
}
