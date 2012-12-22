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
using System.Text;
using System.Text.RegularExpressions;

namespace Sidi.Util
{
    public static class RandomExtensions
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
