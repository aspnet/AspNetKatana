using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Microsoft.Owin.Compression.Storage
{
    public class DefaultCompressedStorage : ICompressedStorage
    {
        private string _storagePath;
        private FileStream _lockFile;

        public void Initialize()
        {
            // TODO: guard against many things, including re-execution or 
            var basePath = Path.Combine(Path.GetTempPath(), "MsOwinCompression");
            _storagePath = Path.Combine(basePath, Guid.NewGuid().ToString("n"));
            var lockPath = Path.Combine(_storagePath, "_lock");
            Directory.CreateDirectory(_storagePath);
            _lockFile = new FileStream(lockPath, FileMode.Create, FileAccess.Write, FileShare.None);

            ThreadPool.QueueUserWorkItem(_ => CleanupReleasedStorage(basePath));
        }
        
        public void Dispose()
        {
            // TODO: implement ~finalizer, etc

            ItemHandle[] items;
            lock (_itemsLock)
            {
                items = _items.Values.ToArray();
                _items.Clear();
            }

            var exceptions = new List<Exception>();
            foreach (var item in items)
            {
                try
                {
                    item.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            try
            {
                _lockFile.Close();
                _lockFile = null;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            try
            {
                Directory.Delete(_storagePath, true);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            if (exceptions.Count != 0)
            {
                throw new AggregateException(exceptions);
            }
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

        private readonly IDictionary<CompressedKey, ItemHandle> _items = new Dictionary<CompressedKey, ItemHandle>(CompressedKey.CompressedKeyComparer);
        private readonly object _itemsLock = new object();

        public ICompressedItemHandle Open(CompressedKey key)
        {
            lock (_itemsLock)
            {
                ItemHandle handle;
                if (_items.TryGetValue(key, out handle))
                {
                    return handle.Clone();
                }
                return null;
            }
        }

        public ICompressedItemBuilder Create(CompressedKey key)
        {
            // TODO: break down into buckets to avoid files-per-folder limits
            var physicalPath = Path.Combine(_storagePath, Guid.NewGuid().ToString("n"));
            return new ItemBuilder(key, physicalPath);
        }

        public ICompressedItemHandle Commit(ICompressedItemBuilder builder)
        {
            var itemBuilder = (ItemBuilder)builder;
            var key = itemBuilder.Key;
            var item = new Item
            {
                PhysicalPath = itemBuilder.PhysicalPath,
                CompressedLength = itemBuilder.Stream.Length
            };
            itemBuilder.Stream.Close();

            var handle = new ItemHandle(item);
            AddHandleInDictionary(key, handle);
            return handle;
        }

        private void AddHandleInDictionary(CompressedKey key, ItemHandle handle)
        {
            lock (_itemsLock)
            {
                ItemHandle addingHandle = handle.Clone();
                ItemHandle existingHandle;
                if (_items.TryGetValue(key, out existingHandle))
                {
                    existingHandle.Dispose();
                }
                _items[key] = addingHandle;
            }
        }

        class ItemBuilder : ICompressedItemBuilder
        {
            public ItemBuilder(CompressedKey key, string physicalPath)
            {
                Key = key;
                PhysicalPath = physicalPath;
                Stream = new FileStream(PhysicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            }

            public CompressedKey Key { get; private set; }
            public string PhysicalPath { get; private set; }
            public Stream Stream { get; private set; }
        }

        class Item
        {
            private int _references;

            public string PhysicalPath { get; set; }

            public long CompressedLength { get; set; }

            public void AddReference()
            {
                Interlocked.Increment(ref _references);
            }

            public void Release()
            {
                if (Interlocked.Decrement(ref _references) == 0)
                {
                    File.Delete(PhysicalPath);
                }
            }
        }

        class ItemHandle : ICompressedItemHandle
        {
            private Item _item;
            private bool _disposed;

            public ItemHandle(Item item)
            {
                item.AddReference();
                _item = item;
            }

            ~ItemHandle()
            {
                Dispose(false);
            }

            public ItemHandle Clone()
            {
                return new ItemHandle(_item);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    var item = Interlocked.Exchange(ref _item, null);
                    if (item != null)
                    {
                        item.Release();
                    }
                    _disposed = true;
                }
            }

            public string PhysicalPath { get { return _item.PhysicalPath; } }
            public long CompressedLength { get { return _item.CompressedLength; } }
        }
    }
}