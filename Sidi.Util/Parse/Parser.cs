using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rule = System.Func<Sidi.Parse.Text>;

namespace Sidi.Parse
{
    public class Tree
    {
        public Tree(IEnumerable<object> childs, Text text)
        {
            Childs = childs.ToArray();
            Text = text;
        }

        public object[] Childs;
        public Text Text;

        public override string ToString()
        {
            return Text.ToString();
        }

        public object this[int index]
        {
            get { return Childs[index]; }
        }

        public static implicit operator Text(Tree tree)
        {
            if (tree == null)
            {
                return null;
            }
            return tree.Text;
        }
    }

    public class Parser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Text Consume(Text text, int length)
        {
            if (length > text.Length)
            {
                return null;
            }
            var t = text.Part(0, length);
            text.Set(text.Part(length));
            return t;
        }

        public static Tree Concatenation(Text text, params Func<Text, object>[] productions)
        {
            var elements = new List<object>();

            var t = text.Copy();
            foreach (var i in productions)
            {
                var p = i(t);
                if (p == null)
                {
                    return null;
                }
                else
                {
                    elements.Add(p);
                }
            }

            var r = new Tree(elements, text.Remove(t));
            text.Set(t);
            return r;
        }

        public static Tree Optional(Text text, Func<Text, object> rule)
        {
            return Repetition(text, 0, 1, rule);
        }

        public static Tree Repetition(Text text, int minimalCount, int maximalCount, Func<Text, object> production)
        {
            var t = text.Copy();

            var r = new List<object>();

            int count = 0;
            for (; count < maximalCount; ++count)
            {
                var e = production(t);
                if (e == null)
                {
                    break;
                }
                else
                {
                    r.Add(e);
                }
            }

            if (count < minimalCount)
            {
                return null;
            }

            var tree = new Tree(r, text.Remove(t));
            text.Set(t);
            return tree;
        }

        public static Tree Alternative(Text text, params Func<Text, object>[] production)
        {
            foreach (var i in production)
            {
                var t = text.Copy();
                var m = i(t);
                if (m != null)
                {
                    var tree = new Tree(new[] { m }, text.Remove(t));
                    text.Set(t);
                    return tree;
                }
            }
            return null;
        }

        public static Text Expect(Text text, string searchString)
        {
            if (text.Length < searchString.Length)
            {
                return null;
            }

            var t = text.Substring(0, searchString.Length);
            if (t.Equals(searchString))
            {
                return Consume(text, searchString.Length);
            }
            else
            {
                return null;
            }
        }

        public static Text Colon(Text text)
        {
            return Expect(text, ":");
        }

        public static Text Digit(Text text)
        {
            if (text.Length < 1)
            {
                return null;
            }

            var c = text[0];
            if (c >= '0' && c <= '9')
            {
                return Consume(text, 1);
            }
            else
            {
                return null;
            }
        }

        public static Text Separator(Text text)
        {
            return Expect(text, @"\");
        }

        public static Tree WhitespaceOrComment(Text text)
        {
            return Repetition(text, 1, Int32.MaxValue, _ => Alternative(_, WhitespaceCharacter, CStyleComment, CPlusPlusStyleComment));
        }

        public static Tree Whitespace(Text text)
        {
            return Repetition(text, 1, Int32.MaxValue, WhitespaceCharacter);
        }

        public static Text CStyleComment(Text text)
        {
            return Concatenation(text, _ => Expect(_, "/*"), _=> TextUntil(_, "*/"));
        }

        public static Text Slash(Text text)
        {
            return Expect(text, "/");
        }

        public static Text Asterisk(Text text)
        {
            return Expect(text, "*");
        }

        public static Text RestOfLine(Text text)
        {
            return TextUntil(text, "\r\n");
        }

        public static Text CR(Text text)
        {
            return Expect(text, "\r");
        }

        public static Text LF(Text text)
        {
            return Expect(text, "\n");
        }

        public static Text NewLine(Text text)
        {
            return Concatenation(text, _ => Repetition(_ , 0, 1, CR), LF);
        }

        public static Text TextUntil(Text text, string terminal)
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
                        text.Set(text.Part(i));
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

        public static Text CPlusPlusStyleComment(Text text)
        {
            return Concatenation(text, _ => Expect(_, "//"), _ => RestOfLine(_));
        }

        public static Text OptionalWhitespace(Text text)
        {
            return Repetition(text, 0, Int32.MaxValue, WhitespaceCharacter);
        }

        public static Text WhitespaceCharacter(Text text)
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
                    return Consume(text, 1);
                    break;
            }

            return null;
        }

        public static bool IsMatch<T>(string text, Func<Text, T> rule)
        {
            var t = new Text(text);
            var r = rule(t);
            return t.IsEmpty && !object.Equals(r, default(T));
        }
    }

}
