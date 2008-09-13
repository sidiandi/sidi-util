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
using System.Linq;
using System.Text;
using System.IO;

namespace Sidi.Util
{
    public static class StringEx
    {
        public static string OneLine(this string text, int maxLen)
        {
            StringReader r = new StringReader(text);
            StringWriter w = new StringWriter();
            for (string line = r.ReadLine(); ; )
            {
                w.Write(line);
                line = r.ReadLine();
                if (line == null)
                {
                    break;
                }
                w.Write(" ");
            }
            string result = w.ToString();
            if (result.Length > maxLen)
            {
                return result.Substring(0, maxLen);
            }
            else
            {
                return result;
            }
        }

        public static string JoinLines(this IEnumerable<string> lines)
        {
            bool first = true;
            StringBuilder s = new StringBuilder();
            foreach (string i in lines)
            {
                if (!String.IsNullOrEmpty(i))
                {
                    if (!first)
                    {
                        s.AppendLine();
                    }
                    s.Append(i);
                    first = false;
                }
            }
            return s.ToString();
        }

        public static string Quote(this string s)
        {
            return "\"" + s + "\"";
        }
    }
}
