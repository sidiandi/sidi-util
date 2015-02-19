using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Rule = System.Func<Sidi.Parse.Text>;

namespace Sidi.Parse
{
    [Serializable]
    public sealed class ParserException: Exception
    {
        public ParserException(Text text)
        {
            this.Text = text;
        }

        public Text Text { get; private set; }

        public override string ToString()
        {
            return Text.ToString();
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        // Serialization constructor is private, as this class is sealed
        private ParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Text = new Text(info.GetString("Text"));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Text", this.Text.AsString());
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Simple recursive descent parser.
    /// </summary>
    public class Parser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Func<Text, Ast> Consume(int length)
        {
            return (text) =>
            {
                if (length > text.Length)
                {
                    return null;
                }
                var t = text.Part(0, length);
                text.Set(text.Part(length));
                return new Ast(t);
            };
        }

        public static Func<Text, Ast> Concatenation(params Func<Text, Ast>[] rules)
        {
            return Rename(() => t =>
                {
                    var elements = new List<Ast>();

                    foreach (var i in rules)
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

                    return new Ast(null, elements);
                });
        }

        public static Func<Text, Ast> Enclose(Func<Func<Text, Ast>> rule, [CallerMemberName] string name = null)
        {
            return text =>
            {
                // log.InfoFormat("Try: {0}", name);
                var t = text.Copy();
                var n = rule()(t);
                if (n != null)
                {
                    var consumedText = text.Remove(t);
                    n.Text = consumedText;
                    var ast = new Ast(consumedText, new[] { n })
                    {
                        Name = name
                    };
                    text.Set(t);
                    // log.InfoFormat("Found: {0}", ast);
                    return ast;
                }
                return n;
            };
        }

        public static Func<Text, Ast> Rename(Func<Func<Text, Ast>> rule, [CallerMemberName] string name = null)
        {
            return text =>
                {
                    // log.InfoFormat("Try: {0}", name);
                    var t = text.Copy();
                    var n = rule()(t);
                    if (n != null)
                    {
                        var consumedText = text.Remove(t);
                        var ast = new Ast(n.Text, n.Childs)
                        {
                            Name = name
                        };
                        text.Set(t);
                        // log.InfoFormat("Found: {0}", ast);
                        return ast;
                    }
                    return n;
                };
        }

        public static Func<Text, Ast> Collapse(Func<Func<Text, Ast>> rule, [CallerMemberName] string name = null)
        {
            return text =>
            {
                // log.InfoFormat("Try: {0}", name);
                var t = text.Copy();
                var n = rule()(t);
                if (n != null)
                {
                    var consumedText = text.Remove(t);
                    n.Text = consumedText;
                    var ast = new Ast(consumedText)
                    {
                        Name = name
                    };
                    text.Set(t);
                    // log.InfoFormat("Found: {0}", ast);
                    return ast;
                }
                return n;
            };
        }

        public static Func<Text, Ast> Optional(Func<Text, Ast> rule)
        {
            return Repetition(0, 1, rule);
        }

        public static Func<Text, Ast> Repetition(int minimalCount, int maximalCount, Func<Text, Ast> rules)
        {
            return Rename(() => t =>
                {
                    var r = new List<Ast>();

                    int count = 0;
                    for (; count < maximalCount; ++count)
                    {
                        var e = rules(t);
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

                    return new Ast(null, r);
                });
        }

        public static Func<Text, Ast> Guard<T>(Func<Text, Ast> rule)
        {
            return text =>
            {
                var t = text.Copy();
                var p = rule(t);
                if (object.Equals(p, default(T)))
                {
                    text.Set(t);
                }
                return p;
            };
        }

        public static Func<Text, Ast> Alternative(params Func<Text, Ast>[] production)
        {
            return text =>
                {
                    var index = 0;
                    foreach (var i in production)
                    {
                        var t = text.Copy();
                        var m = i(t);
                        if (m != null)
                        {
                            text.Set(t);
                            return new Ast(m.Text, new[] { m })
                            {
                                Name = index
                            };
                        }
                        ++index;
                    }
                    return null;
                };
        }

        public static Func<Text, Ast> MandatoryAlternative(params Func<Text, Ast>[] production)
        {
            return text =>
            {
                var index = 0;
                foreach (var i in production)
                {
                    var t = text.Copy();
                    var m = i(t);
                    if (m != null)
                    {
                        text.Set(t);
                        return new Ast(m.Text, new[] { m })
                        {
                            Name = index
                        };
                    }
                    ++index;
                }
                throw new ParserException(text);
            };
        }

        public static Func<Text, Ast> Expect(string searchString)
        {
            return text =>
                {
                    if (text.Length < searchString.Length)
                    {
                        return null;
                    }

                    var t = text.Substring(0, searchString.Length);
                    if (t.Equals(searchString))
                    {
                        return Consume(searchString.Length)(text);
                    }
                    else
                    {
                        return null;
                    }
                };
        }

        public static Func<Text, Ast> Colon()
        {
            return Expect(":");
        }

        public static Func<Text, Ast> Digit()
        {
            return ExpectCharacter(c => (c >= '0' && c <= '9'));
        }

        public static Func<Text, Ast> ExpectCharacter(Func<char, bool> isValid)
        {
            return text =>
                {
                    if (text.Length < 1)
                    {
                        return null;
                    }

                    var c = text[0];

                    if (isValid(c))
                    {
                        return Consume(1)(text);
                    }

                    return null;
                };
        }

        public static Func<Text, Ast> Separator()
        {
            return Expect(@"\");
        }

        public static Func<Text, Ast> WhitespaceOrComment()
        {
            return Repetition(1, Int32.MaxValue, Alternative(WhitespaceCharacter(), CStyleComment(), CPlusPlusStyleComment()));
        }

        public static Func<Text, Ast> Whitespace()
        {
            return Repetition(1, Int32.MaxValue, WhitespaceCharacter());
        }

        public static Func<Text, Ast> CStyleComment()
        {
            return Collapse(() => Concatenation(Expect("/*"), TextUntil("*/")).Name(MethodBase.GetCurrentMethod()));
        }

        public static Func<Text, Ast> Slash()
        {
            return Collapse(() => Expect("/"));
        }

        public static Func<Text, Ast> Asterisk()
        {
            return Collapse(() => Expect("*"));
        }

        public static Func<Text, Ast> RestOfLine()
        {
            return Collapse(() => TextUntil("\r\n"));
        }

        public static Func<Text, Ast> CR()
        {
            return Collapse(() => Expect("\r"));
        }

        public static Func<Text, Ast> LF()
        {
            return Collapse(() => Expect("\n"));
        }

        public static Func<Text, Ast> NewLine()
        {
            return Collapse(() => Concatenation(Repetition(0, 1, CR()), LF()));
        }

        public static Func<Text, Ast> TextUntil(string terminal)
        {
            return text =>
                {
                    int terminalIndex = 0;
                    for (int i = 0; i < text.Length; ++i)
                    {
                        if (text[i] == terminal[terminalIndex])
                        {
                            ++terminalIndex;
                            if (terminalIndex >= terminal.Length)
                            {
                                ++i;
                                var r = text.Part(0, i);
                                text.Set(text.Part(i));
                                return new Ast(r);
                            }
                        }
                        else
                        {
                            terminalIndex = 0;
                        }
                    }
                    return null;
                };
        }

        public static Func<Text, Ast> CPlusPlusStyleComment()
        {
            return Collapse(() => Concatenation(Expect("//"), RestOfLine()));
        }

        public static Func<Text, Ast> OptionalWhitespace()
        {
            return Collapse(() => Repetition(0, Int32.MaxValue, WhitespaceCharacter()));
        }

        public static Func<Text, Ast> WhitespaceCharacter()
        {
            return ExpectCharacter(c =>
                {
                    switch (c)
                    {
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            return true;
                        default:
                            return false;
                    }
                });
        }

        public static bool IsMatch(string text, Func<Text, Ast> rule)
        {
            var t = new Text(text);
            var r = rule(t);
            return t.IsEmpty && !(r == null);
        }
    }

    public static class ParserExtensions
    {
        public static Func<Text, Ast> Name(this Func<Text, Ast> rule, object name)
        {
            return text =>
                {
                    var node = rule(text);
                    if (node != null)
                    {
                        node.Name = name;
                    }
                    return node;
                };
        }
    }
}
