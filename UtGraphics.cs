using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;

namespace TankIconMaker
{
    static partial class Ut
    {
        /// <summary>Returns a new blank (transparent) GDI bitmap of the standard icon size (80x24).</summary>
        /// <param name="draw">Optionally a method to draw into the returned image.</param>
        public static BitmapGdi NewBitmapGdi(Action<D.Graphics> draw = null)
        {
            var result = new BitmapGdi(80, 24);
            if (draw != null)
                using (var g = D.Graphics.FromImage(result.Bitmap))
                    draw(g);
            return result;
        }

        /// <summary>Returns a new blank (transparent) WPF bitmap of the standard icon size (80x24).</summary>
        public static WriteableBitmap NewBitmapWpf()
        {
            return new WriteableBitmap(80, 24, 96, 96, PixelFormats.Bgra32, null);
        }

        /// <summary>Returns a new blank (transparent) WPF bitmap of the standard icon size (80x24).</summary>
        /// <param name="draw">A method to draw into the returned image.</param>
        public static BitmapSource NewBitmapWpf(Action<DrawingContext> draw)
        {
            var bmp = new RenderTargetBitmap(80, 24, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
                draw(context);
            bmp.Render(visual);
            bmp.Freeze();
            return bmp;
        }

        /// <summary>Converts a WPF image to a GDI one.</summary>
        public static BitmapGdi ToGdi(this BitmapSource bmp)
        {
            var result = new BitmapGdi(bmp.PixelWidth, bmp.PixelHeight);
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            bmp.CopyPixels(result.BackBytes, result.BackBufferStride, 0);
            return result;
        }

        /// <summary>Converts a GDI image to a WPF writable one. Also useful for unfreezing frozen WriteableBitmap's.</summary>
        public static WriteableBitmap ToWpfWriteable(this BitmapSource bmp)
        {
            var result = new WriteableBitmap(80, 24, 96, 96, PixelFormats.Bgra32, null);
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            bmp.CopyPixels(new Int32Rect(0, 0, 80, 24), result.BackBuffer, result.BackBufferStride * 23 + 80 * 4, result.BackBufferStride);
            return result;
        }

        /// <summary>Converts this value to the System.Drawing-compatible enum type.</summary>
        public static D.Text.TextRenderingHint ToGdi(this TextAntiAliasStyle style)
        {
            switch (style)
            {
                case TextAntiAliasStyle.Aliased: return D.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                case TextAntiAliasStyle.UnhintedGDI: return D.Text.TextRenderingHint.AntiAlias;
                case TextAntiAliasStyle.AntiAliasGDI: return D.Text.TextRenderingHint.AntiAliasGridFit;
                case TextAntiAliasStyle.ClearType: return D.Text.TextRenderingHint.ClearTypeGridFit;
                default: throw new Exception();
            }
        }

        /// <summary>
        /// Returns a bitmap containing the specified text drawn using the specified font and brush onto a transparent
        /// image. The image is sized to be as small as possible without running the risk of clipping the text.
        /// </summary>
        public static BitmapGdi TextToBitmap(this D.Graphics graphics, string text, D.Font font, D.Brush brush)
        {
            var size = graphics.MeasureString(text, font); // the default is to include any overhangs into the calculation
            var bmp = new BitmapGdi((int) size.Width + 1, (int) size.Height + 1);
            using (var g = D.Graphics.FromImage(bmp.Bitmap))
            {
                g.CompositingQuality = graphics.CompositingQuality;
                g.InterpolationMode = graphics.InterpolationMode;
                g.PixelOffsetMode = graphics.PixelOffsetMode;
                g.SmoothingMode = graphics.SmoothingMode;
                g.TextRenderingHint = graphics.TextRenderingHint;
                g.DrawString(text, font, brush, 0, 0);
            }
            return bmp;
        }

        /// <summary>
        /// Possibly the slowest ever implementation of a text draw routine, but enables pixel-perfect positioning of the text.
        /// If only the <paramref name="left"/> value is non-null, the text is left-aligned. If only the <paramref name="right"/>, it's
        /// right-aligned. If both are null, the text is centered assuming an 80x24 image. If both are non-null, the text is centered around
        /// the mid point of the two values. Top/bottom work the same, with the extra complication of <paramref name="baseline"/>.
        /// </summary>
        /// <param name="graphics">The text is drawn into this drawing object.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="left">X coordinate of the leftmost text pixel, or null (see summary).</param>
        /// <param name="right">X coordinate of the rightmost text pixel, or null (see summary).</param>
        /// <param name="top">Y coordinate of the topmost text pixel, or null (see summary and "baseline").</param>
        /// <param name="bottom">Y coordinate of the bottommost text pixel, or null (see summary and "baseline")</param>
        /// <param name="baseline">If false, top/bottom use the specified string's pixels exactly. If true, top/bottom will instead position the baseline consistently,
        /// so that the top of the tallest letter or the bottom of the lowest descender is located on the specified pixel.</param>
        /// <returns>A rectangle describing the extent of the text's pixels.</returns>
        public static PixelRect DrawString(this D.Graphics graphics, string text, D.Font font, D.Brush brush,
            int? left = null, int? right = null, int? top = null, int? bottom = null, bool baseline = false)
        {
            var bmp = graphics.TextToBitmap(text, font, brush);
            var size = baseline
                ? bmp.PreciseWidth().WithTopBottom(graphics.TextToBitmap("Mgy345", font, D.Brushes.White).PreciseHeight())
                : bmp.PreciseSize();

            int x = (left != null && right != null) ? (left.Value + right.Value) / 2 - size.CenterHorz
                : (left != null) ? left.Value - size.Left
                : (right != null) ? right.Value - size.Right
                : 80 / 2 - size.CenterHorz / 2;
            int y = (top != null && bottom != null) ? (top.Value + bottom.Value) / 2 - size.CenterVert
                : (top != null) ? top.Value - size.Top
                : (bottom != null) ? bottom.Value - size.Bottom
                : 24 / 2 - size.CenterVert;

            graphics.DrawImageUnscaled(bmp.Bitmap, x, y);
            return size.Shifted(x, y);
        }

        /// <summary>A shorthand for drawing an image at coordinate 0,0.</summary>
        public static void DrawImage(this DrawingContext context, BitmapGdi bmp)
        {
            context.DrawImage(bmp.ToWpf(), new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        }

        /// <summary>A shorthand for drawing an image at coordinate 0,0.</summary>
        public static void DrawImage(this DrawingContext context, BitmapSource bmp)
        {
            context.DrawImage(bmp, new Rect(0, 0, bmp.Width, bmp.Height));
        }

        /// <summary>A shorthand for drawing an image into a GDI image at coordinate 0,0.</summary>
        public static BitmapGdi DrawImage(this BitmapGdi target, BitmapGdi source)
        {
            using (var g = D.Graphics.FromImage(target.Bitmap))
                g.DrawImageUnscaled(source.Bitmap, 0, 0);
            return target;
        }

        /// <summary>Returns a new image which contains a 1 pixel wide black outline of the specified image.</summary>
        public static BitmapGdi GetOutline(this BitmapGdi srcBitmap, int opacity = 255)
        {
            var tgtBitmap = Ut.NewBitmapGdi();
            var src = srcBitmap.BackBytes;
            var tgt = tgtBitmap.BackBytes;
            for (int y = 0; y < srcBitmap.PixelHeight; y++)
            {
                int b = y * srcBitmap.BackBufferStride;
                int left = 0;
                int cur = src[b + 0 + 3];
                int right;
                for (int x = 0; x < srcBitmap.PixelWidth; x++, b += 4)
                {
                    right = x == srcBitmap.PixelWidth - 1 ? (byte) 0 : src[b + 4 + 3];
                    if (src[b + 3] == 0)
                    {
                        if (left != 0 || right != 0 || (y > 0 && src[b - srcBitmap.BackBufferStride + 3] > 0) || ((y < srcBitmap.PixelHeight - 1) && src[b + srcBitmap.BackBufferStride + 3] > 0))
                        {
                            tgt[b] = tgt[b + 1] = tgt[b + 2] = 0;
                            tgt[b + 3] = (byte) opacity;
                        }
                    }
                    left = cur;
                    cur = right;
                }
            }
            return tgtBitmap;
        }

        /// <summary>Returns a new image which contains a version of this image with a black semitransparent blur of a hard-coded radius.</summary>
        public static BitmapGdi GetBlurred(this BitmapGdi srcBitmap)
        {
            var tgtBitmap = Ut.NewBitmapGdi();
            var src = srcBitmap.BackBytes;
            var tgt = tgtBitmap.BackBytes;
            for (int y = 0; y < srcBitmap.PixelHeight; y++)
            {
                int b = y * srcBitmap.BackBufferStride;
                for (int x = 0; x < srcBitmap.PixelWidth; x++, b += 4)
                {
                    tgt[b] = tgt[b + 1] = tgt[b + 2] = 0;
                    tgt[b + 3] = src[b + 3];
                }
            }

            for (int iter = 0; iter < 5; iter++)
                for (int y = 0; y < srcBitmap.PixelHeight; y++)
                {
                    int b = y * srcBitmap.BackBufferStride;
                    int left = 0;
                    int cur = tgt[b + 0 + 3];
                    int right;
                    for (int x = 0; x < srcBitmap.PixelWidth; x++, b += 4)
                    {
                        right = x == srcBitmap.PixelWidth - 1 ? (byte) 0 : tgt[b + 4 + 3];
                        if (tgt[b + 3] == 0)
                        {
                            int top = y == 0 ? 0 : tgt[b - srcBitmap.BackBufferStride + 3];
                            int bottom = y == srcBitmap.PixelHeight - 1 ? 0 : tgt[b + srcBitmap.BackBufferStride + 3];
                            tgt[b + 3] = (byte) (((left + right + top + bottom) * 6) / 40);
                        }
                        left = cur;
                        cur = right;
                    }
                }
            return tgtBitmap;
        }
    }

    enum TextAntiAliasStyle
    {
        [Description("Aliased")]
        Aliased,
        [Description("Anti-aliased (hinted)")]
        AntiAliasGDI,
        [Description("Anti-aliased (unhinted)")]
        UnhintedGDI,
        [Description("ClearType")]
        ClearType,
    }

    /// <summary>A better Int32Rect, expressly designed to represent pixel areas - hence the left/right/top/bottom/width/height are always "inclusive".</summary>
    struct PixelRect
    {
        private int _left, _width, _top, _height;
        /// <summary>The leftmost pixel included in the rect.</summary>
        public int Left { get { return _left; } }
        /// <summary>The topmost pixel included in the rect.</summary>
        public int Top { get { return _top; } }
        /// <summary>The rightmost pixel included in the rect.</summary>
        public int Right { get { return _left + _width - 1; } }
        /// <summary>The bottommost pixel included in the rect.</summary>
        public int Bottom { get { return _top + _height - 1; } }
        /// <summary>The total number of pixels, horizontally, included in the rect.</summary>
        public int Width { get { return _width; } }
        /// <summary>The total number of pixels, vertically, included in the rect.</summary>
        public int Height { get { return _height; } }
        /// <summary>The X coordinate of the center pixel. If the number of pixels in the rect is even, returns the pixel to the right of center.</summary>
        public int CenterHorz { get { return _left + _width / 2; } }
        /// <summary>The Y coordinate of the center pixel. If the number of pixels in the rect is even, returns the pixel to the bottom of center.</summary>
        public int CenterVert { get { return _top + _height / 2; } }

        public static PixelRect FromBounds(int left, int top, int right, int bottom)
        {
            return new PixelRect { _left = left, _top = top, _width = right - left + 1, _height = bottom - top + 1 };
        }
        public static PixelRect FromMixed(int left, int top, int width, int height)
        {
            return new PixelRect { _left = left, _top = top, _width = width, _height = height };
        }
        public static PixelRect FromLeftRight(int left, int right) { return FromBounds(left, 0, right, 0); }
        public static PixelRect FromTopBottom(int top, int bottom) { return FromBounds(0, top, 0, bottom); }
        public PixelRect WithLeftRight(int left, int right) { return FromBounds(left, Top, right, Bottom); }
        public PixelRect WithLeftRight(PixelRect width) { return FromBounds(width.Left, Top, width.Right, Bottom); }
        public PixelRect WithTopBottom(int top, int bottom) { return FromBounds(Left, top, Right, bottom); }
        public PixelRect WithTopBottom(PixelRect height) { return FromBounds(Left, height.Top, Right, height.Bottom); }
        public PixelRect Shifted(int deltaX, int deltaY) { return FromMixed(Left + deltaX, Top + deltaY, Width, Height); }
    }
}
