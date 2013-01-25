using System;
using System.IO;
using System.Threading;

namespace Microsoft.Owin.Compression.Storage
{
    public class DefaultCompressedStorage : ICompressedStorage
    {
        private string _storagePath;
        private FileStream _lockFile;

        public void Open()
        {
            // TODO: guard against many things, including re-execution or 
            var basePath = Path.Combine(Path.GetTempPath(), "MsOwinCompression");
            _storagePath = Path.Combine(basePath, Guid.NewGuid().ToString("n"));
            var lockPath = Path.Combine(_storagePath, "_lock");
            Directory.CreateDirectory(_storagePath);
            _lockFile = new FileStream(lockPath, FileMode.Create, FileAccess.Write, FileShare.None);

            ThreadPool.QueueUserWorkItem(_ => CleanupReleasedStorage(basePath));
        }

        public void Close()
        {
            _lockFile.Close();
            _lockFile = null;
            // TODO: this is probably not good enough
            Directory.Delete(_storagePath, true);
        }

        private void CleanupReleasedStorage(string basePath)
        {
            foreach (var directory in Directory.GetDirectories(basePath))
            {
                var directoryPath = Path.Combine(basePath, Path.GetFileName(directory));
                if (string.Equals(directoryPath, _storagePath, StringComparison.OrdinalIgnoreCase))
                {
                    // don't try to cleanup ourselves
                    continue;
                }
                var lockPath = Path.Combine(directoryPath, "_lock");
                if (File.Exists(lockPath))
                {
                    var lockInfo = new FileInfo(lockPath);
                    if (lockInfo.LastAccessTimeUtc > DateTime.UtcNow.Subtract(TimeSpan.FromHours(36)))
                    {
                        // less than a day and a half - don't try cleanup yet to avoid causing
                        // an exception if it's still in use
                        continue;
                    }
                    bool stillInUse = false;
                    try
                    {
                        File.Delete(lockPath);
                    }
                    catch
                    {
                        stillInUse = true;
                    }
                    if (stillInUse)
                    {
                        // can't delete - lock file still in use
                        continue;
                    }
                }
                Directory.Delete(directoryPath, true);
            }
        }

        public ICompressedEntry Lookup(CompressedKey key)
        {
            // TODO: not implemented
            return null; 
        }

        public ICompressedEntryBuilder Start(CompressedKey key)
        {
            // TODO: break down into buckets to avoid files-per-folder limits
            var physicalPath = Path.Combine(_storagePath, Guid.NewGuid().ToString("n"));
            return new EntryBuilder(this, key, physicalPath);
        }

        public ICompressedEntry Finish(CompressedKey key, ICompressedEntryBuilder builder)
        {
            var entryBuilder = (EntryBuilder)builder;
            var entry = new Entry(entryBuilder.PhysicalPath, entryBuilder.Stream.Length);
            entryBuilder.Stream.Close();
            return entry;
        }

        public void Abort(ICompressedEntryBuilder builder)
        {
            throw new System.NotImplementedException();
        }

        class EntryBuilder : ICompressedEntryBuilder
        {
            private readonly DefaultCompressedStorage _storage;
            private readonly CompressedKey _key;
            private readonly string _physicalPath;

            public EntryBuilder(DefaultCompressedStorage storage, CompressedKey key, string physicalPath)
            {
                _storage = storage;
                _key = key;
                _physicalPath = physicalPath;
                Stream = new FileStream(_physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            }

            public string PhysicalPath { get { return _physicalPath; } }
            public Stream Stream { get; private set; }
        }

        class Entry : ICompressedEntry
        {
            private readonly string _physicalPath;
            private readonly long _compressedLength;

            public Entry(string physicalPath, long compressedLength)
            {
                _physicalPath = physicalPath;
                _compressedLength = compressedLength;
            }

            public string PhysicalPath { get { return _physicalPath; } }
            public long CompressedLength { get { return _compressedLength; } }
        }

        private string GetTempFileName()
        {
            throw new System.NotImplementedException();
        }
    }
}