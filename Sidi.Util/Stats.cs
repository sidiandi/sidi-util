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
