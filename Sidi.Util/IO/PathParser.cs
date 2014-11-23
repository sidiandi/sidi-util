using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public class Text
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Text(string text)
        {
            this.text = text;
            this.begin = 0;
            this.end = text.Length;
        }

        public Text(string text, int begin, int end)
        {
            this.text = text;
            this.begin = begin;
            this.end = end;
        }

        string text;
        int begin;
        int end;

        public override string ToString()
        {
            return text.Substring(begin, end - begin);
        }

        public Text Part(int begin)
        {
            return new Text(this.text, this.begin + begin, this.end);
        }

        public Text Remove(Text subText)
        {
            return new Text(text, begin, subText.begin);
        }

        public string Substring(int begin, int length)
        {
            return text.Substring(this.begin + begin, length);
        }

        public Text Part(int begin, int end)
        {
            return new Text(this.text, this.begin + begin, this.begin + end);
        }

        public Text Copy()
        {
            return new Text(text, begin, end);
        }

        public int Length
        {
            get { return end - begin; }
        }

        public char this[int index]
        {
            get
            {
                return text[begin+index];
            }
        }

        public void Set(Text r)
        {
            this.text = r.text;
            this.begin = r.begin;
            this.end = r.end;
        }
    }

    class Parser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate Text Rule(ref Text text);

        public Text Consume(ref Text text, int length)
        {
            var t = text.Part(0, length);
            text = text.Part(length);
            return t;
        }

        public Text Concatenation(ref Text text, params Rule[] productions)
        {
            var t = text.Copy();
            foreach (var i in productions)
            {
                var p = i(ref t);
                if (p == null)
                {
                    return null;
                }
            }
            var orig = text;
            text = t;
            return orig.Remove(text);
        }

        public Text Repetition(ref Text text, int minimalCount, int maximalCount, Rule production)
        {
            var t = text.Copy();

            var r = new List<Text>();

            int count = 0;
            for (; count < maximalCount; ++count)
            {
                var e = production(ref t);
                if (e == null)
                {
                    break;
                }
            }

            if (count < minimalCount)
            {
                return null;
            }

            var orig = text;
            text = t;
            return orig.Remove(text);
        }

        public Text Alternative(ref Text text, params Rule[] production)
        {
            foreach (var i in production)
            {
                var t = text.Copy();
                var m = i(ref t);
                if (m != null)
                {
                    text = t;
                    return m;
                }
            }
            return null;
        }

        public Text Expect(ref Text text, string searchString)
        {
            if (text.Length < searchString.Length)
            {
                return null;
            }

            var t = text.Substring(0, searchString.Length);
            if (t.Equals(searchString))
            {
                return Consume(ref text, searchString.Length);
            }
            else
            {
                return null;
            }
        }

        public Text Colon(ref Text text)
        {
            return Expect(ref text, ":");
        }

        public Text Separator(ref Text text)
        {
            return Expect(ref text, @"\");
        }

        public Text WhitespaceOrComment(ref Text text)
        {
            return Repetition(ref text, 1, Int32.MaxValue, (ref Text t) => Alternative(ref t, WhitespaceCharacter, CStyleComment, CPlusPlusStyleComment));
        }

        public Text Whitespace(ref Text text)
        {
            return Repetition(ref text, 1, Int32.MaxValue, WhitespaceCharacter);
        }

        public Text CStyleComment(ref Text text)
        {
            return Concatenation(ref text, (ref Text _) => Expect(ref _, "/*"), (ref Text _) => TextUntil(ref _, "*/"));
        }

        public Text Slash(ref Text text)
        {
            return Expect(ref text, "/");
        }

        public Text Asterisk(ref Text text)
        {
            return Expect(ref text, "*");
        }

        public Text RestOfLine(ref Text text)
        {
            return TextUntil(ref text, "\r\n");
        }

        public Text CR(ref Text text)
        {
            return Expect(ref text, "\r");
        }

        public Text LF(ref Text text)
        {
            return Expect(ref text, "\n");
        }

        public Text NewLine(ref Text text)
        {
            return Concatenation(ref text, (ref Text _) => Repetition(ref _, 0, 1, CR), LF);
        }

        public Text TextUntil(ref Text text, string terminal)
        {
            int terminalIndex = 0;
            for (int i = 0; i< text.Length;++i)
            {
                if (text[i] == terminal[terminalIndex])
                {
                    ++terminalIndex;
                    if (terminalIndex >= terminal.Length)
                    {
                        ++i;
                        var r = text.Part(0, i);
                        text = text.Part(i);
                        return r;
                    }
                }
                else
                {
                    terminalIndex = 0;
                }
            }
            return null;
        }

        public Text CPlusPlusStyleComment(ref Text text)
        {
            return Concatenation(ref text, (ref Text _) => Expect(ref _, "//"), (ref Text _) => RestOfLine(ref _));
        }

        public Text OptionalWhitespace(ref Text text)
        {
            return Repetition(ref text, 0, Int32.MaxValue, WhitespaceCharacter);
        }

        public Text WhitespaceCharacter(ref Text text)
        {
            if (text.Length < 1)
            {
                return null;
            }

            var c = text[0];

            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return Consume(ref text, 1);
                    break;
            }

            return null;
        }
    }

    class PathParser : Parser
    {
        public LPath Path(ref Text text)
        {
            var t = text.Copy();
            var prefix = Prefix(ref t);
            if (prefix == null) return null;
            var names = Names(ref t);
            if (names == null) return null;
            text = t;
            return new LPath(prefix, names);
        }

        public string Prefix(ref Text text)
        {
            var a = Alternative(ref text, LongPrefix, LongUncPrefix, DeviceNamespacePrefix, Unc, Drive, RootRelative, EmptyPrefix);
            return a == null ? null : a.ToString();
        }

        public Text EmptyPrefix(ref Text text)
        {
            return Expect(ref text, String.Empty);
        }

        public Text LongPrefix(ref Text text)
        {
            return Expect(ref text, @"\\?\");
        }

        public Text LongUncPrefix(ref Text text)
        {
            var t = text.Copy();
            if (null == Expect(ref t, @"\\?\UNC\"))
            {
                return null;
            }

            if (null == Concatenation(ref text, ServerName, Separator, ShareName, Separator))
            {
                return null;
            }

            var orig = text;
            text = t;
            return orig.Remove(text);
        }
    
        public Text DeviceNamespacePrefix(ref Text text)
        {
            return Expect(ref text, @"\\.\");
        }
        
        public string[] Names(ref Text text)
        {
            var names = new List<string>();
            var t = text.Copy();
            
            for (;;)
            {
                var n = NtfsFilename(ref t);
                if (n == null)
                {
                    break;
                }
                else
                {
                    names.Add(n.ToString());
                }
                if (Separator(ref t) == null)
                {
                    break;
                }
            }
            text = t;
            return names.ToArray();
        }

        Text NtfsFilename(ref Text text)
        {
            return Repetition(ref text, 1, Int32.MaxValue, NtfsAllowedCharacter);
        }

        Text NtfsAllowedCharacter(ref Text text)
        {
            if (text.Length < 1)
            {
                return null;
            }

            var c = text[0];
            /*

Use any character in the current code page for a name, including Unicode characters and characters in the extended character set (128–255), except for the following:

    The following reserved characters:
        < (less than)
        > (greater than)
        : (colon)
        " (double quote)
        / (forward slash)
        \ (backslash)
        | (vertical bar or pipe)
        ? (question mark)
        * (asterisk)
    Integer value zero, sometimes referred to as the ASCII NUL character.
    Characters whose integer representations are in the range from 1 through 31, except for alternate data streams where these characters are allowed. For more information about file streams, see File Streams.
    Any other character that the target file system does not allow.
             */

            switch (c)
            {
                case '\0' :
                case '<':
                case '>':
                case ':':
                case '"':
                case '/':
                case '\\':
                case '|':
                case '?':
                    return null;
            }

            return Consume(ref text, 1);
        }

        public Text Unc(ref Text text)
        {
            return Concatenation(ref text, Separator, Separator, ServerName, Separator, ShareName, Separator);
        }

        public Text ServerName(ref Text text)
        {
            return NtfsFilename(ref text);
        }

        public Text ShareName(ref Text text)
        {
            return NtfsFilename(ref text);
        }

        public Text RootRelative(ref Text text)
        {
            return Separator(ref text);
        }

        public Text Drive(ref Text text)
        {
            return Concatenation(ref text, DriveLetter, Colon, Separator);
        }

        Text DriveLetter(ref Text text)
        {
            if (text.Length < 1)
            {
                return null;
            }

            var c = text[0];
            if (!((c>='a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
            {
                return null;
            }

            return Consume(ref text, 1);
        }
    }
}
