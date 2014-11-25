using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Parse;
using Rule = System.Func<Sidi.Parse.Text>;

namespace Sidi.IO
{
    class PathParser : Parser
    {
        public static LPath Path(Text text)
        {
            var t = text.Copy();
            var prefix = Prefix(t);
            if (prefix == null) return null;
            var names = Names(t);
            if (names == null) return null;
            var orig = text;
            text.Set(t);
            return new LPath(orig.Remove(text).ToString());
        }

        public static Prefix Prefix(Text text)
        {
            var items = Alternative(text, LongUncPrefix, DeviceNamespacePrefix, LongPrefix, UncPrefix, LocalDrivePrefix, RootRelative, RelativePrefix);
            if (items == null)
            {
                return null;
            }

            var p = (Prefix) items.Childs[0];
            p.Text = items.ToString();
            return p;
        }

        public static Prefix RelativePrefix(Text text)
        {
            var t = Expect(text, String.Empty);
            if (t == null)
            {
                return null;
            }

            return new RelativePrefix { Text = t.ToString() };
        }

        public static Prefix LongPrefix(Text text)
        {
            var items = Concatenation(text, _ => (object) Expect(_, @"\\?\"), _ => (object)LocalDrivePrefix(_));
            if (items == null)
            {
                return null;
            }
            return new LocalDrivePrefix
            {
                Text = items.Text.ToString(),
                Drive = ((LocalDrivePrefix)items.Childs[1]).Drive
            };
        }

        public static Prefix LongUncPrefix(Text text)
        {
            var items = Concatenation(text, _ => Expect(_, @"\\?\UNC\"), ServerName, Separator, ShareName, Separator);
            if (items == null)
            {
                return null;
            }

            return new UncPrefix
            {
                Server = items[1].ToString(),
                Share = items[3].ToString(),
                Text = items.ToString()
            };
        }

        public static Prefix DeviceNamespacePrefix(Text text)
        {
            var t = Expect(text, @"\\.\");
            if (t == null)
            {
                return null;
            }
            return new DeviceNamespacePrefix { Text = t.ToString() };
        }

        public static IList<T> List<T, S>(Text text, Func<Text, T> element, Func<Text, S> separator)
        {
            var list = new List<T>();
            var t = text.Copy();

            for (; ; )
            {
                var item = element(t);
                if (item == null)
                {
                    break;
                }
                else
                {
                    list.Add(item);
                }
                if (separator(t) == null)
                {
                    break;
                }
            }
            text.Set(t);
            return list;
        }

        public static string[] Names(Text text)
        {
            var names = new List<string>();
            var t = text.Copy();

            for (; ; )
            {
                var n = NtfsFilename(t);
                if (n == null)
                {
                    break;
                }
                else
                {
                    names.Add(n.ToString());
                }
                if (Separator(t) == null)
                {
                    break;
                }
            }
            text = t;
            return names.ToArray();
        }

        public static Text NtfsFilename(Text text)
        {
            Text r = Repetition(text, 1, Int32.MaxValue, NtfsAllowedCharacter);
            if (r == null)
            {
                return null;
            }

            if (r.Length > LPath.MaxFilenameLength)
            {
                throw new System.ArgumentOutOfRangeException(String.Format("file name {0} is {1} characters long. {2} characters are allowed.", r, r.Length, LPath.MaxFilenameLength));
            }

            return r;
        }

        public static Text NtfsAllowedCharacter(Text text)
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
                case '\0':
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

            return Consume(text, 1);
        }

        public static Prefix UncPrefix(Text text)
        {
            var items = Concatenation(text, Separator, Separator, ServerName, Separator, ShareName, Separator);
            if (items == null)
            {
                return null;
            }

            return new UncPrefix
            {
                Server = items.Childs[2].ToString(),
                Share = items.Childs[4].ToString(),
                Text = items.ToString()
            };
        }

        public static Text ServerName(Text text)
        {
            return Repetition(text, 1, Int32.MaxValue, UncAllowedCharacter);
        }

        public static Text ShareName(Text text)
        {
            return Repetition(text, 1, Int32.MaxValue, UncAllowedCharacter);
        }

        public static Text UncAllowedCharacter(Text text)
        {
            if (text.Length < 1)
            {
                return null;
            }

            if (text[0] == '\\')
            {
                return null;
            }
            else
            {
                return Consume(text, 1);
            }
        }

        public static Prefix RootRelative(Text text)
        {
            var t = Separator(text);
            if (t == null)
            {
                return null;
            }
            return new RootRelativePrefix
            {
                Text = t.ToString()
            };
        }

        public static Prefix LocalDrivePrefix(Text text)
        {
            var items = Concatenation(text, DriveLetter, Colon, Separator);
            if (items == null)
            {
                return null;
            }

            return new LocalDrivePrefix
            {
                Drive = items[0].ToString(),
                Text = items.ToString()
            };
        }

        public static Text DriveLetter(Text text)
        {
            if (text.Length < 1)
            {
                return null;
            }

            var c = text[0];
            if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
            {
                return null;
            }

            return Consume(text, 1);
        }
    }
}
