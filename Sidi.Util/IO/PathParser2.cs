﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Parse;
using Rule = System.Func<Sidi.Parse.Text, Sidi.Parse.Ast>;
using Sidi.Extensions;

namespace Sidi.IO
{
    /// <summary>
    /// Parser used by LPath
    /// </summary>
    internal class PathParser : Parser
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Rule Path()
        {
            return Concatenation(Prefix(), Names());
        }

        public static Rule Prefix()
        {
            return Rename(() => Alternative(
                SpecialPrefix(),
                LocalDrivePrefix(),
                RootRelative(),
                RelativePrefix()));
        }

        public static Rule SpecialPrefix()
        {
            var special = Concatenation(Separator(), Separator(), MandatoryAlternative(
                LongUncPrefix(), 
                DeviceNamespacePrefix(),
                LongPrefix(),
                UncPrefix()
                ));

            return text =>
                {
                    var t = text.Copy();
                    var a = special(t);
                    if (a != null)
                    {
                        a.Name = a.Childs[2].Childs[0].Name;
                        a.Childs = a.Childs[2].Childs[0].Childs;
                        text.Set(t);
                    }
                    return a;
                };
        }

        public static Rule RelativePrefix()
        {
            return Enclose(() => Expect(String.Empty));
        }

        public static Rule LongPrefix()
        {
            return Rename(() => Concatenation(Expect(@"?\"), DriveLetter(), Colon(), Separator()));
        }

        public static Rule LongUncPrefix()
        {
            return Rename(() => Concatenation(Expect(@"?\UNC\"), ServerName(), Separator(), ShareName(), Separator()));
        }

        public static Rule DeviceNamespacePrefix()
        {
            return Enclose(() => Expect(@".\"));
        }

        public static Rule List(Rule element, Rule separator)
        {
            return Rename(() => text =>
                {
                    var elements = new List<Ast>();
                    for (; ; )
                    {
                        var item = element(text);
                        if (item == null)
                        {
                            break;
                        }
                        else
                        {
                            elements.Add(item);
                        }
                        if (separator(text) == null)
                        {
                            break;
                        }
                    }
                    return new Ast(null, elements);
                });
        }

        public static Rule Names()
        {
            return Rename(() => List(NtfsFilenameWithWildcards(), Separator()));
        }

        public static Rule Name(int maxLength, Rule character)
        {
            return Collapse(() => text =>
                {
                    var r = Repetition(1, Int32.MaxValue, character)(text);
                    if (r == null)
                    {
                        return null;
                    }

                    if (r.Text.Length > maxLength)
                    {
                        throw new System.ArgumentOutOfRangeException(String.Format("name {0} is {1} characters long. {2} characters are allowed.", r, r.Text.Length, LPath.MaxFilenameLength));
                    }

                    return r;
                });
        }

        public static Rule NtfsFilename()
        {
            return Collapse(() => Name(LPath.MaxFilenameLength, NtfsAllowedCharacter()));
        }

        public static Rule NtfsFilenameWithWildcards()
        {
            return Collapse(() => Name(LPath.MaxFilenameLength, NtfsAllowedCharacterWithWildcards()));
        }

        public static Rule NtfsAllowedCharacter()
        {
            return Rename(() => ExpectCharacter(c => !illegalFilenameCharacters.Contains(c)));
        }

        static HashSet<char> illegalFilenameCharacters = new HashSet<char>(System.IO.Path.GetInvalidFileNameChars());
        static HashSet<char> illegalFilenameCharactersWithoutWildcards = new HashSet<char>(System.IO.Path.GetInvalidFileNameChars()
            .Where(x => x != '*' && x != '?'));

        public static Rule NtfsAllowedCharacterWithWildcards()
        {
            return text =>
                {
                    if (text.Length < 1)
                    {
                        return null;
                    }

                    var c = text[0];

                    if (illegalFilenameCharactersWithoutWildcards.Contains(c))
                    {
                        return null;
                    }

                    return Consume(1)(text);
                };
        }

        public static Rule UncPrefix()
        {
            return Rename(() => Concatenation(ServerName(), Separator(), ShareName(), SeparatorOrEnd()));
        }

        static Rule SeparatorOrEnd()
        {
            return OrEnd(Separator(), @"\");
        }

        public static Rule OrEnd(Rule rule, string defaultString)
        {
            return (text) =>
                {
                    if (text.Length == 0)
                    {
                        return new Ast(new Text(defaultString));
                    }

                    return rule(text);
                };
        }

        public static Rule ServerName()
        {
            return Collapse(() => Name(Int32.MaxValue, UncAllowedCharacter()));
        }

        public static Rule ShareName()
        {
            return Collapse(() => Name(Int32.MaxValue, UncAllowedCharacter()));
        }

        public static Rule UncAllowedCharacter()
        {
            return text =>
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
                        return Consume(1)(text);
                    }
                };
        }

        public static Rule RootRelative()
        {
            return Collapse(() => Separator());
        }

        public static Rule LocalDrivePrefix()
        {
            return Rename(() => Concatenation(DriveLetter(), Colon(), SeparatorOrEnd()));
        }

        public static Rule DriveLetter()
        {
            return Collapse(() => text =>
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

                    return Consume(1)(text);
                });
        }

        internal static string MakeValidFilename(Text text)
        {
            var r = Repetition(1, Int32.MaxValue, Alternative(
                NtfsAllowedCharacter(), Consume(1)))(text);

            var valid = r.Childs.Select(x =>
                {
                    if (object.Equals(x[0].Name, "NtfsAllowedCharacter"))
                    {
                        return x[0].ToString();
                    }
                    else
                    {
                        return "_";
                    }
                })
                .Join(String.Empty);

            return valid.Shorten(LPath.MaxFilenameLength);
        }
    }
}
