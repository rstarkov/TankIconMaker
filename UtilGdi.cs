using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using W = System.Windows.Media;
using WI = System.Windows.Media.Imaging;

namespace TankIconMaker
{
    /// <summary>
    /// Wrapper around a Bitmap that allows access to its raw byte data.
    /// </summary>
    public sealed class BytesBitmap : IDisposable
    {
        private SharedPinnedByteArray _bytes;
        private Bitmap _bitmap;
        private int _stride;
        private int _pixelFormatSize;

        /// <summary>
        /// Gets an array that contains the bitmap bit buffer.
        /// </summary>
        public byte[] Bits
        {
            get { return _bytes.Bytes; }
        }

        /// <summary>
        /// Gets the underlying Bitmap that this BytesBitmap wraps. USAGE WARNING:
        /// DO NOT use this if the BytesBitmap wrapping it may have gone out of context
        /// and disposed of. This will cause intermittent issues - when the BytesBitmap
        /// gets GC'd. Use <see cref="GetBitmapCopy"/> instead.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }

        /// <summary>
        /// Use this to create a new Bitmap that is a copy of the image stored in this
        /// BytesBitmap. This can be passed around safely, unlike the wrapped bitmap
        /// returned by <see cref="Bitmap"/>.
        /// </summary>
        public Bitmap GetBitmapCopy()
        {
            Bitmap bmp = new Bitmap(_bitmap);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImageUnscaled(_bitmap, 0, 0);
            return bmp;
        }

        /// <summary>
        /// Gets the stride (the number of bytes to go one pixel down) of the bitmap.
        /// </summary>
        public int Stride
        {
            get { return _stride; }
        }

        /// <summary>
        /// Gets the number of bits needed to store a pixel.
        /// </summary>
        public int PixelFormatSize
        {
            get { return _pixelFormatSize; }
        }

        /// <summary>
        /// Gets a safe pointer to the buffer containing the bitmap bits.
        /// </summary>
        public IntPtr BitPtr
        {
            get { return _bytes.Address; }
        }

        /// <summary>
        /// Creates a new, blank BytesBitmap with the specified width, height, and pixel format.
        /// </summary>
        public BytesBitmap(int width, int height, PixelFormat format)
        {
            _pixelFormatSize = Image.GetPixelFormatSize(format);
            _stride = width * _pixelFormatSize / 8;
            int padding = _stride % 4;
            _stride += (padding == 0) ? 0 : 4 - padding;
            _bytes = new SharedPinnedByteArray(_stride * height);
            _bitmap = new Bitmap(width, height, _stride, format, _bytes.Address);
        }

        public int Width { get { return Bitmap.Width; } }
        public int Height { get { return Bitmap.Height; } }

        public WI.BitmapSource ToWpf()
        {
            var writable = new WI.WriteableBitmap(Width, Height, Bitmap.HorizontalResolution, Bitmap.VerticalResolution, W.PixelFormats.Bgra32, null);
            writable.WritePixels(new System.Windows.Int32Rect(0, 0, Width, Height), Bits, Stride, 0);
            writable.Freeze();
            return writable;
        }

        #region Dispose stuff

        private bool _disposed;

        /// <summary>Specifies whether the underlying resources for this <see cref="BytesBitmap"/> have already been disposed.</summary>
        public bool Disposed
        {
            get { return _disposed; }
        }

        /// <summary>Disposes the underlying resources.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _bitmap.Dispose();
            _bytes.ReleaseReference();
            _disposed = true;
            _bitmap = null;
        }

        #endregion
    }

    /// <summary>
    /// This class represents a byte array which is pinned to avoid relocation
    /// by the GC and implements reference counting.
    /// </summary>
    internal sealed class SharedPinnedByteArray
    {
        private GCHandle _handle;
        private int _refCount;
        private bool _destroyed;

        /// <summary>
        /// Gets the allocated byte array. This can be modified as desired.
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Gets an unmanaged address of the first (index 0) byte of the byte array.
        /// </summary>
        public IntPtr Address { get; private set; }

        /// <summary>
        /// Returns an unmanaged address of the specified byte in the byte array.
        /// </summary>
        public IntPtr AddressOf(int index)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, index);
        }

        /// <summary>
        /// Creates a new pinned array of the specified size, that can be accessed through <see cref="Bytes"/>.
        /// One reference is automatically added; call <see cref="ReleaseReference"/> when finished using this array.
        /// </summary>
        /// <param name="length">The number of bytes that the pinned array should contain</param>
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
        /// </summary>
        public void ReleaseReference()
        {
            _refCount--;
            if (_refCount <= 0)
                destroy();
        }

        /// <summary>Gets the length of the byte array.</summary>
        public int Length { get { return Bytes.Length; } }

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

    /// <summary>Provides a way to temporarily modify the Transform of a System.Drawing.Graphics object by enclosing the affected code in a “using” scope.</summary>
    /// <example>
    /// <para>The following example demonstrates how GraphicsTransformer can be used to render graphics translated.</para>
    /// <code>
    ///     var g = Graphics.FromImage(...);
    ///     using (new GraphicsTransformer(g).Translate(15, 10))
    ///     {
    ///         // As this is inside the scope of the GraphicsTransformer, the rectangle is translated 15 pixels to the right and 10 down.
    ///         g.DrawRectangle(20, 20, 100, 100);
    ///     }
    ///     
    ///     // As this statement is outside the scope of the GraphicsTransformer, the rectangle is not translated.
    ///     // The net effect is that two rectangles are rendered even though both calls use the same co-ordinates.
    ///     g.DrawRectangle(20, 20, 100, 100);
    /// </code>
    /// </example>
    public class GraphicsTransformer : IDisposable
    {
        private Graphics _graphics;
        private static Dictionary<Graphics, Stack<Matrix>> _transforms = new Dictionary<Graphics, Stack<Matrix>>();

        /// <summary>Instantiates a new <see cref="GraphicsTransformer"/> instance. Use this in a “using” statement.</summary>
        /// <param name="g">The Graphics object whose Transform to modify.</param>
        public GraphicsTransformer(Graphics g)
        {
            _graphics = g;
            if (!_transforms.ContainsKey(g))
            {
                _transforms[g] = new Stack<Matrix>();
                _transforms[g].Push(g.Transform.Clone());
            }
            _transforms[g].Push(_transforms[g].Peek().Clone());
        }

        /// <summary>Translates the graphics by the specified amount.</summary>
        public GraphicsTransformer Translate(float offsetX, float offsetY)
        {
            var m = _transforms[_graphics].Peek();
            m.Translate(offsetX, offsetY, MatrixOrder.Append);
            _graphics.Transform = m;
            return this;
        }

        /// <summary>Translates the graphics by the specified amount.</summary>
        public GraphicsTransformer Translate(double offsetX, double offsetY) { return Translate((float) offsetX, (float) offsetY); }

        /// <summary>Scales the graphics by the specified factors.</summary>
        public GraphicsTransformer Scale(float scaleX, float scaleY)
        {
            var m = _transforms[_graphics].Peek();
            m.Scale(scaleX, scaleY, MatrixOrder.Append);
            _graphics.Transform = m;
            return this;
        }

        /// <summary>Scales the graphics by the specified factors.</summary>
        public GraphicsTransformer Scale(double scaleX, double scaleY) { return Scale((float) scaleX, (float) scaleY); }

        /// <summary>Rotates the graphics by the specified angle in radians.</summary>
        public GraphicsTransformer Rotate(float angle)
        {
            var m = _transforms[_graphics].Peek();
            m.Rotate(angle, MatrixOrder.Append);
            _graphics.Transform = m;
            return this;
        }

        /// <summary>Rotates the graphics clockwise by the specified angle in radians about the specified center point.</summary>
        public GraphicsTransformer RotateAt(float angle, PointF point)
        {
            var m = _transforms[_graphics].Peek();
            m.RotateAt(angle, point, MatrixOrder.Append);
            _graphics.Transform = m;
            return this;
        }

        /// <summary>Rotates the graphics clockwise by the specified angle in radians about the specified center point.</summary>
        public GraphicsTransformer RotateAt(float angle, float x, float y) { return RotateAt(angle, new PointF(x, y)); }

        /// <summary>Returns the Transform of the Graphics object back to its original value.</summary>
        public void Dispose()
        {
            _transforms[_graphics].Pop();
            _graphics.Transform = _transforms[_graphics].Peek();
            if (_transforms[_graphics].Count == 1)
                _transforms.Remove(_graphics);
        }
    }
}
