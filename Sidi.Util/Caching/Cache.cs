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

namespace Sidi.Caching
{
    /// <summary>
    /// Pre-computes values and stores them as flat files. The computation results will be persistent between runs 
    /// of the program.
    /// </summary>
    public class Cache : CacheBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Cache(LPath storeDirectory)
            : base(new HybridHashAddressableStorage(storeDirectory), HashProvider.GetDefault())
        {
        }

        internal static LPath GetLocalCachePath(MethodBase method)
        {
            return Paths.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                .CatDir(
                    Paths.Get(method.DeclaringType.Assembly), 
                    "cache", 
                    LPath.GetValidFilename(method.DeclaringType.FullName),
                    LPath.GetValidFilename(method.Name));
        }

        public static Cache Local(MethodBase method)
        {
            Cache localCache;
            var cachePath = GetLocalCachePath(method);
            if (localCaches.TryGetValue(cachePath, out localCache))
            {

            }
            else
            {
                localCache = new Cache(cachePath);
                localCaches[cachePath] = localCache;
            }
            return localCache;
        }

        static Dictionary<LPath, Cache> localCaches = new Dictionary<LPath, Cache>();

        public static void DisposeLocalCaches()
        {
            foreach (var i in localCaches.Values)
            {
                i.Dispose();
            }
            localCaches.Clear();
        }

        /// <summary>
        /// Returns a default instance, which stores values in local AppData
        /// Typical use:
        /// var cache = Cache.Local(MethodBase.GetCurrentMethod());
        /// </summary>
        /// <param name="id">Identifier of the cache</param>
        /// <returns>A cache object specific to id</returns>
        public static Cache Local(object id)
        {
            Type type = null;
            if (id is MethodBase)
            {
                type = ((MethodBase)id).DeclaringType;
            }
            else if (id is Type)
            {
                type = (Type)id;
            }
            else
            {
                type = id.GetType();
            }

            return new Cache(
                Paths.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                .CatDir(Paths.Get(type), "cache", HashProvider.GetDefault().GetObjectHash(id).Value.HexString()));
        }
    }
}
