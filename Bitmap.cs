using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
using D = System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace TankIconMaker
{
    abstract unsafe class BitmapBase : IDisposable
    {
        /// <summary>Gets the width of the image in pixels.</summary>
        public int Width { get; protected set; }
        /// <summary>Gets the height of the image in pixels.</summary>
        public int Height { get; protected set; }
        /// <summary>Gets the stride (the number of bytes to go one pixel down) of the bitmap.</summary>
        public int Stride { get; protected set; }
        /// <summary>Gets a pointer to the pixel data. This value is only valid between calls to <see cref="Acquire"/> and <see cref="Release"/>.</summary>
        public byte* Data { get; private set; }
        /// <summary>Gets a pointer to the first byte outside of the visible pixel data. This value is only valid between calls to <see cref="Acquire"/> and <see cref="Release"/>.</summary>
        public byte* DataEnd { get; private set; }
        /// <summary>Gets a pointer to the first byte outside of the writable pixel buffer. This value is only valid between calls to <see cref="Acquire"/> and <see cref="Release"/>.</summary>
        public byte* DataEndStride { get; private set; }

        #region Acquire / Release

        protected abstract IntPtr Acquire();
        protected abstract void Release();

        private int _acquiresRead = 0;
        private int _acquiresWrite = 0;
        private Thread _thread = null;
        private bool _multithreadedReading = false;

        private void acquire(bool write)
        {
            if (_acquiresRead == int.MinValue)
                throw new ObjectDisposedException("BitmapBase");
            if (write && IsReadOnly)
                throw new InvalidOperationException("Cannot use this bitmap for writing because it is marked as read-only.");
            if (_acquiresRead == 0 && _acquiresWrite == 0)
            {
                // First concurrent operation: always allow
                _thread = Thread.CurrentThread;
                // And also do the actual Acquire
                Data = (byte*) Acquire();
                DataEnd = Data + (Height - 1) * Stride + Width * 4;
                DataEndStride = Data + Height * Stride;
            }
            else if (_multithreadedReading)
            {
                if (write)
                    throw new InvalidOperationException("Cannot use this bitmap for writing because there exist several concurrent readers.");
            }
            else
            {
                // We have a single thread that is reading, writing, or both
                if (_thread != Thread.CurrentThread)
                {
                    // Same thread is allowed and nothing else is required. But this is a different thread.
                    if (!write && _acquiresWrite == 0)
                        _multithreadedReading = true;
                    else
                        throw new InvalidOperationException("Cannot use this bitmap because another thread is currently using it.");
                }
            }

            if (write)
                _acquiresWrite++;
            else
                _acquiresRead++;
        }

        private void release(bool write)
        {
            if (_acquiresRead == int.MinValue) // a manual dispose followed by a manual release: tell the programmer they messed up
                throw new ObjectDisposedException("BitmapBase");

            if (write)
                _acquiresWrite--;
            else
                _acquiresRead--;

            if (_acquiresRead == 0 && _acquiresWrite == 0)
            {
                Release();
                Data = DataEnd = DataEndStride = null;
                _multithreadedReading = false;
                _thread = null;
            }
            else if (_acquiresRead < 0 || _acquiresWrite < 0)
                throw new Exception("4109876");
        }

        public BitmapReadReleaser UseRead()
        {
            return new BitmapReadReleaser(this);
        }

        public BitmapWriteReleaser UseWrite()
        {
            return new BitmapWriteReleaser(this);
        }

        public struct BitmapReadReleaser : IDisposable
        {
            private BitmapBase _bmp;
            public BitmapReadReleaser(BitmapBase bitmap) { _bmp = bitmap; _bmp.acquire(write: false); }
            public void Dispose() { _bmp.release(write: false); }
        }

        public struct BitmapWriteReleaser : IDisposable
        {
            private BitmapBase _bmp;
            public BitmapWriteReleaser(BitmapBase bitmap) { _bmp = bitmap; _bmp.acquire(write: true); }
            public void Dispose() { _bmp.release(write: true); }
        }

        public virtual void Dispose()
        {
            if (_acquiresRead > 0 || _acquiresWrite > 0)
                Release();
            _acquiresRead = int.MinValue;
        }

        ~BitmapBase()
        {
            Dispose();
        }

        #endregion

        public bool IsReadOnly { get; private set; }

        public void MarkReadOnly()
        {
            IsReadOnly = true;
        }

        public void CopyPixelsFrom(byte* srcData, int srcWidth, int srcHeight, int srcStride, bool upsideDown = false)
        {
            using (this.UseWrite())
            {
                int copyBytes = srcWidth <= this.Width ? Math.Min(srcStride, this.Stride) : (this.Width * 4);
                if (!upsideDown)
                {
                    byte* dest = srcData;
                    byte* src = this.Data;
                    byte* srcDataEnd = srcData + srcWidth * srcHeight * 4;
                    do
                    {
                        Ut.MemCpy(dest, src, copyBytes);
                        dest += srcStride;
                        src += this.Stride;
                    }
                    while (dest < srcDataEnd && src < this.DataEnd);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void CopyPixelsFrom(byte[] srcData, int srcWidth, int srcHeight, int srcStride, bool upsideDown = false)
        {
            fixed (byte* srcDataPtr = srcData)
                CopyPixelsFrom(srcData, srcWidth, srcHeight, srcStride, upsideDown);
        }

        public void CopyPixelsFrom(BitmapBase source)
        {
            using (source.UseRead())
                CopyPixelsFrom(source.Data, source.Width, source.Height, source.Stride);
        }

        public void CopyPixelsFrom(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            source.CopyPixels(new Int32Rect(0, 0, Math.Min(Width, source.PixelWidth), Math.Min(Height, source.PixelHeight)),
                (IntPtr) Data, Stride * Height, Stride);
        }

        public BitmapBase AsWritable()
        {
            return IsReadOnly ? ToBitmapRam() : this;
        }

        public BitmapBase AsWritableSame()
        {
            return IsReadOnly ? ToBitmapSame() : this;
        }

        public BitmapRam ToBitmapRam()
        {
            var result = new BitmapRam(Width, Height);
            result.CopyPixelsFrom(this);
            return result;
        }

        public BitmapGdi ToBitmapGdi()
        {
            var result = new BitmapGdi(Width, Height);
            result.CopyPixelsFrom(this);
            return result;
        }

        public BitmapWpf ToBitmapWpf()
        {
            var result = new BitmapWpf(Width, Height);
            result.CopyPixelsFrom(this);
            return result;
        }

        public abstract BitmapBase ToBitmapSame();

        public void DrawImage(BitmapBase image, int destX = 0, int destY = 0)
        {
            DrawImage(image, destX, destY, 0, 0, image.Width, image.Height);
        }

        public void DrawImage(BitmapBase image, int destX, int destY, int srcX, int srcY, int width, int height)
        {
            using (UseWrite())
            using (image.UseRead())
            {
                if (width <= 0 || height <= 0)
                    return;
                if (destX < 0)
                {
                    srcX -= destX;
                    width += destX;
                    destX = 0;
                }
                if (destY < 0)
                {
                    srcY -= destY;
                    height += destY;
                    destY = 0;
                }
                if (srcX >= image.Width || srcY >= image.Height)
                    return;
                if (srcX < 0)
                {
                    destX -= srcX;
                    width += srcX;
                    srcX = 0;
                }
                if (srcY < 0)
                {
                    destY -= srcY;
                    height += srcY;
                    srcY = 0;
                }
                if (destX >= Width || destY >= Height)
                    return;
                if (destX + width > Width)
                    width = Width - destX;
                if (destY + height > Height)
                    height = Height - destY;
                if (srcX + width > image.Width)
                    width = image.Width - srcX;
                if (srcY + height > image.Height)
                    height = image.Height - srcY;
                if (width <= 0 || height <= 0) // cannot be negative at this stage, but just in case...
                    return;

                byte* dest = Data + destY * Stride + destX * 4;
                byte* src = image.Data + srcY * image.Stride + srcX * 4;

                for (int y = 0; y < height; y++, dest += Stride, src += image.Stride)
                {
                    byte* d = dest;
                    byte* s = src;
                    byte* e = d + width * 4;
                    do
                    {
                        byte sa = *(s + 3);
                        byte da = *(d + 3);
                        if (sa == 255 || da == 0)
                        {
                            *(int*) d = *(int*) s;
                        }
                        else if (da == 255)
                        {
                            // green
                            *(d + 1) = (byte) ((*(s + 1) * sa + *(d + 1) * (255 - sa)) >> 8);
                            // red and blue
                            *(uint*) d = (*(uint*) d & 0xFF00FF00u) | (((((*(uint*) s) & 0x00FF00FFu) * sa + ((*(uint*) d) & 0x00FF00FFu) * (uint) (255 - sa)) >> 8) & 0x00FF00FFu);
                            // leave alpha untouched, it's already 255
                        }
                        else if (sa != 0)
                        {
                            // alpha
                            *(d + 3) = (byte) (da + (((255 - da) * sa) >> 8));
                            byte oa = (byte) (*(d + 3) * 255);
                            // rgb
                            *(d + 0) = (byte) ((*(s + 0) * sa + ((*(d + 0) * da * (255 - sa)) >> 8)) / oa);
                            *(d + 1) = (byte) ((*(s + 1) * sa + ((*(d + 1) * da * (255 - sa)) >> 8)) / oa);
                            *(d + 2) = (byte) ((*(s + 2) * sa + ((*(d + 2) * da * (255 - sa)) >> 8)) / oa);
                        }
                        d += 4;
                        s += 4;
                    }
                    while (d < e);
                }
            }
        }

        /// <summary>Applies a "colorize" effect to this image.</summary>
        /// <param name="hue">The hue of the color to apply, 0..359</param>
        /// <param name="saturation">The saturation of the color to apply, 0..1</param>
        /// <param name="lightness">A lightness adjustment, -1..1</param>
        /// <param name="alpha">Overall strength of the effect, 0..1. A value of 0 keeps only the original image.</param>
        public void Colorize(int hue, double saturation, double lightness, double alpha)
        {
            using (UseWrite())
            {
                // http://stackoverflow.com/a/9177602/33080
                var color = Ut.BlendColors(Color.FromRgb(128, 128, 128), ColorHSV.FromHSV(hue, 100, 100).ToColorWpf(), saturation);
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        double pixel = Math.Max(*ptr, Math.Max(*(ptr + 1), *(ptr + 2))) / 255.0;
                        double position = lightness >= 0 ? (2 * (1 - lightness) * (pixel - 1) + 1) : 2 * (1 + lightness) * (pixel) - 1;
                        *ptr = (byte) (*ptr * (1 - alpha) + (position < 0 ? color.B * (position + 1) : (color.B * (1 - position) + 255 * position)) * alpha);
                        ptr++;
                        *ptr = (byte) (*ptr * (1 - alpha) + (position < 0 ? color.G * (position + 1) : (color.G * (1 - position) + 255 * position)) * alpha);
                        ptr++;
                        *ptr = (byte) (*ptr * (1 - alpha) + (position < 0 ? color.R * (position + 1) : (color.R * (1 - position) + 255 * position)) * alpha);
                        ptr += 2;
                    }
                }
            }
        }

        public void PreMultiply()
        {
            using (UseWrite())
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        byte alpha = ptr[3];
                        ptr[0] = (byte) ((ptr[0] * alpha) / 255);
                        ptr[1] = (byte) ((ptr[1] * alpha) / 255);
                        ptr[2] = (byte) ((ptr[2] * alpha) / 255);
                        ptr += 4;
                    }
                }
        }

        public void UnPreMultiply()
        {
            using (UseWrite())
                for (int y = 0; y < Height; y++)
                {
                    byte* ptr = Data + y * Stride;
                    byte* end = ptr + Width * 4;
                    while (ptr < end)
                    {
                        byte alpha = ptr[3];
                        if (alpha > 0)
                        {
                            ptr[0] = (byte) ((ptr[0] * 255) / alpha);
                            ptr[1] = (byte) ((ptr[1] * 255) / alpha);
                            ptr[2] = (byte) ((ptr[2] * 255) / alpha);
                        }
                        ptr += 4;
                    }
                }
        }

        /// <summary>Returns a new image which contains a 1 pixel wide black outline of the specified image.</summary>
        public void GetOutline(BitmapBase result, Color color)
        {
            using (UseRead())
            using (result.UseWrite())
            {
                var src = Data;
                var tgt = result.Data;
                byte cr = color.R, cg = color.G, cb = color.B, ca = color.A;
                for (int y = 0; y < Height; y++)
                {
                    int b = y * Stride;
                    int left = 0;
                    int cur = src[b + 0 + 3];
                    int right;
                    for (int x = 0; x < Width; x++, b += 4)
                    {
                        right = x == Width - 1 ? (byte) 0 : src[b + 4 + 3];
                        if (src[b + 3] == 0)
                        {
                            if (left != 0 || right != 0 || (y > 0 && src[b - Stride + 3] > 0) || ((y < Height - 1) && src[b + Stride + 3] > 0))
                            {
                                tgt[b] = cb;
                                tgt[b + 1] = cg;
                                tgt[b + 2] = cr;
                                tgt[b + 3] = ca;
                            }
                        }
                        left = cur;
                        cur = right;
                    }
                }
            }
        }

    }

    sealed unsafe class BitmapRam : BitmapBase
    {
        private byte[] _bytes;
        private GCHandle _handle;

        public BitmapRam(int width, int height)
        {
            Width = width;
            Height = height;
            Stride = width * 4 + (16 - (width * 4) % 16) % 16; // pad to 16 bytes
            _bytes = new byte[Stride * Height];
        }

        protected override IntPtr Acquire()
        {
            _handle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
            return _handle.AddrOfPinnedObject();
        }

        protected override void Release()
        {
            _handle.Free();
        }

        public override BitmapBase ToBitmapSame()
        {
            return ToBitmapRam();
        }
    }

    /// <summary>
    /// Wrapper around a GDI Bitmap that allows access to its raw byte data. The API is intended to somewhat
    /// resemble that of the WPF BitmapSource. Intentionally supports just a single pixel format: 32bppArgb, aka Bgra32.
    /// </summary>
    sealed class BitmapGdi : BitmapBase, IDisposable
    {
        private SharedPinnedByteArray _bytes;

        /// <summary>
        /// Creates a new, blank BitmapGdi with the specified width and height. The pixel format is fixed: 32bppArgb, aka Bgra32.
        /// </summary>
        public BitmapGdi(int width, int height)
        {
            Width = width;
            Height = height;
            Stride = width * 4 + (16 - (width * 4) % 16) % 16; // pad to 16 bytes
            _bytes = new SharedPinnedByteArray(Stride * Height);
            Bitmap = new D.Bitmap(Width, Height, Stride, D.Imaging.PixelFormat.Format32bppArgb, _bytes.Address);
            Bitmap.SetResolution(96, 96);
        }

        /// <summary>Gets the bitmap bit buffer. Writes to this array modify the image; writes to the image modify this array.</summary>
        public byte[] BackBytes { get { return _bytes.Bytes; } }

        /// <summary>Gets a pointer to the buffer containing the bitmap bit buffer.</summary>
        public IntPtr BackBuffer { get { return _bytes.Address; } }

        /// <summary>
        /// Gets the underlying Bitmap that this BitmapGdi wraps. USAGE WARNING:
        /// DO NOT use this if the BitmapGdi wrapping it may have gone out of scope
        /// and disposed of. This will cause intermittent issues - when the BitmapGdi
        /// gets GC'd. Use <see cref="GetBitmapCopy"/> or GC.KeepAlive the wrapper.
        /// </summary>
        public D.Bitmap Bitmap { get; private set; }

        /// <summary>
        /// Use this to create a new Bitmap that is a copy of the image stored in this
        /// BitmapGdi. This can be passed around safely, unlike the wrapped bitmap
        /// returned by <see cref="Bitmap"/>.
        /// </summary>
        public D.Bitmap GetBitmapCopy()
        {
            var bmp = new D.Bitmap(Bitmap);
            using (var gr = D.Graphics.FromImage(bmp))
                gr.DrawImageUnscaled(Bitmap, 0, 0);
            return bmp;
        }

        /// <summary>Converts this bitmap to a WPF BitmapSource instance.</summary>
        public BitmapSource ToWpf()
        {
            var writable = ToWpfWriteable();
            writable.Freeze();
            return writable;
        }

        /// <summary>Converts this bitmap to a modifiable WPF WriteableBitmap instance.</summary>
        public WriteableBitmap ToWpfWriteable()
        {
            var writable = new WriteableBitmap(Width, Height, Bitmap.HorizontalResolution, Bitmap.VerticalResolution, PixelFormats.Bgra32, null);
            writable.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), BackBytes, Stride, 0);
            return writable;
        }

        /// <summary>Disposes of the underlying resources.</summary>
        public override void Dispose()
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
            base.Dispose();
        }

        protected override IntPtr Acquire()
        {
            return _bytes.Address;
        }

        protected override void Release()
        {
        }

        public override BitmapBase ToBitmapSame()
        {
            return ToBitmapGdi();
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

    sealed class BitmapWpf : BitmapBase
    {
        private WriteableBitmap _bitmap;

        public BitmapWpf(int width, int height)
        {
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            Width = width;
            Height = height;
            Stride = _bitmap.BackBufferStride;
        }

        protected override IntPtr Acquire()
        {
            return _bitmap.BackBuffer;
        }

        protected override void Release()
        {
        }

        public override BitmapBase ToBitmapSame()
        {
            return ToBitmapWpf();
        }
    }

}
