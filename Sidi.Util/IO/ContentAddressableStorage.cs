﻿using Sidi.IO;
using Sidi.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;

namespace Sidi.IO
{
    public class ContentAddressableStorage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IHashProvider hashAlgorithm;
        LPath rootDirectory;

        public ContentAddressableStorage(LPath rootDirectory, IHashProvider hashAlgorithm)
        {
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("rootDirectory");
            }

            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            this.rootDirectory = rootDirectory;
            rootDirectory.EnsureDirectoryExists();
            this.hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        /// True, if the storage contains content with the specified hash value.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public bool Contains(Hash hash)
        {
            var p = CalculatePath(hash);
            return p.IsFile;
        }

        public class WriteResult
        {
            public WriteResult(Hash hash, bool added)
            {
                this.Hash = hash;
                this.Added = added;
            }

            /// <summary>
            /// Hash that was assigned to this content
            /// </summary>
            public readonly Hash Hash;

            /// <summary>
            /// True, if content was added to the storage, false if it already did exist.
            /// </summary>
            public readonly bool Added;
        }

        /// <summary>
        /// Writes the contents of file content to the storage.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public WriteResult Write(LPath content)
        {
            var hash = hashAlgorithm.Get(content);
            bool added;
            if (Contains(hash))
            {
                added = false;
            }
            else
            {
                var dest = CalculatePath(hash);
                var tempFile = GetTempFile();
                try
                {
                    content.CopyFile(tempFile);
                    dest.EnsureParentDirectoryExists();
                    tempFile.Move(dest);
                    added = true;
                }
                finally
                {
                    tempFile.EnsureFileNotExists();
                }
            }
            return new WriteResult(hash, added);
        }

        LPath GetTempFile()
        {
            return rootDirectory.CatDir(Path.GetRandomFileName());
        }

        /// <summary>
        /// Writes the contents of stream content to the storage.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public WriteResult Write(Stream content)
        {
            var tempFile = GetTempFile();
            using (var w = tempFile.OpenWrite())
            {
                content.CopyTo(w);
            }
            var hash = hashAlgorithm.Get(tempFile);
            bool added;
            if (Contains(hash))
            {
                tempFile.EnsureFileNotExists();
                added = false;
            }
            else
            {
                var dest = CalculatePath(hash);
                dest.EnsureParentDirectoryExists();
                tempFile.Move(dest);
                added = true;
            }
            return new WriteResult(hash, added);
        }

        /// <summary>
        /// Returns the content addressed by the specified hash as stream
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public Stream Read(Hash hash)
        {
            return GetPath(hash).OpenRead();
        }

        /// <summary>
        /// Copies the content addressed by the specified hash to file destinationFile.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public void Read(Hash hash, LPath destinationFile)
        {
            GetPath(hash).CopyFile(destinationFile);
        }

        LPath CalculatePath(Hash hash)
        {
            return rootDirectory.CatDir(hash.Value.Window(4, 4).Select(_ => _.HexString()));
        }

        /// <summary>
        /// Returns the path under which the content with the specified hash is stored.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public LPath GetPath(Hash hash)
        {
            var p = CalculatePath(hash);
            if (p.IsFile)
            {
                return p;
            }
            else
            {
                throw new ArgumentOutOfRangeException("hash", hash, "Hash not found");
            }
        }
    }
}
