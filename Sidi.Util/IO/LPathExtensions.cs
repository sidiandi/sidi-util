using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sidi.IO
{
    public static class LPathExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void WriteAllText(this LPath path, string text)
        {
            path.EnsureParentDirectoryExists();
            using (var w = path.WriteText())
            {
                w.Write(text);
            }
        }

        public static string ReadAllText(this LPath path)
        {
            using (var r = path.ReadText())
            {
                return r.ReadToEnd();
            }
        }

        public static IEnumerable<string> ReadAllLines(this LPath path)
        {
            return path.Read(Sidi.Extensions.StringExtensions.ReadLines);
        }

        public static void Move(this LPath source, LPath destination)
        {
            source.FileSystem.Move(source, destination);
        }

        public static void RemoveDirectory(this LPath path)
        {
            path.FileSystem.RemoveDirectory(path);
        }

        public static void DeleteFile(this LPath path)
        {
            path.FileSystem.DeleteFile(path);
        }

        public static void CopyFile(
            this LPath source,
            LPath dest,
            IProgress<CopyFileProgress> progress = null,
            System.Threading.CancellationToken cancellationToken = new CancellationToken(),
            CopyFileOptions options = null)
        {
            source.FileSystem.CopyFile(source, dest, progress, cancellationToken, options);
        }

        public static void CreateHardLink(this LPath existingFilename, LPath newHardlinkFilename)
        {
            existingFilename.FileSystem.CreateHardLink(newHardlinkFilename, existingFilename);
        }

        public static void CopyOrHardLink(this LPath source, LPath destination)
        {
            destination.EnsureParentDirectoryExists();
            if (LPath.IsSameFileSystem(source, destination))
            {
                source.CreateHardLink(destination);
            }
            else
            {
                source.CopyFile(destination);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPath"></param>
        public static LPath GetAdministrativeShareUnc(this LPath localPath)
        {
            return LPath.GetUncRoot(System.Environment.MachineName, localPath.DriveLetter + "$").CatDir(localPath.Parts);
        }

        public static T Read<T>(this LPath path, Func<TextReader, T> reader)
        {
            using (var r = path.ReadText())
            {
                try
                {
                    return reader(r);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(String.Format("Exception at position {1} while reading {0}", path, r.BaseStream.Position), ex);
                }
            }
        }

        class StreamReaderEnumerator<T> : IEnumerator<T>
        {
            IEnumerator<T> enumerator;
            LPath path;
            TextReader textReader;
            Func<TextReader, IEnumerable<T>> reader;

            public StreamReaderEnumerator(LPath path, Func<TextReader, IEnumerable<T>> reader)
            {
                this.path = path;
                this.reader = reader;
                this.textReader = path.ReadText();
                this.enumerator = this.reader(textReader).GetEnumerator();
            }

            public T Current
            {
                get { return enumerator.Current; }
            }

            public void Dispose()
            {
                enumerator.Dispose();
                textReader.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return enumerator.Current; }
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                textReader.Close();
                this.textReader = path.ReadText();
                this.enumerator = this.reader(textReader).GetEnumerator();
            }
        }
        
        class StreamReaderEnumerable<T> : IEnumerable<T>
        {
            public StreamReaderEnumerable(LPath path, Func<TextReader, IEnumerable<T>> reader)
            {
                this.path = path;
                this.reader = reader;
            }

            LPath path;
            Func<TextReader, IEnumerable<T>> reader;

            public IEnumerator<T> GetEnumerator()
            {
                return new StreamReaderEnumerator<T>(path, reader);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return new StreamReaderEnumerator<T>(path, reader);
            }
        }

        public static IEnumerable<T> Read<T>(this LPath path, Func<TextReader, IEnumerable<T>> reader)
        {
            return new StreamReaderEnumerable<T>(path, reader);
        }

        public static void Write(this LPath path, Action<TextWriter> writer)
        {
            using (var r = path.WriteText())
            {
                try
                {
                    writer(r);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(String.Format("Exception at position {1} while writing {0}", path, r.BaseStream.Position), ex);
                }
            }
        }

        public static T Read<T>(this LPath path, Func<Stream, T> processor)
        {
            using (var r = path.OpenRead())
            {
                try
                {
                    return processor(r);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(String.Format("Exception at position {1} while reading {0}", path, r.Position), ex);
                }
            }
        }

        class StreamEnumerator<T> : IEnumerator<T>
        {
            IEnumerator<T> enumerator;
            LPath path;
            Stream textReader;
            Func<Stream, IEnumerable<T>> reader;

            public StreamEnumerator(LPath path, Func<Stream, IEnumerable<T>> reader)
            {
                this.path = path;
                this.reader = reader;
                this.textReader = path.OpenRead();
                this.enumerator = this.reader(textReader).GetEnumerator();
            }

            public T Current
            {
                get { return enumerator.Current; }
            }

            public void Dispose()
            {
                enumerator.Dispose();
                textReader.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return enumerator.Current; }
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                textReader.Close();
                this.textReader = path.OpenRead();
                this.enumerator = this.reader(textReader).GetEnumerator();
            }
        }

        class StreamEnumerable<T> : IEnumerable<T>
        {
            public StreamEnumerable(LPath path, Func<Stream, IEnumerable<T>> reader)
            {
                this.path = path;
                this.reader = reader;
            }

            LPath path;
            Func<Stream, IEnumerable<T>> reader;

            public IEnumerator<T> GetEnumerator()
            {
                return new StreamEnumerator<T>(path, reader);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return new StreamEnumerator<T>(path, reader);
            }
        }

        /// <summary>
        /// Manages opening and closing the underlying Stream correctly when calling reader on the file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<T> Read<T>(this LPath path, Func<Stream, IEnumerable<T>> reader)
        {
            return new StreamEnumerable<T>(path, reader);
        }
    }
}
