using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        public static BitmapGdi NewBitmapGdi(Action<D.Graphics> draw = null, int width = 80, int height = 24)
        {
            var result = new BitmapGdi(width, height);
            if (draw != null)
                using (var g = D.Graphics.FromImage(result.Bitmap))
                    draw(g);
            return result;
        }

        /// <summary>Returns a new blank (transparent) WPF bitmap of the standard icon size (80x24).</summary>
        /// <param name="draw">A method to draw into the returned image.</param>
        public static BitmapSource NewBitmapWpf(Action<DrawingContext> draw, int width = 80, int height = 24)
        {
            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
                draw(context);
            bmp.Render(visual);
            bmp.Freeze();
            return bmp;
        }

        public static BitmapRam ToBitmapRam(this BitmapSource src)
        {
            var result = new BitmapRam(src.PixelWidth, src.PixelHeight);
            result.CopyPixelsFrom(src);
            return result;
        }

        public static BitmapWpf ToBitmapWpf(this BitmapSource src)
        {
            var result = new BitmapWpf(src.PixelWidth, src.PixelHeight);
            result.CopyPixelsFrom(src);
            return result;
        }

        public static BitmapGdi ToBitmapGdi(this BitmapSource src)
        {
            var result = new BitmapGdi(src.PixelWidth, src.PixelHeight);
            result.CopyPixelsFrom(src);
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
        /// </summary>
        /// <param name="graphics">The text is drawn into this drawing object.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="fontFamily">Name of the font family.</param>
        /// <param name="fontSize">Maximum font size: if width/height specified, text will be shrunk from this.</param>
        /// <param name="fontStyle">Font style in which to draw the text.</param>
        /// <param name="x">X coordinate of the anchor point.</param>
        /// <param name="y">Y coordinate of the anchor point (see also <paramref name="baseline"/>).</param>
        /// <param name="anchor">Specifies where the anchor point is positioned relative to the text.</param>
        /// <param name="width">Maximum text width, in pixels. Text is shurnk if necessary to fit in.</param>
        /// <param name="height">Maximum text height, in pixels. Text is shurnk if necessary to fit in.</param>
        /// <param name="baseline">If false, the vertical anchoring is done on the topmost/bottommost pixel. If true, top/bottom will instead position the baseline consistently,
        /// so that the top of the tallest letter or the bottom of the lowest descender is located on the specified pixel.</param>
        /// <returns>A rectangle describing the extent of the text's pixels.</returns>
        public static PixelRect DrawString(this D.Graphics graphics, string text, D.Brush brush, string fontFamily, double fontSize, D.FontStyle fontStyle,
            int x, int y, Anchor anchor, int? width = null, int? height = null, bool baseline = false)
        {
            BitmapGdi bmp;
            PixelRect size;

            double fontSizeMin = 0;
            double fontSizeMax = fontSize;
            double threshold = graphics.TextRenderingHint == D.Text.TextRenderingHint.AntiAlias ? 0.07 : 0.5;

            while (true)
            {
                var font = new D.Font(fontFamily, (float) fontSize, fontStyle);
                bmp = graphics.TextToBitmap(text, font, brush);
                size = baseline
                    ? bmp.PreciseWidth().WithTopBottom(graphics.TextToBitmap("Mgy345", font, D.Brushes.White).PreciseHeight())
                    : bmp.PreciseSize();

                if (width == null && height == null)
                    break;
                if ((width == null || size.Width <= width.Value) && (height == null || size.Height <= height.Value))
                    fontSizeMin = fontSize; // Fits
                else
                    fontSizeMax = fontSize; // Doesn't fit
                if (fontSize == fontSizeMin && fontSizeMax - fontSizeMin <= threshold)
                    break;
                fontSize = (fontSizeMin + fontSizeMax) / 2;
            }

            var anchr = (AnchorRaw) anchor;
            x -= anchr.HasFlag(AnchorRaw.Right) ? size.Width - 1 : anchr.HasFlag(AnchorRaw.Center) ? (size.Width - 1) / 2 : 0;
            y -= anchr.HasFlag(AnchorRaw.Bottom) ? size.Height - 1 : anchr.HasFlag(AnchorRaw.Mid) ? (size.Height - 1) / 2 : 0;

            graphics.DrawImageUnscaled(bmp.Bitmap, x - size.Left, y - size.Top);
            GC.KeepAlive(bmp);
            return size.Shifted(x, y);
        }

        /// <summary>A shorthand for drawing an image at coordinate 0,0.</summary>
        public static void DrawImage(this DrawingContext context, BitmapGdi bmp)
        {
            context.DrawImage(bmp.ToWpf(), new Rect(0, 0, bmp.Width, bmp.Height));
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

        public static void SaveImage(BitmapSource image, string path, string extension)
        {
            if (extension.EqualsNoCase(".tga"))
                Targa.Save(image.ToBitmapRam(), path);
            else
            {
                var encoder = extension.EqualsNoCase(".jpg") ? new JpegBitmapEncoder()
                    : extension.EqualsNoCase(".bmp") ? new BmpBitmapEncoder()
                    : (BitmapEncoder) new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                    encoder.Save(file);
            }
        }
    }

    [TypeConverter(typeof(TextSmoothingStyleTranslation.Conv))]
    enum TextSmoothingStyle
    {
        Aliased,
        AntiAliasGDI,
        UnhintedGDI,
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
        /// <summary>The X coordinate of the center pixel. If the number of pixels in the rect is even, returns a non-integer value.</summary>
        public double CenterHorzD { get { return _left + _width / 2.0; } }
        /// <summary>The Y coordinate of the center pixel. If the number of pixels in the rect is even, returns a non-integer value.</summary>
        public double CenterVertD { get { return _top + _height / 2.0; } }

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

    [TypeConverter(typeof(OpacityStyleTranslation.Conv))]
    enum OpacityStyle
    {
        Auto,
        MoveEndpoint,
        MoveMidpoint,
        Additive
    }

    [TypeConverter(typeof(BlurEdgeModeTranslation.Conv))]
    enum BlurEdgeMode
    {
        Transparent,
        Same,
        Mirror,
        Wrap,
    }

    [Flags]
    enum AnchorRaw
    {
        Left = 0x01, Center = 0x02, Right = 0x04,
        Top = 0x10, Mid = 0x20, Bottom = 0x40,
    }

    enum Anchor
    {
        TopLeft = AnchorRaw.Top | AnchorRaw.Left,
        TopCenter = AnchorRaw.Top | AnchorRaw.Center,
        TopRight = AnchorRaw.Top | AnchorRaw.Right,

        MidLeft = AnchorRaw.Mid | AnchorRaw.Left,
        MidCenter = AnchorRaw.Mid | AnchorRaw.Center,
        MidRight = AnchorRaw.Mid | AnchorRaw.Right,

        BottomLeft = AnchorRaw.Bottom | AnchorRaw.Left,
        BottomCenter = AnchorRaw.Bottom | AnchorRaw.Center,
        BottomRight = AnchorRaw.Bottom | AnchorRaw.Right,
    }

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

        internal unsafe void Horizontal(BitmapBase src, BitmapBase dest, BlurEdgeMode edgeMode)
        {
            for (int y = 0; y < src.Height; y++)
            {
                byte* rowSource = src.Data + y * src.Stride;
                byte* rowResult = dest.Data + y * dest.Stride;
                for (int x = 0; x < src.Width; x++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, aSum = 0;
                    for (int k = 0, xSrc = x - _kernel.Length / 2; k < _kernel.Length; k++, xSrc++)
                    {
                        int xRead = xSrc;
                        if (xRead < 0 || xRead >= src.Width)
                            switch (edgeMode)
                            {
                                case BlurEdgeMode.Transparent:
                                    continue;
                                case BlurEdgeMode.Same:
                                    xRead = xRead < 0 ? 0 : src.Width - 1;
                                    break;
                                case BlurEdgeMode.Wrap:
                                    xRead = Ut.ModPositive(xRead, src.Width);
                                    break;
                                case BlurEdgeMode.Mirror:
                                    if (xRead < 0)
                                        xRead = -xRead - 1;
                                    xRead = xRead % (2 * src.Width);
                                    if (xRead >= src.Width)
                                        xRead = 2 * src.Width - xRead - 1;
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

        internal unsafe void Vertical(BitmapBase src, BitmapBase dest, BlurEdgeMode edgeMode)
        {
            for (int x = 0; x < src.Width; x++)
            {
                byte* colSource = src.Data + x * 4;
                byte* colResult = dest.Data + x * 4;
                for (int y = 0; y < src.Height; y++)
                {
                    int rSum = 0, gSum = 0, bSum = 0, aSum = 0;
                    for (int k = 0, ySrc = y - _kernel.Length / 2; k < _kernel.Length; k++, ySrc++)
                    {
                        int yRead = ySrc;
                        if (yRead < 0 || yRead >= src.Height)
                            switch (edgeMode)
                            {
                                case BlurEdgeMode.Transparent:
                                    continue;
                                case BlurEdgeMode.Same:
                                    yRead = yRead < 0 ? 0 : src.Height - 1;
                                    break;
                                case BlurEdgeMode.Wrap:
                                    yRead = Ut.ModPositive(yRead, src.Height);
                                    break;
                                case BlurEdgeMode.Mirror:
                                    if (yRead < 0)
                                        yRead = -yRead - 1;
                                    yRead = yRead % (2 * src.Height);
                                    if (yRead >= src.Height)
                                        yRead = 2 * src.Height - yRead - 1;
                                    break;
                            }
                        yRead *= src.Stride;
                        bSum += _kernel[k] * colSource[yRead + 0];
                        gSum += _kernel[k] * colSource[yRead + 1];
                        rSum += _kernel[k] * colSource[yRead + 2];
                        aSum += _kernel[k] * colSource[yRead + 3];
                    }

                    int yWrite = y * dest.Stride;
                    colResult[yWrite + 0] = (byte) (bSum / _kernelSum);
                    colResult[yWrite + 1] = (byte) (gSum / _kernelSum);
                    colResult[yWrite + 2] = (byte) (rSum / _kernelSum);
                    colResult[yWrite + 3] = (byte) (aSum / _kernelSum);
                }
            }
        }
    }
}
