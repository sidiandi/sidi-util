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
    public class HashAddressableStorage : Sidi.IO.IHashAddressableStorage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        LPath rootDirectory;

        public HashAddressableStorage(LPath rootDirectory)
        {
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("rootDirectory");
            }

            this.rootDirectory = rootDirectory;
            rootDirectory.EnsureDirectoryExists();
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

        LPath GetTempFile()
        {
            return rootDirectory.CatDir(Path.GetRandomFileName());
        }

        /// <summary>
        /// Writes the contents of file content to the storage.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public void Write(Hash key, LPath content)
        {
            var dest = CalculatePath(key);
            dest.EnsureParentDirectoryExists();
            content.CopyFile(dest, options: new CopyFileOptions { Overwrite = true });
        }

        class WriteStream : FileStream
        {
            public WriteStream(LPath tempPath, LPath finalPath)
            : base(tempPath, FileMode.CreateNew, FileAccess.Write)
            {
                this.tempPath = tempPath;
                this.finalPath = finalPath;
            }

            LPath finalPath;
            LPath tempPath;

            public override void Close()
            {
 	            base.Close();
                if (tempPath != null)
                {
                    finalPath.EnsureParentDirectoryExists();
                    finalPath.EnsureFileNotExists();
                    tempPath.Move(finalPath);
                    tempPath = null;
                }
            }
        }
        
        /// <summary>
        /// Returns a stream that can be used to write to key.
        /// Call StorageWriteStream.Commit to commit the write to the storage
        /// </summary>
        /// <param name="key">storage address</param>
        /// <returns>stream that can be used to write to key</returns>
        public Stream Write(Hash key)
        {
            var dest = CalculatePath(key);
            var temp = GetTempFile();
            return new WriteStream(temp, dest);
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
        /// Copies the content addressed by the specified key to file destinationFile.
        /// </summary>
        /// <param name="key">storage address</param>
        /// <param name="destinationFiles">destination of the copy operation</param>
        /// <returns></returns>
        public void Read(Hash key, LPath destinationFile)
        {
            GetPath(key).CopyFile(destinationFile);
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
        LPath GetPath(Hash hash)
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

        public void Remove(Hash key)
        {
            var d = CalculatePath(key);
            d.EnsureFileNotExists();
        }

        public void Clear()
        {
            foreach (var c in rootDirectory.GetChildren())
            {
                c.EnsureNotExists();
            }
        }

        public bool TryGetInfo(Hash key, out StorageItemInfo info)
        {
            var p = CalculatePath(key);
            if (p.IsFile)
            {
                var fileInfo = p.Info;
                info = new StorageItemInfo(fileInfo.Length, fileInfo.LastWriteTimeUtc);
                return true;
            }
            else
            {
                info = null;
                return false;
            }
        }
    }
}
