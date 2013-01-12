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
using System.Net;
using System.Reflection;
using System.Net.Sockets;

namespace Sidi.Net
{
    public static class TcpClientExtensions
    {
        public static EndPoint GetRemoteEndPoint(this TcpClient c)
        {
            PropertyInfo pi = c.GetStream().GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
            Socket socket = (Socket)pi.GetValue(c.GetStream(), new object[] { });
            return socket.RemoteEndPoint;
        }
    }
}