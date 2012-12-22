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
using System.Drawing;

namespace Sidi.Visualization
{
    public class DistinctColor
    {
        Dictionary<object, Color> valueToColor;

        public DistinctColor()
        {
            valueToColor = new Dictionary<object, Color>();
        }

        Color GetColor(int n)
        {
            var hsl = new HSLColor(Color.Red);
            hsl.Hue += Fill(n);
            return hsl;
        }

        double Fill(int n)
        {
            double x = 0.0;
            double increment = 1.0;

            for (; n > 0; n = n >> 1)
            {
                increment /= 2;
                if ((n & 1) != 0)
                {
                    x += increment;
                }
            }
            return x;
        }

        public Color ToColor(object value)
        {
            Color color;
            if (!valueToColor.TryGetValue(value, out color))
            {
                var n = valueToColor.Count + 1;
                color = GetColor(n);
                valueToColor[value] = color;
            }
            return color;
        }
    }
}
