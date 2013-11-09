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
    public class PercentileColorScale
    {
        Bins bins;
        ColorScale colorScale;

        public PercentileColorScale(IEnumerable<IComparable> list, Color[] colorScale)
        {
            bins = new Bins(list);
            this.colorScale = new ColorScale(0.0, 1.0, colorScale);
        }

        public Color GetColor(IComparable item)
        {
            var p = bins.Percentile(item);
            return colorScale.ToColor(p);
        }
    }
}
