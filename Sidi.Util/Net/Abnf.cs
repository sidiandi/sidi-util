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
using System.Reflection;
using System.IO;
using Sidi.Util;
using Sidi.Extensions;

namespace Sidi.Net
{
    public class Abnf
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        class AbnfGrammar
        {
            public static rulelist rulelist(Match m)
            {
                return new rulelist()
                {
                    rule = m.Childs.Where(c => c.Closure is rule).Select(c => rule(c)).ToArray(),
                    match = m
                };
            }
            
            public static rule rule(Match m)
            {
                return new rule()
                {
                    rulename = m["rulename"].Value,
                    match = m,
                    closure = elements(m["elements"]),
                };
            }

            public static closure elements(Match m)
            {
                return alternation(m["alternation"]);
            }
            
            public static object rulename       (Match m)
            {
                return new rulename(m.Value)
                {
                    match = m,
                };
            }

            public static object defined(Match m)
            {
                return m;
            }

            public static object c_wsp          (Match m)
            {
                return m;
            }

            public static object c_nl           (Match m)
            {
                return m;
            }

            public static object comment        (Match m)
            {
                return m;
            }

            public static closure alternation    (Match m)
            {
                var c = m.Rules("concatenation").Select(x => concatenation(x)).ToArray();
                if (c.Length == 1)
                {
                    return c.First();
                }
                else
                {
                    return new alternation()
                    {
                        closure = c,
                        match = m,
                    };
                }
            }

            public static closure concatenation  (Match m)
            {
                var c = m.Rules("repetition").Select(x => repetition(x)).ToArray();
                if (c.Length == 1)
                {
                    return c.First();
                }
                else
                {
                    return new concatenation()
                    {
                        closure = c,
                        match = m,
                    };
                }
            }

            public static closure repetition     (Match m)
            {
                var r = m[0].Rules("repeat").Select(x => repeat(x)).FirstOrDefault();
                if (r != null)
                {
                    return new repetition()
                    {
                        closure = element(m["element"]),
                        match = m,
                        repeat = r,
                    };
                }
                else
                {
                    return element(m["element"]);
                }
            }

            public static repeat repeat         (Match m)
            {
                var r= new repeat();

                if (m.Childs.Length == 3)
                {
                    r.min_count = !m[0].Range.Empty ? Int32.Parse(m[0].Value) : 0;
                    r.max_count = !m[2].Range.Empty ? Int32.Parse(m[2].Value) : Int32.MaxValue;
                }
                else if (m.Childs.Length == 1)
                {
                    r.min_count = Int32.Parse(m[0].Value);
                    r.max_count = r.min_count;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(m.ToString());
                }

                return r;
            }

            public static closure element        (Match m)
            {
                return (closure) GetObject(m[0]);
            }

            public static object GetObject(Match m)
            {
                var rule = (rule)m.Closure;
                var creator = typeof(AbnfGrammar)
                    .GetMethod(Abnf.GetTypeName(rule.rulename), BindingFlags.Static | BindingFlags.Public);
                if (creator == null)
                {
                    throw new NotImplementedException(rule.rulename);
                }
                return creator.Invoke(null, new object[] { m });
            }

            public static object group          (Match m)
            {
                return alternation(m["alternation"]);
            }

            public static object option         (Match m)
            {
                return new repetition(0, 1, alternation(m["alternation"]));
            }

            public static char_val char_val(Match m)
            {
                return new char_val(m.Value)
                {
                    match = m,
                };
            }

            public static closure num_val(Match m)
            {
                return new num_val()
                {
                    match = m,
                    set = (int_set)GetObject(m[1])
                };
            }

            public static int_set int_set (Match m, int b)
            {
                // ""b"" 1*BIT [ 1*(""."" 1*BIT) / (""-"" 1*BIT) ]
                var ir = new int_range();
                ir.begin = Convert.ToInt32(m[1].Value, b);
                if (!m[2].Range.Empty)
                {
                    ir.end = Convert.ToInt32(m[1][1].Value, b);
                }

                return new bin_val()
                {
                    ranges = new[]{ ir }
                };
            }

            public static int_set bin_val(Match m)
            {
                return int_set(m, 2);
            }

            public static int_set dec_val(Match m)
            {
                return int_set(m, 10);
            }

            public static int_set hex_val(Match m)
            {
                return int_set(m, 16);
            }
        }

        public class rulelist : closure
        {
            public rulelist()
            {
                current = this;
            }

            public rulelist(Match m)
            {
                current = this;
                rule = m.Childs.Select(c => c.Create()).OfType<rule>().ToArray();
            }

            public rule[] rule;
            public static rulelist current;

            public override Match Parse(Range range)
            {
                return rule.First().Parse(range);
            }
        }

        public abstract class closure
        {
            public static implicit operator closure(string name)
            {
                return new rulename(name);
            }

            public Match match;

            public abstract Match Parse(Range range);

            public static closure Create(Match m)
            {
                if (!m.Success)
                {
                    return null;
                }

                if (m.Closure is rule)
                {
                    var rule = m.Closure as rule;
                    var typeName = rule.rulename.ToLower().Replace("-", "_");
                    var type = typeof(Abnf).GetNestedType(typeName);
                    if (type != null && typeof(closure).IsAssignableFrom(type))
                    {
                        return (closure)Activator.CreateInstance(type, new object[] { m });
                    }
                }
                return m.Childs.Select(c => Create(c)).First(c => c != null);
            }

            public static object GetObject(Match m)
            {
                if (m == null || !m.Success)
                {
                    return null;
                }

                m = new Match[]{m}.Concat(m.Childs)
                    .First(i => i.Closure is rule);

                var rule = m.Closure as rule;
                var typeName = rule.rulename.ToLower().Replace("-", "_");
                var type = typeof(Abnf).GetNestedType(typeName);
                if (type != null)
                {
                    return Activator.CreateInstance(type, new object[] { m });
                }
                else
                {
                    return null;
                }
            }

            protected Match Fail(Range range, params Match[] child)
            {
                return new Match()
                {
                    Success = false,
                    Closure = this,
                    Range = range,
                    Childs = child
                };
            }

            protected Match Fail(Range range)
            {
                return new Match()
                {
                    Success = false,
                    Closure = this,
                    Range = range,
                    Childs = new Match[] { }
                };
            }
        }

        public class rule : closure
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public string rulename;
            public closure closure;

            public rule(string rulename, closure closure)
            {
                this.rulename = rulename;
                this.closure = closure;
            }

            public rule()
            {
            }

            public override string ToString()
            {
                return rulename;
            }

            override public Match Parse(Range range)
            {
                var m = closure.Parse(range);
                if (m.Success)
                {
                    if (m.Closure is rule)
                    {
                        return new Match(this, m.Range, new Match[] { m });
                    }
                    else
                    {
                        return new Match(this, m.Range, m.Childs);
                    }
                }
                else
                {
                    return Fail(range, m);
                }
            }
        }

        public class alternation : closure
        {
            public alternation(params closure[] c)
            {
                closure = c;
            }

            public closure[] closure;

            public override Match Parse(Range range)
            {
                var alternatives = closure.Select(x => x.Parse(range)).ToArray();
                var best = alternatives.Best(x => x.Range.Length + (x.Success ? 0 : Int32.MinValue));
                if (best.Success)
                {
                    return best;
                }
                else
                {
                    return Fail(range);
                }
            }
        }

        public class concatenation : closure
        {
            public concatenation(params closure[] closure)
            {
                this.closure = closure;
            }

            public closure[] closure;

            public override Match Parse(Range range)
            {
                var c = new Match[closure.Length];
                var rest = range;
                int index = 0;
                foreach (var i in closure)
                {
                    var m = i.Parse(rest);
                    if (!m.Success)
                    {
                        return Fail(range, m);
                    }
                    c[index++] = m;
                    rest = rest.Remove(m.Range);
                }
                return new Match(this, range.Part(c.First().Range.Begin, c.Last().Range.End), c.ToArray());
            }
        }

        public class repeat
        {
            public repeat()
            {
            }

            public int min_count;
            public int max_count;
        }

        public class repetition : closure
        {
            public repetition(int min, int max, closure closure)
            {
                repeat = new repeat()
                {
                    min_count = min,
                    max_count = max,
                };
                this.closure = closure;
            }

            public repetition()
            {
            }

            public repeat repeat;
            public closure closure;

            public override Match Parse(Range range)
            {
                var c = new List<Match>();
                var rest = range;
                Match m = null;
                for (int i=0; i<repeat.max_count; ++i)
                {
                    m = closure.Parse(rest);
                    if (!m.Success)
                    {
                        break;
                    }
                    c.Add(m);
                    rest = rest.Remove(m.Range);
                }
                if (c.Count >= repeat.min_count)
                {
                    if (c.Count > 0)
                    {
                        return new Match(this, range.Part(c.First().Range.Begin, c.Last().Range.End), c.ToArray());
                    }
                    else
                    {
                        return new Match(this, range.Part(range.Begin, range.Begin), c.ToArray());
                    }
                }
                else
                {
                    return Fail(range, m);
                }
            }
        }

        public class option : closure
        {
            public option(closure closure)
            {
                this.closure = closure;
            }

            public closure closure;

            public override Match Parse(Range range)
            {
                var m = closure.Parse(range);
                if (m.Success)
                {
                    return new Match(this, m.Range, new Match[] { m });
                }
                else
                {
                    return new Match(this, range.Part(range.Begin, range.Begin), new Match[] {m});
                }
            }
        }

        public class char_val : closure
        {
            public char_val(string value)
            {
                this.value = value;
            }

            public string value;

            public override Match Parse(Range range)
            {
                var r = range.Left(value.Length);
                if (r.Text.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Match(this, r, new Match[] { });
                }
                else
                {
                    return Fail(range);
                }
            }
        }

        public class int_range
        {
            public int begin;
            public int end;

            public bool Contains(int i)
            {
                return begin <= i && i <= end;
            }
        }

        public class int_set
        {
            public int_range[] ranges;

            public bool Contains(int i)
            {
                return ranges.Any(r => r.Contains(i));
            }
        }

        public class bin_val : int_set
        {
        }

        public class dec_val : int_set
        {
        }

        public class hex_val : int_set
        {
        }

        public class num_val : closure
        {
            public num_val(int begin)
            : this(begin, begin)
            {
            }

            public int_set set;

            public num_val(int begin, int end)
            {
                set = new int_set()
                {
                    ranges = new int_range[] { new int_range()
                    {
                        begin = begin,
                        end = end,
                    }}
                };
            }

            public num_val()
            {
            }

            public override Match Parse(Range range)
            {
                if (range.Empty)
                {
                    return Fail(range);
                }

                int v = range.FirstChar;
                if (set.Contains(v))
                {
                    return new Match(this, range.Left(1), new Match[] { });
                }
                else
                {
                    return Fail(range);
                }
            }
        }

        public class rulename : closure
        {
            public rulename(string name)
            {
                this.name = name;
            }

            public string name;
            public rulelist rulelist = rulelist.current;

            rule Rule
            {
                get
                {
                    if (rule == null)
                    {
                        rule = coreRules.rule.Concat(rulelist.rule).First(r => r.rulename.Equals(name));
                    }
                    return rule;
                }
            }
            rule rule;

            public override Match Parse(Range range)
            {
                return Rule.Parse(range);
            }
        }

        public class prose_val : closure
        {
            public string value;

            public override Match Parse(Range range)
            {
                throw new NotImplementedException();
            }
        }

        rulelist rules;
        public static Abnf abnf;
        static rulelist coreRules;

        static closure any(closure c)
        {
            return new repetition(0, Int32.MaxValue, c);
        }

        static closure c(string charval)
        {
            return new char_val(charval);
        }

        public class Range
        {
            public Range(string text)
                : this(text, 0, text.Length)
            {
            }

            public Range(string text, int begin, int end)
            {
                this.begin = begin;
                this.end = end;
                this.text = text;
            }

            string text;
            int begin;
            int end;

            public int Begin { get { return begin; } }
            public int End { get { return end; } }
            public int Length { get { return end - begin; } }

            public override string ToString()
            {
                return this.Left(80).Text.ToLiteral();
            }

            public bool Empty
            {
                get
                {
                    return begin >= end;
                }
            }

            public string Text
            {
                get
                {
                    return text.Substring(begin, end - begin);
                }
            }

            public Range Part(int b, int e)
            {
                return new Range(text, b, e);
            }

            public Range Full
            {
                get
                {
                    return new Range(text, 0, text.Length);
                }
            }
            
            public Range Left(int length)
            {
                return Part(Begin, Math.Min(End, Begin + length));
            }

            public override bool Equals(object obj)
            {
                var r = obj as Range;
                return r != null && r.Text == Text && r.Begin == Begin && r.End == End;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public char FirstChar
            {
                get
                {
                    return text[begin];
                }
            }

            public Range Remove(Range range)
            {
                if (range.Begin != this.Begin)
                {
                    throw new ArgumentOutOfRangeException("range");
                }
                return Part(range.End, this.End);
            }
        }

        public class Match
        {
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public Match()
            {
            }

            public void DumpAll(TextWriter o)
            {
                Dump(o, String.Empty, m => true);
            }

            public void DumpRules(TextWriter o)
            {
                Dump(o, String.Empty, m => m.Closure is rule);
            }

            public void Dump(TextWriter o, string indent, Func<Match, bool> displaySelector)
            {
                if(displaySelector(this))
                {
                    o.WriteLine("{0}{1}: {2}", indent, Closure.ToString(), Range.Left(80));
                }
                    var childIndent = indent + " ";
                foreach (var c in Childs)
                {
                    c.Dump(o, childIndent, displaySelector);
                }
            }

            public Match(closure rule, Range range, Match[] childs)
            {
                this.Closure = rule;
                this.Range = range;
                this.Childs = childs;
                this.Success = true;
                // log.InfoFormat("{0}: {1}", Rule, Range.Full.Part(Range.Begin, Range.Begin + 80));
            }

            public bool Success;
            public closure Closure;
            public Range Range;
            public Match[] Childs;

            public override string ToString()
            {
                return String.Format("{0}: {1}", Closure, Range);
            }

            public IEnumerable<Match> GetChildRules(string ruleName)
            {
                foreach (var i in Childs)
                {
                    if (i.Closure is rule && ((rule)i.Closure).rulename.Equals(ruleName))
                    {
                        yield return i;
                    }
                    else
                    {
                        foreach (var c in i.GetChildRules(ruleName))
                        {
                            yield return c;
                        }
                    }
                }
            }

            public bool Is(string rulename)
            {
                return Success && Closure is rule && ((rule)Closure).rulename.Equals(rulename, StringComparison.InvariantCultureIgnoreCase);
            }

            public IEnumerable<Match> Rules(string name)
            {
                return Childs.Where(c => c.Is(name));
            }

            /// <summary>
            /// Returns the first child rule with name
            /// </summary>
            /// <param name="name"></param>
            /// <returns>null, if rule not present</returns>
            public Match this[string name]
            {
                get
                {
                    return Rules(name).First();
                }
            }

            public Match this[int n ]
            {
                get
                {
                    return Childs[n];
                }
            }

            public string Value
            {
                get
                {
                    return Range.Text;
                }
            }

            /// <summary>
            /// Used for Create
            /// </summary>
            static public List<Type> Types = new List<Type>();

            public object Create()
            {
                if (!Success)
                {
                    return null;
                }

                var rule = this.Closure as rule;
                if (rule == null)
                {
                    return null;
                }
                var tn = GetTypeName(rule.rulename);
                var type = Types.FirstOrDefault(t => t.Name.Equals(tn));
                if (type == null)
                {
                    return null;
                }
                return Activator.CreateInstance(type, new object[] { this });
            }
        }

        public static string GetTypeName(string ruleName)
        {
            return ruleName.ToLower().Replace("-", "_");
        }

        Abnf()
        {
        }

        static Abnf()
        {
            coreRules = new rulelist()
            {
                rule = new rule[]
                {
new rule("ALPHA", new alternation(new num_val(0x41, 0x5A), new num_val(0x61, 0x7A))), //    ; A-Z / a-z),

new rule("BIT", new alternation(new char_val("0"), new char_val("1"))),

new rule("CHAR", new num_val(0x01, 0x7F)), // ; any 7-bit US-ASCII character, excluding NUL

new rule("CR", new num_val(0x0D)), //; carriage return

new rule("CRLF", new concatenation("CR", "LF")), // ; Internet standard newline

new rule("CTL", new alternation(new num_val(0x00, 0x1F), new num_val(0x7F))), // ; controls

new rule("DIGIT", new num_val(0x30, 0x39)), // ; 0-9

new rule("DQUOTE", new num_val(0x22)), // ; " (Double Quote)

new rule("HEXDIG", new alternation("DIGIT", new num_val('A', 'F'))), //         =  DIGIT / "A" / "B" / "C" / "D" / "E" / "F"),

new rule("HTAB", new num_val(0x09)), // ; horizontal tab

new rule("LF", new num_val(0x0A)), // ; linefeed

new rule("LWSP", any(new alternation("WSP", new concatenation("CRLF", "WSP")))), // ; linear white space (past newline)

new rule("OCTET", new num_val(0x00, 0xFF)),  // ; 8 bits of data

new rule("SP", new num_val(0x20)),

new rule("VCHAR", new num_val(0x21, 0x7E)),  // ; visible (printing) characters

new rule("WSP", new alternation("SP", "HTAB")) //; white space                
                }
            };

            abnf = new Abnf();
            abnf.rules = new rulelist()
            {
                rule = new rule[]
                {
                    new rule("rulelist", 
                        new repetition(1, Int32.MaxValue,
                            new alternation(
                                "rule",
                                new concatenation(
                                    any("c-wsp"),
                                    "c-nl"
                                    )
                                )
                            )
                        ),

                    new rule("rule", new concatenation("rulename", "defined-as", "elements", "c-nl")),

                    new rule("rulename", new concatenation("ALPHA", any(new alternation("ALPHA", "DIGIT", new char_val("-"))))),

                    new rule("defined-as", new concatenation(any("c-wsp"), new alternation(new char_val("="), new char_val("=/")), any("c-wsp"))),

                    new rule("elements", new concatenation("alternation", any("c-wsp"))),

                    new rule("c-wsp", new alternation("WSP", new concatenation("c-nl", "WSP"))),

                    new rule("c-nl", new alternation("comment", "CRLF")),

                    new rule("comment", new concatenation(
                        new char_val(";"), any(new alternation("WSP", "VCHAR")), "CRLF")),

                    new rule("alternation", new concatenation(
                        "concatenation", any(new concatenation(any("c-wsp"), new char_val("/"), any("c-wsp"), "concatenation")))),

                    new rule("concatenation", new concatenation("repetition", any(new concatenation(
                        new repetition(1, Int32.MaxValue, "c-wsp"), "repetition")))),

                    new rule("repetition", new concatenation(new option("repeat"), "element")),

                    new rule("repeat", new alternation(
                        new repetition(1, Int32.MaxValue, "DIGIT"),
                        new concatenation(any("DIGIT"), new char_val("*"), any("DIGIT")))),

                    new rule("element", new alternation("rulename", "group", "option", "char-val", "num-val", "prose-val")),

                    new rule("group", new concatenation(new char_val("("), any("c-wsp"), "alternation", any("c-wsp"), new char_val(")"))),

                    new rule("option", new concatenation(new char_val("["), any("c-wsp"), "alternation", any("c-wsp"), new char_val("]"))),

                    new rule("char-val", new concatenation("DQUOTE", any(new alternation(new num_val(0x20, 0x21), new num_val(0x23, 0x7e))), "DQUOTE")),

                    new rule("num-val", new concatenation(new char_val("%"), new alternation("bin-val", "dec-val", "hex-val"))),

                    new rule("bin-val", new concatenation(
                        new char_val("b"), 
                        new repetition(1, Int32.MaxValue, "BIT"), 
                        new option(
                            new alternation(
                                new repetition(1, Int32.MaxValue, 
                                    new concatenation(
                                        new char_val("."), 
                                        new repetition(1, Int32.MaxValue, "BIT")
                                    )
                                ),
                                new concatenation(
                                    new char_val("-"),
                                    new repetition(1, Int32.MaxValue, "BIT")
                                )
                            )
                        )
                    )),

                    new rule("dec-val", new concatenation(
                        new char_val("d"), 
                        new repetition(1, Int32.MaxValue, "DIGIT"), 
                        new option(
                            new alternation(
                                new repetition(1, Int32.MaxValue, 
                                    new concatenation(
                                        new char_val("."), 
                                        new repetition(1, Int32.MaxValue, "DIGIT")
                                    )
                                ),
                                new concatenation(
                                    new char_val("-"),
                                    new repetition(1, Int32.MaxValue, "DIGIT")
                                )
                            )
                        )
                    )),

                    new rule("hex-val", new concatenation(
                        new char_val("x"), 
                        new repetition(1, Int32.MaxValue, "HEXDIG"), 
                        new option(
                            new alternation(
                                new repetition(1, Int32.MaxValue, 
                                    new concatenation(
                                        new char_val("."), 
                                        new repetition(1, Int32.MaxValue, "HEXDIG")
                                    )
                                ),
                                new concatenation(
                                    new char_val("-"),
                                    new repetition(1, Int32.MaxValue, "HEXDIG")
                                )
                            )
                        )
                    )),

                    new rule("prose-val", new concatenation(
                        new char_val("<"),
                        new repetition(1, Int32.MaxValue, new alternation(new num_val(0x20, 0x3D), new num_val(0x3F, 0x7E))),
                        new char_val(">")))
                }
            };
        }

        public Abnf(string grammar)
        {
            var rulelist = abnf.Parse(grammar, "rulelist");

            if (!rulelist.Success)
            {
                using (var w = new StringWriter())
                {
                    w.WriteLine("Cannot parse grammar");
                    rulelist.DumpAll(w);
                    throw new ArgumentException(w.ToString());
                }
            }

            Match.Types.AddRange(this.GetType().GetNestedTypes());
            this.rules = AbnfGrammar.rulelist(rulelist);
        }

        public Match Parse(string text, string rule)
        {
            var range = new Range(text, 0, text.Length);

            var m = rules.rule
                .Concat(coreRules.rule)
                .First(r => r.rulename.Equals(rule, StringComparison.InvariantCultureIgnoreCase))
                .Parse(range);

            if (!m.Success)
            {
                m.DumpAll(Console.Out);
            }

            return m;
        }
    }
}
