using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using W = System.Windows.Media;
using WI = System.Windows.Media.Imaging;

namespace TankIconMaker
{
    /// <summary>
    /// Wrapper around a GDI Bitmap that allows access to its raw byte data. The API is intended to somewhat
    /// resemble that of the WPF BitmapSource. Intentionally supports just a single pixel format: 32bppArgb, aka Bgra32.
    /// </summary>
    sealed class BitmapGdi : IDisposable
    {
        private SharedPinnedByteArray _bytes;

        /// <summary>
        /// Creates a new, blank BitmapGdi with the specified width and height. The pixel format is fixed: 32bppArgb, aka Bgra32.
        /// </summary>
        public BitmapGdi(int width, int height)
        {
            init(width, height);
        }

        /// <summary>
        /// Creates a BitmapGdi by loading an image from the specified file and copying the pixel data, converting to 32bppArgb, aka Bgra32, if necessary.
        /// </summary>
        public BitmapGdi(string filename)
        {
            var image = Image.FromFile(filename);
            init(image.Width, image.Height);
            using (var g = Graphics.FromImage(Bitmap))
                g.DrawImageUnscaled(image, 0, 0);
        }

        private void init(int width, int height)
        {
            PixelWidth = width;
            PixelHeight = height;
            BackBufferStride = width * Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8;
            int padding = BackBufferStride % 4;
            BackBufferStride += (padding == 0) ? 0 : 4 - padding;
            _bytes = new SharedPinnedByteArray(BackBufferStride * height);
            Bitmap = new Bitmap(width, height, BackBufferStride, PixelFormat.Format32bppArgb, _bytes.Address);
            Bitmap.SetResolution(96, 96);
        }

        /// <summary>Gets the width of the image in pixels.</summary>
        public int PixelWidth { get; private set; }

        /// <summary>Gets the height of the image in pixels.</summary>
        public int PixelHeight { get; private set; }

        /// <summary>Gets the bitmap bit buffer. Writes to this array modify the image; writes to the image modify this array.</summary>
        public byte[] BackBytes { get { return _bytes.Bytes; } }

        /// <summary>Gets a pointer to the buffer containing the bitmap bit buffer.</summary>
        public IntPtr BackBuffer { get { return _bytes.Address; } }

        /// <summary>Gets the stride (the number of bytes to go one pixel down) of the bitmap.</summary>
        public int BackBufferStride { get; private set; }

        /// <summary>
        /// Gets the underlying Bitmap that this BitmapGdi wraps. USAGE WARNING:
        /// DO NOT use this if the BitmapGdi wrapping it may have gone out of scope
        /// and disposed of. This will cause intermittent issues - when the BitmapGdi
        /// gets GC'd. Use <see cref="GetBitmapCopy"/> or GC.KeepAlive the wrapper.
        /// </summary>
        public Bitmap Bitmap { get; private set; }

        /// <summary>
        /// Use this to create a new Bitmap that is a copy of the image stored in this
        /// BitmapGdi. This can be passed around safely, unlike the wrapped bitmap
        /// returned by <see cref="Bitmap"/>.
        /// </summary>
        public Bitmap GetBitmapCopy()
        {
            var bmp = new Bitmap(Bitmap);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImageUnscaled(Bitmap, 0, 0);
            return bmp;
        }

        /// <summary>Converts this bitmap to a WPF BitmapSource instance.</summary>
        public WI.BitmapSource ToWpf()
        {
            var writable = ToWpfWriteable();
            writable.Freeze();
            return writable;
        }

        /// <summary>Converts this bitmap to a modifiable WPF WriteableBitmap instance.</summary>
        public WI.WriteableBitmap ToWpfWriteable()
        {
            var writable = new WI.WriteableBitmap(PixelWidth, PixelHeight, Bitmap.HorizontalResolution, Bitmap.VerticalResolution, W.PixelFormats.Bgra32, null);
            writable.WritePixels(new System.Windows.Int32Rect(0, 0, PixelWidth, PixelHeight), BackBytes, BackBufferStride, 0);
            return writable;
        }

        /// <summary>Disposes of the underlying resources.</summary>
        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
            }
            if (_bytes != null)
            {
                _bytes.ReleaseReference();
                _bytes = null;
            }
        }
    }

    /// <summary>
    /// This class represents a byte array which is pinned to avoid relocation
    /// by the GC and implements reference counting.
    /// </summary>
    sealed class SharedPinnedByteArray
    {
        private GCHandle _handle;
        private int _refCount;
        private bool _destroyed;

        /// <summary>Gets the allocated byte array.</summary>
        public byte[] Bytes { get; private set; }

        /// <summary>Gets the length of the byte array.</summary>
        public int Length { get { return Bytes.Length; } }

        /// <summary>Gets an unmanaged address of the first (index 0) byte of the byte array.</summary>
        public IntPtr Address { get; private set; }

        /// <summary>Returns an unmanaged address of the specified byte in the byte array.</summary>
        public IntPtr AddressOf(int index) { return Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, index); }

        /// <summary>
        /// Creates a new pinned array of the specified size, that can be accessed through <see cref="Bytes"/>.
        /// One reference is automatically added; call <see cref="ReleaseReference"/> when finished using this array.
        /// </summary>
        /// <param name="length">The number of bytes that the pinned array should contain.</param>
        public SharedPinnedByteArray(int length)
        {
            Bytes = new byte[length];
            _handle = GCHandle.Alloc(Bytes, GCHandleType.Pinned);
            Address = Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, 0);
            _refCount++;
        }

        /// <summary>
        /// Adds a reference to this array. One reference is counted when the array is created. It is deleted when
        /// all references are released using <see cref="ReleaseReference"/>.
        /// </summary>
        public void AddReference()
        {
            _refCount++;
        }

        /// <summary>
        /// Releases a reference to this array. When there are none left, the array is unpinned and can get garbage-collected.
        /// Note that the array is released the moment the ref count drops to zero for the first time.
        /// </summary>
        public void ReleaseReference()
        {
            _refCount--;
            if (_refCount <= 0)
                destroy();
        }

        private void destroy()
        {
            if (!_destroyed)
            {
                _handle.Free();
                Bytes = null;
                _destroyed = true;
            }
        }

        ~SharedPinnedByteArray()
        {
            destroy();
        }
    }
}
