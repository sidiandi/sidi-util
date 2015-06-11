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
using Sidi.IO;
using Sidi.Util;
using Sidi.Extensions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Cryptography;
using Sidi.Persistence;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace Sidi.Caching
{
    /// <summary>
    /// Pre-computes values and stores them as flat files. The computation results will be persistent between runs 
    /// of the program.
    /// </summary>
    public class SqliteCache : CacheBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SqliteCache(LPath dbPath)
            : base(new SQLiteHashAddressableStorage(dbPath), HashProvider.GetDefault())
        {
        }

        public static SqliteCache Local(MethodBase method)
        {
            return new SqliteCache(Cache.GetLocalCachePath(method).CatName(".sqlite"));
        }
    }
}
