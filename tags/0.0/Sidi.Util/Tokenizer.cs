// Copyright (c) 2008, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sidi.Util
{
    /// <summary>
    /// Simple text tokenizer. Separates strings at whitespace. Understands # comments, and quoted strings.
    /// </summary>
    public class Tokenizer
    {
        TextReader r;

        public Tokenizer(TextReader input)
        {
            r = input;
        }

        void Whitespace()
        {
            for (int n = r.Peek(); n != -1 && System.Char.IsWhiteSpace((char)n); n = r.Peek())
            {
                r.Read();    
            }
        }

        string Comment()
        {
            return r.ReadLine();
        }

        string QuotedString()
        {
            r.Read();
            string s = String.Empty;
            for (int n = r.Peek(); n != -1; n = r.Peek())
            {
                if ((char)n == '"')
                {
                    r.Read();
                    if ((char)r.Peek() == '"')
                    {
                        s += (char)r.Read();
                    }
                    else
                    {
                        break;
                    }
                }
                s += (char)r.Read();
            }
            return s;
        }

        string UnquotedString()
        {
            string s = String.Empty;
            for (int n = r.Peek(); n != -1 && !System.Char.IsWhiteSpace((char)n); n = r.Peek())
            {
                s += (char) r.Read();
            }
            return s;
        }

        public IEnumerable<String> Tokens
        {
            get
            {
                for (; ; )
                {
                    Whitespace();
                    int n = r.Peek();
                    if (n == -1)
                    {
                        break;
                    }
                    switch ((char)n)
                    {
                        case '#':
                            Comment();
                            break;
                        case '"':
                            yield return QuotedString();
                            break;
                        default:
                            yield return UnquotedString();
                            break;
                    }
                }
            }
        }
    }
}
