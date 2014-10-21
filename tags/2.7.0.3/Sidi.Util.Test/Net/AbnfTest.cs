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
using NUnit.Framework;
using Sidi.Test;

namespace Sidi.Net
{
    [TestFixture]
    public class AbnfTest : TestBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        const string abnf = @"rulelist       =  1*( rule / (*c-wsp c-nl) )

rule           =  rulename defined-as elements c-nl
                        ; continues if next line starts
                        ;  with white space

rulename       =  ALPHA *(ALPHA / DIGIT / ""-"")

defined-as     =  *c-wsp (""="" / ""=/"") *c-wsp
                        ; basic rules definition and
                        ;  incremental alternatives

elements       =  alternation *c-wsp

c-wsp          =  WSP / (c-nl WSP)

c-nl           =  comment / CRLF
                        ; comment or newline

comment        =  "";"" *(WSP / VCHAR) CRLF

alternation    =  concatenation
                    *(*c-wsp ""/"" *c-wsp concatenation)

concatenation  =  repetition *(1*c-wsp repetition)

repetition     =  [repeat] element

repeat         =  1*DIGIT / (*DIGIT ""*"" *DIGIT)

element        =  rulename / group / option /
                    char-val / num-val / prose-val

group          =  ""("" *c-wsp alternation *c-wsp "")""

option         =  ""["" *c-wsp alternation *c-wsp ""]""

char-val       =  DQUOTE *(%x20-21 / %x23-7E) DQUOTE
                        ; quoted string of SP and VCHAR
                            without DQUOTE

num-val        =  ""%"" (bin-val / dec-val / hex-val)

bin-val        =  ""b"" 1*BIT
                    [ 1*(""."" 1*BIT) / (""-"" 1*BIT) ]
                        ; series of concatenated bit values
                        ; or single ONEOF range

dec-val        =  ""d"" 1*DIGIT
                    [ 1*(""."" 1*DIGIT) / (""-"" 1*DIGIT) ]

hex-val        =  ""x"" 1*HEXDIG
                    [ 1*(""."" 1*HEXDIG) / (""-"" 1*HEXDIG) ]
";

        public void Check(string rule, string input)
        {
            var all = new Abnf.Range(input);
            var m = Abnf.abnf.Parse(input, rule);
            if (m.Success)
            {
                m.DumpRules(Console.Out);
            }
            else
            {
                m.DumpAll(Console.Out);
            }
            Assert.IsTrue(m.Success);
            var rest = all.Remove(m.Range);
            Assert.IsTrue(rest.Empty, rest.ToString());
        }

        [Test]
        public void Create()
        {
            Check("rulename", "ALPHA");
            Check("rule", @"rulename = ALPHA
");
        }

        [Test]
        public void Element()
        {
            Check("element", "ALPHA");
            // var p = new Abnf(abnf);
        }

        [Test]
        public void AbnfGrammar()
        {
            Check("rulelist", abnf);
        }

        [Test]
        public void SelfParse()
        {
            var parser = new Abnf(abnf);
            var m = parser.Parse(abnf, "rulelist");
            Assert.IsTrue(m.Success);
        }
    }
}
