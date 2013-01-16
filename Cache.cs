using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace TankIconMaker
{
    /// <summary>A strongly-typed wrapper around <see cref="WeakReference"/>.</summary>
    struct WeakReference<T> where T : class
    {
        private WeakReference _ref;
        public WeakReference(T value) { _ref = new WeakReference(value); }
        public T Target { get { return _ref == null ? null : (T) _ref.Target; } }
        public bool IsAlive { get { return _ref != null && _ref.IsAlive; } }
    }

    /// <summary>Base class for all cache entries.</summary>
    abstract class CacheEntry
    {
        /// <summary>Ensures that the entry is up-to-date, reloading the data if necessary. Not called again within a short timeout period.</summary>
        public abstract void Refresh();
        /// <summary>Gets the approximate size of this entry, in the same units as the <see cref="Cache.MaximumSize"/> property.</summary>
        public abstract long Size { get; }
    }

    /// <summary>
    /// Implements a strongly-typed cache which stores all entries up to a certain size, and above that evicts little-used items randomly.
    /// Evicted items will remain accessible, however, until the garbage collector actually collects them. All public methods are thread-safe.
    /// </summary>
    sealed class Cache<TKey, TEntry> where TEntry : CacheEntry
    {
        /// <summary>Keeps track of various information related to the cache entry, as well as a weak and, optionally, a strong reference to it.</summary>
        private class Container { public WeakReference<TEntry> Weak; public TEntry Strong; public int UseCount; public DateTime ValidStamp; }
        /// <summary>The actual keyed cache.</summary>
        private Dictionary<TKey, Container> _cache = new Dictionary<TKey, Container>();
        /// <summary>The root for all strongly-referenced entries.</summary>
        private HashSet<Container> _strong = new HashSet<Container>();

        /// <summary>Incremented every time an entry is looked up and an existing entry is already available.</summary>
        public int Hits { get; private set; }
        /// <summary>Incremented every time an entry is looked up but a new entry must be created.</summary>
        public int Misses { get; private set; }
        /// <summary>Incremented every time a strong reference is evicted due to the cache size going over the quota.</summary>
        public int Evictions { get; private set; }
        /// <summary>The maximum total size of the strongly-referenced entries that the cache is allowed to have. Units are up to <typeparamref name="TEntry"/>.</summary>
        public long MaximumSize { get; set; }
        /// <summary>The total size of the strongly-referenced entries that the cache currently has. Units are up to <typeparamref name="TEntry"/>.</summary>
        public long CurrentSize { get; set; }

        /// <summary>Gets an entry associated with the specified key. Returns a valid entry regardless of whether it was in the cache.</summary>
        /// <param name="key">The key that the entry is identified by.</param>
        /// <param name="createEntry">The function that instantiates a new entry in case there is no cached entry available.</param>
        public TEntry GetEntry(TKey key, Func<TEntry> createEntry)
        {
            var now = DateTime.UtcNow;
            lock (_cache)
            {
                Container c;
                if (!_cache.TryGetValue(key, out c))
                    _cache[key] = c = new Container();

                // Gets are counted to prioritize eviction; the count is maintained even if the weak reference gets GC’d
                c.UseCount++;

                // Retrieve the actual entry and ensure it’s up-to-date
                long wasSize = 0;
                var entry = c.Weak.Target; // grab a strong reference, if any
                if (entry == null)
                {
                    Misses++;
                    entry = createEntry();
                    entry.Refresh();
                    c.ValidStamp = now;
                    c.Weak = new WeakReference<TEntry>(entry);
                }
                else
                {
                    Hits++;
                    wasSize = entry.Size;
                    if (now - c.ValidStamp > TimeSpan.FromSeconds(1))
                    {
                        entry.Refresh();
                        c.ValidStamp = now;
                    }
                }

                // Update the strong reference list
                long nowSize = entry.Size;
                if (c.Strong != null)
                    CurrentSize += nowSize - wasSize;
                else if (Rnd.NextDouble() > Math.Min(0.5, CurrentSize / MaximumSize))
                {
                    c.Strong = entry;
                    _strong.Add(c);
                    CurrentSize += nowSize;
                }
                if (CurrentSize > MaximumSize)
                    evictStrong();

                return entry;
            }
        }

        /// <summary>Evicts entries from the strongly-referenced cache until the <see cref="MaximumSize"/> is satisfied.</summary>
        private void evictStrong()
        {
            while (CurrentSize > MaximumSize && _strong.Count > 0)
            {
                // Pick two random entries and evict the one that's been used the least.
                var item1 = _strong.Skip(Rnd.Next(_strong.Count)).First();
                var item2 = _strong.Skip(Rnd.Next(_strong.Count)).First();
                if (item1.UseCount < item2.UseCount)
                {
                    _strong.Remove(item1);
                    CurrentSize -= item1.Strong.Size;
                    item1.Strong = null;
                }
                else
                {
                    _strong.Remove(item2);
                    CurrentSize -= item2.Strong.Size;
                    item2.Strong = null;
                }
                Evictions++;
            }
        }

        /// <summary>
        /// Removes all the metadata associated with entries which have been evicted and garbage-collected. Note that this wipes
        /// the metadata which helps ensure that frequently evicted and re-requested items eventually stop being evicted from the strong cache.
        /// </summary>
        public void Collect()
        {
            lock (_cache)
            {
                var removeKeys = _cache.Where(kvp => !kvp.Value.Weak.IsAlive).Select(kvp => kvp.Key).ToArray();
                foreach (var key in removeKeys)
                    _cache.Remove(key);
            }
        }

        /// <summary>
        /// Empties the cache completely, resetting it to blank state.
        /// </summary>
        public void Clear()
        {
            lock (_cache)
            {
                _cache.Clear();
                _strong.Clear();
                Hits = Misses = Evictions = 0;
            }
        }
    }

    /// <summary>
    /// Implements a cache for <see cref="ZipFile"/> instances.
    /// </summary>
    static class ZipCache
    {
        private static Cache<string, ZipCacheEntry> _cache = new Cache<string, ZipCacheEntry> { MaximumSize = 1 * 1024 * 1024 };

        /// <summary>Empties the cache completely, resetting it to blank state.</summary>
        public static void Clear() { _cache.Clear(); }

        /// <summary>
        /// Opens a file inside a zip file, returning the stream for reading its contents. The stream must be disposed after use.
        /// Returns null if the zip file or the file inside it does not exist.
        /// </summary>
        public static Stream GetZipFileStream(CompositePath path)
        {
            var zipfile = _cache.GetEntry(path.File.ToLowerInvariant(), () => new ZipCacheEntry(path.File)).Zip;
            if (zipfile == null)
                return null;
            var entry = zipfile.GetEntry(path.InnerFile.Replace('\\', '/'));
            if (entry == null)
                return null;
            else
                return zipfile.GetInputStream(entry);
        }
    }

    /// <summary>
    /// Implements a zip file cache entry.
    /// </summary>
    sealed class ZipCacheEntry : CacheEntry
    {
        public ZipFile Zip { get; private set; }

        private string _path;
        private DateTime _lastModified;

        public ZipCacheEntry(string path)
        {
            _path = path;
        }

        public override void Refresh()
        {
            if (!File.Exists(_path))
                Zip = null;
            else
                try
                {
                    var modified = File.GetLastWriteTimeUtc(_path);
                    if (_lastModified == modified)
                        return;
                    _lastModified = modified;
                    Zip = new ZipFile(_path);
                }
                catch (FileNotFoundException) { Zip = null; }
                catch (DirectoryNotFoundException) { Zip = null; }
        }

        public override long Size
        {
            get { return IntPtr.Size * 6 + (Zip == null ? 0 : (IntPtr.Size * Zip.Count)); } // very approximate
        }
    }

    /// <summary>
    /// Implements a cache for images loaded from a variety of formats and, optionally, from inside zip files.
    /// </summary>
    static class ImageCache
    {
        private static Cache<string, ImageEntry> _cache = new Cache<string, ImageEntry> { MaximumSize = 10 * 1024 * 1024 };

        /// <summary>Empties the cache completely, resetting it to blank state.</summary>
        public static void Clear() { _cache.Clear(); }

        /// <summary>Retrieves an image which may optionally be stored inside a zip file.</summary>
        public static BitmapRam GetImage(CompositePath path)
        {
            return _cache.GetEntry(path.ToString(),
                () => path.InnerFile == null ? (ImageEntry) new FileImageEntry(path.File) : new ZipImageEntry(path)).Image;
        }
    }

    abstract class ImageEntry : CacheEntry
    {
        public BitmapRam Image;

        protected void LoadImage(Stream file, string extension)
        {
            if (extension == ".tga")
                Image = Targa.Load(file);
            else
            {
                if (!file.CanSeek) // http://stackoverflow.com/questions/14286462/how-to-use-bitmapdecoder-with-a-non-seekable-stream
                    file = new MemoryStream(file.ReadAllBytes());
                Image = BitmapDecoder.Create(file, BitmapCreateOptions.None, BitmapCacheOption.None).Frames[0].ToBitmapRam();
            }
            Image.MarkReadOnly();
        }

        public override long Size
        {
            get { return 10 + (Image == null ? 0 : (Image.Width * Image.Height * 4)); } // very approximate
        }
    }

    sealed class ZipImageEntry : ImageEntry
    {
        private CompositePath _path;

        public ZipImageEntry(CompositePath path)
        {
            _path = path;
        }

        public override void Refresh()
        {
            using (var stream = ZipCache.GetZipFileStream(_path))
                if (stream == null)
                    Image = null;
                else
                    LoadImage(stream, Path.GetExtension(_path.InnerFile).ToLowerInvariant());
        }
    }

    sealed class FileImageEntry : ImageEntry
    {
        private string _path;
        private DateTime _lastModified;

        public FileImageEntry(string path)
        {
            _path = path;
        }

        public override void Refresh()
        {
            if (!File.Exists(_path))
                Image = null;
            else
                try
                {
                    var modified = File.GetLastWriteTimeUtc(_path);
                    if (_lastModified == modified)
                        return;
                    _lastModified = modified;
                    using (var file = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        LoadImage(file, Path.GetExtension(_path).ToLowerInvariant());
                }
                catch (FileNotFoundException) { Image = null; }
                catch (DirectoryNotFoundException) { Image = null; }
        }
    }
}
