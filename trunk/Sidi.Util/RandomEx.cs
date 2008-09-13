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
using System.Text.RegularExpressions;

namespace Sidi.Util
{
    public static class RandomEx
    {
        public static string String(this Random random, int length)
        {
            return String(random, length, @"[0-9a-zA-Z]");
        }

        public static string String(this Random random, int length, string allowedCharacterRegex)
        {
            List<String> allowed = new List<String>();
            Regex r = new Regex(allowedCharacterRegex);
            for (char c = Char.MinValue; c < (char)256; ++c)
            {
                String s = new String(new char[] { c });
                if (r.Match(s).Success)
                {
                    allowed.Add(s);
                }
            }
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < length; ++i)
            {
                b.Append(allowed[random.Next(0, allowed.Count)]);
            }
            return b.ToString();
        }
    }
}
