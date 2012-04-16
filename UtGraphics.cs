using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
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

        /// <summary>Converts a WPF image source to a WPF writable one. Also useful for unfreezing frozen WriteableBitmap's.</summary>
        public static WriteableBitmap ToWpfWriteable(this BitmapSource bmp)
        {
            var result = new WriteableBitmap(80, 24, 96, 96, PixelFormats.Bgra32, null);
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            bmp.CopyPixels(new Int32Rect(0, 0, 80, 24), result.BackBuffer, result.BackBufferStride * 23 + 80 * 4, result.BackBufferStride);
            return result;
        }

        /// <summary>Converts this value to the System.Drawing-compatible enum type.</summary>
        public static D.Text.TextRenderingHint ToGdi(this TextSmoothingStyle style)
        {
            switch (style)
            {
                case TextSmoothingStyle.Aliased: return D.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                case TextSmoothingStyle.UnhintedGDI: return D.Text.TextRenderingHint.AntiAlias;
                case TextSmoothingStyle.AntiAliasGDI: return D.Text.TextRenderingHint.AntiAliasGridFit;
                case TextSmoothingStyle.ClearType: return D.Text.TextRenderingHint.ClearTypeGridFit;
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

        /// <summary>Applies a "colorize" effect to the image.</summary>
        /// <param name="hue">The hue of the color to apply, 0..359</param>
        /// <param name="saturation">The saturation of the color to apply, 0..1</param>
        /// <param name="lightness">A lightness adjustment, -1..1</param>
        /// <param name="alpha">Overall strength of the effect, 0..1. A value of 0 keeps only the original image.</param>
        public static unsafe void Colorize(this WriteableBitmap bmp, int hue, double saturation, double lightness, double alpha)
        {
            Colorize((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, hue, saturation, lightness, alpha);
        }

        /// <summary>Applies a "colorize" effect to the image.</summary>
        /// <param name="hue">The hue of the color to apply, 0..359</param>
        /// <param name="saturation">The saturation of the color to apply, 0..1</param>
        /// <param name="lightness">A lightness adjustment, -1..1</param>
        /// <param name="alpha">Overall strength of the effect, 0..1. A value of 0 keeps only the original image.</param>
        public static unsafe void Colorize(this BitmapGdi bmp, int hue, double saturation, double lightness, double alpha)
        {
            Colorize((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, hue, saturation, lightness, alpha);
        }

        /// <summary>Applies a "colorize" effect to the image.</summary>
        /// <param name="hue">The hue of the color to apply, 0..359</param>
        /// <param name="saturation">The saturation of the color to apply, 0..1</param>
        /// <param name="lightness">A lightness adjustment, -1..1</param>
        /// <param name="alpha">Overall strength of the effect, 0..1. A value of 0 keeps only the original image.</param>
        public static unsafe void Colorize(byte* image, int width, int height, int stride, int hue, double saturation, double lightness, double alpha)
        {
            // http://stackoverflow.com/a/9177602/33080
            var color = Ut.BlendColors(Color.FromRgb(128, 128, 128), ColorHSV.FromHSV(hue, 100, 100).ToColorWpf(), saturation);
            for (int y = 0; y < height; y++)
            {
                byte* ptr = image + y * stride;
                byte* end = ptr + width * 4;
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

        /// <summary>Makes the whole image more transparent by adjusting the alpha channel.</summary>
        /// <param name="opacity">The opacity to apply, 0..255. 0 makes the image completely transparent, while 255 makes no changes at all.</param>
        public static unsafe void Transparentize(this WriteableBitmap bmp, int opacity)
        {
            Transparentize((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, opacity);
        }

        /// <summary>Makes the whole image more transparent by adjusting the alpha channel.</summary>
        /// <param name="opacity">The opacity to apply, 0..255. 0 makes the image completely transparent, while 255 makes no changes at all.</param>
        public static unsafe void Transparentize(this BitmapGdi bmp, int opacity)
        {
            Transparentize((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, opacity);
        }

        /// <summary>Makes the whole image more transparent by adjusting the alpha channel.</summary>
        /// <param name="opacity">The opacity to apply, 0..255. 0 makes the image completely transparent, while 255 makes no changes at all.</param>
        public static unsafe void Transparentize(byte* image, int width, int height, int stride, int opacity)
        {
            for (int y = 0; y < height; y++)
            {
                byte* ptr = image + y * stride + 3;
                byte* end = ptr + width * 4;
                while (ptr < end)
                {
                    *ptr = (byte) ((*ptr * opacity) / 255);
                    ptr += 4;
                }
            }
        }

        public static unsafe void ScaleOpacity(this WriteableBitmap bmp, double adjustment, OpacityStyle style)
        {
            ScaleOpacity((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, adjustment, style);
        }

        public static unsafe void ScaleOpacity(this BitmapGdi bmp, double adjustment, OpacityStyle style)
        {
            ScaleOpacity((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, adjustment, style);
        }

        public static unsafe void ScaleOpacity(byte* image, int width, int height, int stride, double adjustment, OpacityStyle style)
        {
            if (adjustment == 0)
                return;
            if (style == OpacityStyle.Auto)
                style = adjustment > 0 ? OpacityStyle.Additive : OpacityStyle.MoveEndpoint;

            var lut = new byte[256];
            for (int x = 0; x < 256; x++)
                switch (style)
                {
                    case OpacityStyle.MoveEndpoint:
                        if (adjustment < 0)
                            lut[x] = (byte) (x / (1.0 + -adjustment));
                        else
                            lut[x] = (byte) (255 - (255 - x) / (1.0 + adjustment));
                        break;
                    case OpacityStyle.MoveMidpoint:
                        lut[x] = (byte) (Math.Pow(x / 255.0, adjustment < 0 ? (1 - adjustment) : (1 / (1 + adjustment))) * 255);
                        break;
                    case OpacityStyle.Additive:
                        if (adjustment < 0)
                            lut[x] = (byte) Math.Max(0, 255 - (255 - x) * (1.0 - adjustment));
                        else
                            lut[x] = (byte) Math.Min(255, x * (1.0 + adjustment));
                        break;
                    default:
                        throw new Exception();
                }

            for (int y = 0; y < height; y++)
            {
                byte* ptr = image + y * stride + 3;
                byte* end = ptr + width * 4;
                while (ptr < end)
                {
                    *ptr = lut[*ptr];
                    ptr += 4;
                }
            }
        }

        public static unsafe void SetColor(this WriteableBitmap bmp, Color color)
        {
            SetColor((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, color);
        }

        public static unsafe void SetColor(this BitmapGdi bmp, Color color)
        {
            SetColor((byte*) bmp.BackBuffer, bmp.PixelWidth, bmp.PixelHeight, bmp.BackBufferStride, color);
        }

        public static unsafe void SetColor(byte* image, int width, int height, int stride, Color color)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;
            for (int y = 0; y < height; y++)
            {
                byte* ptr = image + y * stride;
                byte* end = ptr + width * 4;
                while (ptr < end)
                {
                    *ptr++ = b;
                    *ptr++ = g;
                    *ptr++ = r;
                    ptr++;
                }
            }
        }

        public static unsafe void PreMultiply(byte* image, int width, int height, int stride)
        {
            for (int y = 0; y < height; y++)
            {
                byte* ptr = image + y * stride;
                byte* end = ptr + width * 4;
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

        public static unsafe void UnPreMultiply(byte* image, int width, int height, int stride)
        {
            for (int y = 0; y < height; y++)
            {
                byte* ptr = image + y * stride;
                byte* end = ptr + width * 4;
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

        public static unsafe WriteableBitmap BlendImages(WriteableBitmap imgLeft, WriteableBitmap imgRight, double rightAmount)
        {
            if (imgLeft.PixelWidth != imgRight.PixelWidth || imgLeft.PixelHeight != imgRight.PixelHeight)
                throw new ArgumentException();
            var result = new WriteableBitmap(imgLeft.PixelWidth, imgLeft.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
            BlendImages(
                (byte*) imgLeft.BackBuffer, imgLeft.BackBufferStride,
                (byte*) imgRight.BackBuffer, imgRight.BackBufferStride,
                (byte*) result.BackBuffer, result.BackBufferStride, result.PixelWidth, result.PixelHeight, rightAmount);
            return result;
        }

        public static unsafe void BlendImages(byte* imgLeft, int strideLeft, byte* imgRight, int strideRight, byte* imgResult, int strideResult, int width, int height, double rightAmount)
        {
            var leftAmount = 1 - rightAmount;
            for (int y = 0; y < height; y++)
            {
                byte* ptrLeft = imgLeft + y * strideLeft;
                byte* ptrRight = imgRight + y * strideLeft;
                byte* ptrResult = imgResult + y * strideLeft;
                byte* endResult = ptrResult + width * 4;
                for (; ptrResult < endResult; ptrLeft += 4, ptrRight += 4, ptrResult += 4)
                {
                    double rightRatio = blendRightRatio(*(ptrLeft + 3), *(ptrRight + 3), rightAmount);
                    double leftRatio = 1 - rightRatio;

                    *(ptrResult + 0) = (byte) (*(ptrLeft + 0) * leftRatio + *(ptrRight + 0) * rightRatio);
                    *(ptrResult + 1) = (byte) (*(ptrLeft + 1) * leftRatio + *(ptrRight + 1) * rightRatio);
                    *(ptrResult + 2) = (byte) (*(ptrLeft + 2) * leftRatio + *(ptrRight + 2) * rightRatio);
                    *(ptrResult + 3) = (byte) (*(ptrLeft + 3) * leftAmount + *(ptrRight + 3) * rightAmount);
                }
            }
        }
    }

    enum TextSmoothingStyle
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

    enum OpacityStyle { Auto, [Description("Move endpoint")] MoveEndpoint, [Description("Move midpoint")] MoveMidpoint, Additive }
    enum BlurEdgeMode { Transparent, Same, Wrap }

    class GaussianBlur
    {
        private int _radius;
        private int[] _kernel;
        private int _kernelSum;

        public double Radius { get; private set; }

        public GaussianBlur(double radius)
        {
            Radius = radius;
            _radius = (int) Math.Ceiling(radius);

            // Compute the kernel by sampling the gaussian
            int len = _radius * 2 + 1;
            double[] kernel = new double[len];
            double sigma = radius / 3;
            double sigma22 = 2 * sigma * sigma;
            double sigmaPi2 = 2 * Math.PI * sigma;
            double sqrtSigmaPi2 = (double) Math.Sqrt(sigmaPi2);
            double radius2 = radius * radius;
            double total = 0;
            int index = 0;
            for (int x = -_radius; x <= _radius; x++)
            {
                double distance = x * x;
                if (distance > radius2)
                    kernel[index] = 0;
                else
                    kernel[index] = Math.Exp(-distance / sigma22) / sqrtSigmaPi2;
                total += kernel[index];
                index++;
            }

            // Convert to integers
            _kernel = new int[len];
            _kernelSum = 0;
            double scale = 2147483647.0 / (255 * total * len); // scale so that the integer total can never overflow
            scale /= 5; // there will be rounding errors; make sure we don’t overflow even then
            for (int i = 0; i < len; i++)
            {
                _kernel[i] = (int) (kernel[i] * scale);
                _kernelSum += _kernel[i];
            }
        }

        public unsafe WriteableBitmap Blur(WriteableBitmap image, BlurEdgeMode edgeMode)
        {
            var sourceAndResult = image.Clone();
            var temp = new WriteableBitmap(image.PixelWidth, image.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
            Blur((byte*) sourceAndResult.BackBuffer, (byte*) temp.BackBuffer,
                sourceAndResult.BackBufferStride, temp.BackBufferStride,
                image.PixelWidth, image.PixelHeight, edgeMode);
            GC.KeepAlive(temp);
            return sourceAndResult;
        }

        public unsafe WriteableBitmap BlurHorizontal(WriteableBitmap image, BlurEdgeMode edgeMode)
        {
            var source = image.Clone();
            var result = new WriteableBitmap(image.PixelWidth, image.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
            BlurHorizontal((byte*) source.BackBuffer, (byte*) result.BackBuffer,
                source.BackBufferStride, result.BackBufferStride,
                image.PixelWidth, image.PixelHeight, edgeMode);
            GC.KeepAlive(source);
            return result;
        }

        public unsafe WriteableBitmap BlurVertical(WriteableBitmap image, BlurEdgeMode edgeMode)
        {
            var source = image.Clone();
            var result = new WriteableBitmap(image.PixelWidth, image.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
            BlurHorizontal((byte*) source.BackBuffer, (byte*) result.BackBuffer,
                source.BackBufferStride, result.BackBufferStride,
                image.PixelWidth, image.PixelHeight, edgeMode);
            GC.KeepAlive(source);
            return result;
        }

        private unsafe void Blur(byte* imgSourceAndResult, byte* imgTemp, int strideSource, int strideTemp, int width, int height, BlurEdgeMode edgeMode)
        {
            Ut.PreMultiply(imgSourceAndResult, width, height, strideSource);
            horizontal(imgSourceAndResult, imgTemp, strideSource, strideTemp, width, height, edgeMode);
            vertical(imgTemp, imgSourceAndResult, strideSource, strideTemp, width, height, edgeMode);
            Ut.UnPreMultiply(imgSourceAndResult, width, height, strideTemp);
        }

        private unsafe void BlurHorizontal(byte* imgSource, byte* imgResult, int strideSource, int strideResult, int width, int height, BlurEdgeMode edgeMode)
        {
            Ut.PreMultiply(imgSource, width, height, strideSource);
            horizontal(imgSource, imgResult, strideSource, strideResult, width, height, edgeMode);
            Ut.UnPreMultiply(imgResult, width, height, strideResult);
        }

        private unsafe void BlurVertical(byte* imgSource, byte* imgResult, int strideSource, int strideResult, int width, int height, BlurEdgeMode edgeMode)
        {
            Ut.PreMultiply(imgSource, width, height, strideSource);
            vertical(imgSource, imgResult, strideSource, strideResult, width, height, edgeMode);
            Ut.UnPreMultiply(imgResult, width, height, strideResult);
        }

        private unsafe void horizontal(byte* imgSource, byte* imgResult, int strideSource, int strideResult, int width, int height, BlurEdgeMode edgeMode)
        {
            for (int y = 0; y < height; y++)
            {
                byte* rowSource = imgSource + y * strideSource;
                byte* rowResult = imgResult + y * strideResult;
                for (int x = 0; x < width; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, aSum = 0;
                    for (int k = 0, xSrc = x - _kernel.Length / 2; k < _kernel.Length; k++, xSrc++)
                    {
                        int xRead = xSrc;
                        if (xRead < 0 || xRead >= width)
                            switch (edgeMode)
                            {
                                case BlurEdgeMode.Transparent:
                                    continue;
                                case BlurEdgeMode.Same:
                                    xRead = xRead < 0 ? 0 : width - 1;
                                    break;
                                case BlurEdgeMode.Wrap:
                                    xRead = Ut.ModPositive(xRead, width);
                                    break;
                            }
                        xRead <<= 2; // * 4
                        bSum += _kernel[k] * rowSource[xRead + 0];
                        gSum += _kernel[k] * rowSource[xRead + 1];
                        rSum += _kernel[k] * rowSource[xRead + 2];
                        aSum += _kernel[k] * rowSource[xRead + 3];
                    }

                    int xWrite = x << 2; // * 4
                    rowResult[xWrite + 0] = (byte) (bSum / _kernelSum);
                    rowResult[xWrite + 1] = (byte) (gSum / _kernelSum);
                    rowResult[xWrite + 2] = (byte) (rSum / _kernelSum);
                    rowResult[xWrite + 3] = (byte) (aSum / _kernelSum);
                }
            }
        }

        private unsafe void vertical(byte* imgSource, byte* imgResult, int strideSource, int strideResult, int width, int height, BlurEdgeMode edgeMode)
        {
            for (int x = 0; x < width; x++)
            {
                byte* colSource = imgSource + x * 4;
                byte* colResult = imgResult + x * 4;
                for (int y = 0; y < height; y++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, aSum = 0;
                    for (int k = 0, ySrc = y - _kernel.Length / 2; k < _kernel.Length; k++, ySrc++)
                    {
                        int yRead = ySrc;
                        if (yRead < 0 || yRead >= height)
                            switch (edgeMode)
                            {
                                case BlurEdgeMode.Transparent:
                                    continue;
                                case BlurEdgeMode.Same:
                                    yRead = yRead < 0 ? 0 : height - 1;
                                    break;
                                case BlurEdgeMode.Wrap:
                                    yRead = Ut.ModPositive(yRead, height);
                                    break;
                            }
                        yRead *= strideSource;
                        bSum += _kernel[k] * colSource[yRead + 0];
                        gSum += _kernel[k] * colSource[yRead + 1];
                        rSum += _kernel[k] * colSource[yRead + 2];
                        aSum += _kernel[k] * colSource[yRead + 3];
                    }

                    int yWrite = y * strideResult;
                    colResult[yWrite + 0] = (byte) (bSum / _kernelSum);
                    colResult[yWrite + 1] = (byte) (gSum / _kernelSum);
                    colResult[yWrite + 2] = (byte) (rSum / _kernelSum);
                    colResult[yWrite + 3] = (byte) (aSum / _kernelSum);
                }
            }
        }
    }
}
