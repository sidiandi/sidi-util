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

namespace Sidi.Util
{
    public class Rate
    {
        double m_value = 0;
        DateTime m_time = DateTime.Now;
        double m_window = 2;

        public double Value
        {
            get
            {
                lock (this)
                {
                    Add(0);
                    return m_value;
                }
            }
        }

        public void Add(double increment)
        {
            lock (this)
            {
                DateTime t = DateTime.Now;
                TimeSpan dt = t - m_time;
                m_value = (m_window * m_value + increment) / (m_window + dt.TotalSeconds);
                m_time = t;
            }
        }
    }
}
