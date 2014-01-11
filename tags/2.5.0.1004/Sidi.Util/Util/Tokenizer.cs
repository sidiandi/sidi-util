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
using System.Text;
using System.IO;
using System.Linq;
using Sidi.IO;

namespace Sidi.Util
{
    public class ReadAheadTextReader : TextReader
    {
        TextReader underlyingReader;
        
        public ReadAheadTextReader(TextReader underlyingReader)
        {
            this.underlyingReader = underlyingReader;
        }

        int peek;
        bool peekValid = false;

        public override int Peek()
        {
            if (!peekValid)
            {
                peek = underlyingReader.Read();
                peekValid = true;
            }
            return peek;
        }

        public override int Read()
        {
            if (peekValid)
            {
                peekValid = false;
                return peek;
            }
            else
            {
                return underlyingReader.Read();
            }
        }
    }

    /// <summary>
    /// Simple text tokenizer. Separates strings at whitespace. Understands # comments, and quoted strings.
    /// </summary>
    public class Tokenizer
    {
        TextReader r;

        public static string[] ToArray(string tokenString)
        {
            using (var sr = new StringReader(tokenString))
            {
                return new Tokenizer(sr).Tokens.ToArray();
            }
        }

        public static List<string> ToList(string tokenString)
        {
            using (var sr = new StringReader(tokenString))
            {
                return new Tokenizer(sr).Tokens.ToList();
            }
        }

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

        public static string[] FromFile(LPath file)
        {
            using (var r = LFile.StreamReader(file))
            {
                return new Tokenizer(r).Tokens.ToArray();
            }
        }
    }
}
