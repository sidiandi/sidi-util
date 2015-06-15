using Sidi.IO;
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

        InternalHashProvider hashAlgorithm;
        LPath rootDirectory;

        const string importExtension = ".import";

        class InternalHashProvider : IHashProvider
        {
            ContentAddressableStorage storage;
            public IHashProvider hashProvider;
            Sidi.Caching.Cache cache;
            
            public InternalHashProvider(ContentAddressableStorage storage, IHashProvider hashProvider)
            {
                this.storage = storage;
                this.hashProvider = hashProvider;
                this.cache = new Caching.Cache(storage.rootDirectory.CatDir("hash"));
            }

            public Hash Get(IFileSystemInfo file)
            {
                var fv = FileVersion.Get(file);
                object hashObject = null;
                if (cache.TryGetValue(fv, out hashObject))
                {
                    var hash = (Hash)hashObject;
                    return hash;
                }
                else
                {
                    return cache.GetCached(fv, _ => GetHashAndImport(_));
                }
            }

            public Hash Get(Stream stream)
            {
                return hashProvider.Get(stream);
            }

            class CopyStream : Stream
            {
                Stream input;
                Stream copyDestination;
                
                public CopyStream(Stream input, Stream copyDestination)
                {
                    this.input = input;
                    this.copyDestination = copyDestination;
                }

                public override bool CanRead
                {
                    get { return input.CanRead; }
                }

                public override bool CanSeek
                {
                    get { return false; }
                }

                public override bool CanWrite
                {
                    get { return false; }
                }

                public override void Flush()
                {
                    throw new NotImplementedException();
                }

                public override long Length
                {
                    get { throw new NotImplementedException(); }
                }

                public override long Position
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                    set
                    {
                        throw new NotImplementedException();
                    }
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    var bytesRead = input.Read(buffer, offset, count);
                    if (bytesRead > 0)
                    {
                        copyDestination.Write(buffer, offset, bytesRead);
                    }
                    return bytesRead;
                }

                public override long Seek(long offset, SeekOrigin origin)
                {
                    throw new NotImplementedException();
                }

                public override void SetLength(long value)
                {
                    throw new NotImplementedException();
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    throw new NotImplementedException();
                }
            }

            Hash GetHashAndImport(FileVersion fv)
            {
                var p = new LPath(fv.Path);
                var tempFile = storage.GetTempFile();
                try
                {
                    Hash hash = null;
                    using (var write = tempFile.OpenWrite())
                    {
                        using (var read = p.OpenRead())
                        {
                            hash = Get(read);
                        }
                    }
                    var dest = storage.CalculatePath(hash).CatName(importExtension);
                    dest.EnsureParentDirectoryExists();
                    tempFile.Move(dest);
                    return hash;
                }
                finally
                {
                    tempFile.EnsureFileNotExists();
                }
            }
        }
        
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
            this.hashAlgorithm = new InternalHashProvider(this, hashAlgorithm);
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
        public WriteResult Write(IFileSystemInfo content)
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
                var importTempFile = dest.CatName(importExtension);
                importTempFile.EnsureParentDirectoryExists();
                try
                {
                    if (importTempFile.IsFile)
                    {
                    }
                    else
                    {
                        content.FullName.CopyFile(importTempFile);
                    }
                    importTempFile.Move(dest);
                    added = true;
                }
                finally
                {
                    importTempFile.EnsureFileNotExists();
                }
            }
            return new WriteResult(hash, added);
        }

        public WriteResult ImportHardlink(LPath content)
        {
            var hash = hashAlgorithm.Get(content.Info);
            bool added;
            if (Contains(hash))
            {
                added = false;
            }
            else
            {
                var dest = CalculatePath(hash);
                dest.EnsureParentDirectoryExists();
                content.CreateHardLink(dest);
                added = true;
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
            var hash = hashAlgorithm.hashProvider.Get(tempFile.Info);
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
            return rootDirectory.CatDir(
                new[]{
                hash.Value.Skip(0).Take(1),
                hash.Value.Skip(1).Take(1),
                hash.Value.Skip(2)
                }.Select(_ => _.HexString()));
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
