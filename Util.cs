using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;
using DI = System.Drawing.Imaging;

namespace TankIconMaker
{
    static class Ut
    {
        public static IEnumerable<Tuple<int, string[]>> ReadCsvLines(string filename)
        {
            int num = 0;
            foreach (var line in File.ReadLines(filename))
            {
                num++;
                var fields = parseCsvLine(line);
                if (fields == null)
                    throw new Exception(string.Format("Couldn't parse line {0}.", num));
                yield return Tuple.Create(num, fields);
            }
        }

        private static string[] parseCsvLine(string line)
        {
            var fields = Regex.Matches(line, @"(^|(?<=,)) *(?<quote>""?)(("""")?[^""]*?)*?\k<quote> *($|(?=,))").Cast<Match>().Select(m => m.Value).ToArray();
            if (line != string.Join(",", fields))
                return null;
            return fields.Select(f => f.Contains('"') ? Regex.Replace(f, @"^ *""(.*)"" *$", "$1").Replace(@"""""", @"""") : f).ToArray();
        }

        public static BytesBitmap NewGdiBitmap()
        {
            return new BytesBitmap(80, 24, DI.PixelFormat.Format32bppArgb);
        }

        public static BytesBitmap NewGdiBitmap(Action<D.Graphics> draw)
        {
            var result = new BytesBitmap(80, 24, DI.PixelFormat.Format32bppArgb);
            using (var g = D.Graphics.FromImage(result.Bitmap))
                draw(g);
            return result;
        }

        public static Color BlendColors(Color left, Color right, double rightAmount)
        {
            return Color.FromArgb(
                a: (byte) Math.Round(left.A * (1 - rightAmount) + right.A * rightAmount),
                r: (byte) Math.Round(left.R * (1 - rightAmount) + right.R * rightAmount),
                g: (byte) Math.Round(left.G * (1 - rightAmount) + right.G * rightAmount),
                b: (byte) Math.Round(left.B * (1 - rightAmount) + right.B * rightAmount));
        }

        public unsafe static int PreciseLeft(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            byte* start = image + 3;
            byte* end = image + stride * (height - 1) + width * 4; // pointer to first byte beyond the last pixel
            for (int x = 0; x < width; x++, start += 4)
                for (byte* alpha = start; alpha < end; alpha += stride)
                    if (*alpha > alphaThreshold)
                        return x;
            return width;
        }

        public unsafe static int PreciseRight(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            byte* start = image + (width - 1) * 4 + 3;
            byte* end = image + stride * (height - 1) + width * 4; // pointer to first byte beyond the last pixel
            for (int x = width - 1; x >= 0; x--, start -= 4)
                for (byte* alpha = start; alpha < end; alpha += stride)
                    if (*alpha > alphaThreshold)
                        return x;
            return -1;
        }

        public unsafe static int PreciseTop(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            byte* start = image + left * 4 + 3;
            for (int y = 0; y < height; y++, start += stride)
            {
                byte* end = start + (width - left) * 4;
                for (byte* alpha = start; alpha < end; alpha += 4)
                    if (*alpha > alphaThreshold)
                        return y;
            }
            return height;
        }

        public unsafe static int PreciseBottom(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            byte* start = image + (height - 1) * stride + left * 4 + 3;
            for (int y = height - 1; y >= 0; y--, start -= stride)
            {
                byte* end = start + (width - left) * 4;
                for (byte* alpha = start; alpha < end; alpha += 4)
                    if (*alpha > alphaThreshold)
                        return y;
            }
            return -1;
        }

        public unsafe static PixelRect PreciseSize(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            int left = PreciseLeft(image, width, height, stride, alphaThreshold);
            int right = PreciseRight(image, width, height, stride, alphaThreshold);
            int top = PreciseTop(image, right + 1, height, stride, alphaThreshold, left);
            int bottom = PreciseBottom(image, right + 1, height, stride, alphaThreshold, left);
            return PixelRect.FromBounds(left, top, right, bottom);
        }

        public unsafe static PixelRect PreciseWidth(byte* image, int width, int height, int stride, int alphaThreshold = 0)
        {
            return PixelRect.FromLeftRight(
                PreciseLeft(image, width, height, stride, alphaThreshold),
                PreciseRight(image, width, height, stride, alphaThreshold));
        }

        public unsafe static PixelRect PreciseHeight(byte* image, int width, int height, int stride, int alphaThreshold = 0, int left = 0)
        {
            return PixelRect.FromTopBottom(
                PreciseTop(image, width, height, stride, alphaThreshold, left),
                PreciseBottom(image, width, height, stride, alphaThreshold, left));
        }

        public unsafe static PixelRect PreciseSize(this WriteableBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseSize((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseSize(this BytesBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseSize((byte*) image.BitPtr, image.Width, image.Height, image.Stride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseWidth(this WriteableBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseWidth((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseWidth(this BytesBitmap image, int alphaThreshold = 0)
        {
            var result = PreciseWidth((byte*) image.BitPtr, image.Width, image.Height, image.Stride, alphaThreshold);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseHeight(this WriteableBitmap image, int alphaThreshold = 0, int left = 0)
        {
            var result = PreciseHeight((byte*) image.BackBuffer, image.PixelWidth, image.PixelHeight, image.BackBufferStride, alphaThreshold, left);
            GC.KeepAlive(image);
            return result;
        }

        public unsafe static PixelRect PreciseHeight(this BytesBitmap image, int alphaThreshold = 0, int left = 0)
        {
            var result = PreciseHeight((byte*) image.BitPtr, image.Width, image.Height, image.Stride, alphaThreshold, left);
            GC.KeepAlive(image);
            return result;
        }

    }

    static class ExtensionMethods
    {
        public static string Fmt(this string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }

        public static bool EqualsNoCase(this string string1, string string2)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(string1, string2);
        }

        public static D.Color ToColorGdi(this Color color)
        {
            return D.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ToColorWpf(this D.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color WithAlpha(this Color color, int alpha)
        {
            if (alpha < 0 || alpha > 255) throw new ArgumentOutOfRangeException("alpha");
            return Color.FromArgb((byte) alpha, color.R, color.G, color.B);
        }

        public static BytesBitmap TextToBitmap(this D.Graphics graphics, string text, D.Font font, D.Brush brush)
        {
            var size = graphics.MeasureString(text, font); // the default is to include any overhangs into the calculation
            var bmp = new BytesBitmap((int) size.Width + 1, (int) size.Height + 1, D.Imaging.PixelFormat.Format32bppArgb);
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

        public static PixelRect DrawString(this D.Graphics graphics, string text, D.Font font, D.Brush brush,
            int? left = null, int? right = null, int? top = null, int? bottom = null, bool baseline = false)
        {
            var bmp = graphics.TextToBitmap(text, font, brush);
            var size = baseline ? bmp.PreciseWidth().WithTopBottom(0, bmp.Height - 1) : bmp.PreciseSize();

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

        public static void DrawImage(this DrawingContext context, BytesBitmap bmp)
        {
            context.DrawImage(bmp.ToWpf(), new Rect(0, 0, bmp.Width, bmp.Height));
        }

        public static void DrawImage(this DrawingContext context, BitmapSource bmp)
        {
            context.DrawImage(bmp, new Rect(0, 0, bmp.Width, bmp.Height));
        }

        public static BytesBitmap DrawImage(this BytesBitmap target, BytesBitmap source)
        {
            using (var g = D.Graphics.FromImage(target.Bitmap))
                g.DrawImageUnscaled(source.Bitmap, 0, 0);
            return target;
        }

        public static BytesBitmap GetOutline(this BytesBitmap srcBitmap, int opacity = 255)
        {
            var tgtBitmap = Ut.NewGdiBitmap();
            var src = srcBitmap.Bits;
            var tgt = tgtBitmap.Bits;
            for (int y = 0; y < srcBitmap.Height; y++)
            {
                int b = y * srcBitmap.Stride;
                int left = 0;
                int cur = src[b + 0 + 3];
                int right;
                for (int x = 0; x < srcBitmap.Width; x++, b += 4)
                {
                    right = x == srcBitmap.Width - 1 ? (byte) 0 : src[b + 4 + 3];
                    if (src[b + 3] == 0)
                    {
                        if (left != 0 || right != 0 || (y > 0 && src[b - srcBitmap.Stride + 3] > 0) || ((y < srcBitmap.Height - 1) && src[b + srcBitmap.Stride + 3] > 0))
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

        public static BytesBitmap GetBlurred(this BytesBitmap srcBitmap)
        {
            var tgtBitmap = Ut.NewGdiBitmap();
            var src = srcBitmap.Bits;
            var tgt = tgtBitmap.Bits;
            for (int y = 0; y < srcBitmap.Height; y++)
            {
                int b = y * srcBitmap.Stride;
                for (int x = 0; x < srcBitmap.Width; x++, b += 4)
                {
                    tgt[b] = tgt[b + 1] = tgt[b + 2] = 0;
                    tgt[b + 3] = src[b + 3];
                }
            }

            for (int iter = 0; iter < 5; iter++)
                for (int y = 0; y < srcBitmap.Height; y++)
                {
                    int b = y * srcBitmap.Stride;
                    int left = 0;
                    int cur = tgt[b + 0 + 3];
                    int right;
                    for (int x = 0; x < srcBitmap.Width; x++, b += 4)
                    {
                        right = x == srcBitmap.Width - 1 ? (byte) 0 : tgt[b + 4 + 3];
                        if (tgt[b + 3] == 0)
                        {
                            int top = y == 0 ? 0 : tgt[b - srcBitmap.Stride + 3];
                            int bottom = y == srcBitmap.Height - 1 ? 0 : tgt[b + srcBitmap.Stride + 3];
                            tgt[b + 3] = (byte) (((left + right + top + bottom) * 6) / 40);
                        }
                        left = cur;
                        cur = right;
                    }
                }
            return tgtBitmap;
        }

        public static BytesBitmap ToGdi(this BitmapSource bmp)
        {
            var result = new BytesBitmap(bmp.PixelWidth, bmp.PixelHeight, DI.PixelFormat.Format32bppArgb);
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            bmp.CopyPixels(result.Bits, result.Stride, 0);
            return result;
        }

        public static T Pick<T>(this Country country, T ussr, T germany, T usa, T france, T china)
        {
            switch (country)
            {
                case Country.USSR: return ussr;
                case Country.Germany: return germany;
                case Country.USA: return usa;
                case Country.France: return france;
                case Country.China: return china;
                default: throw new Exception();
            }
        }

        public static T Pick<T>(this Class class_, T light, T medium, T heavy, T destroyer, T artillery)
        {
            switch (class_)
            {
                case Class.Light: return light;
                case Class.Medium: return medium;
                case Class.Heavy: return heavy;
                case Class.Destroyer: return destroyer;
                case Class.Artillery: return artillery;
                default: throw new Exception();
            }
        }

        public static T Pick<T>(this Category class_, T normal, T premium, T special)
        {
            switch (class_)
            {
                case Category.Normal: return normal;
                case Category.Premium: return premium;
                case Category.Special: return special;
                default: throw new Exception();
            }
        }

        public static D.Text.TextRenderingHint ToGdi(this TextAntiAliasStyle style)
        {
            switch (style)
            {
                case TextAntiAliasStyle.AliasedHinted: return D.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                case TextAntiAliasStyle.AntiAliased: return D.Text.TextRenderingHint.AntiAlias;
                case TextAntiAliasStyle.AntiAliasedHinted: return D.Text.TextRenderingHint.AntiAliasGridFit;
                case TextAntiAliasStyle.ClearTypeHinted: return D.Text.TextRenderingHint.ClearTypeGridFit;
                default: throw new Exception();
            }
        }
    }

    enum TextAntiAliasStyle
    {
        AliasedHinted,
        AntiAliased,
        AntiAliasedHinted,
        ClearTypeHinted,
    }

    /// <summary>A better Int32Rect, expressly designed to represent pixel areas - hence the left/right/top/bottom/width/height are always "inclusive".</summary>
    struct PixelRect
    {
        private int _left, _width, _top, _height;
        public int Left { get { return _left; } }
        public int Top { get { return _top; } }
        public int Right { get { return _left + _width - 1; } }
        public int Bottom { get { return _top + _height - 1; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int CenterHorz { get { return _left + _width / 2; } }
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
        public PixelRect WithTopBottom(int top, int bottom) { return FromBounds(Left, top, Right, bottom); }
        public PixelRect Shifted(int deltaX, int deltaY) { return FromMixed(Left + deltaX, Top + deltaY, Width, Height); }
    }

    /// <summary>Adapted from Paint.NET and thus exactly compatible in the RGB/HSV conversion (apart from hue 360, which must be 0 instead)</summary>
    public struct ColorHSV
    {
        /// <summary>Hue, 0..359</summary>
        public int Hue { get; private set; }
        /// <summary>Saturation, 0..100</summary>
        public int Saturation { get; private set; }
        /// <summary>Value, 0..100</summary>
        public int Value { get; private set; }
        /// <summary>Alpha, range 0..255</summary>
        public int Alpha { get; private set; }

        private ColorHSV(int hue, int saturation, int value, int alpha)
            : this()
        {
            if (hue < 0 || hue > 359) throw new ArgumentException("hue");
            if (saturation < 0 || saturation > 100) throw new ArgumentException("saturation");
            if (value < 0 || value > 100) throw new ArgumentException("value");
            if (alpha < 0 || alpha > 255) throw new ArgumentException("alpha");
            Hue = hue;
            Saturation = saturation;
            Value = value;
            Alpha = alpha;
        }

        public static ColorHSV FromHSV(int hue, int saturation, int value, int alpha = 255)
        {
            return new ColorHSV(hue, saturation, value, alpha);
        }

        public D.Color ToColorGdi()
        {
            return ToColorWpf().ToColorGdi();
        }

        public Color ToColorWpf()
        {
            double h;
            double s;
            double v;

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360. Saturation
            // and value scale to be between 0 and 1.
            h = (double) Hue % 360;
            s = (double) Saturation / 100;
            v = (double) Value / 100;

            if (s == 0)
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                r = v;
                g = v;
                b = v;
            }
            else
            {
                double p;
                double q;
                double t;

                double fractionalSector;
                int sectorNumber;
                double sectorPos;

                // The color wheel consists of 6 sectors.
                // Figure out which sector you're in.
                sectorPos = h / 60;
                sectorNumber = (int) (Math.Floor(sectorPos));

                // get the fractional part of the sector.
                // That is, how many degrees into the sector
                // are you?
                fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes
                // of the color. 
                p = v * (1 - s);
                q = v * (1 - (s * fractionalSector));
                t = v * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                }
            }
            return Color.FromArgb((byte) Alpha, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
        }

        public static ColorHSV FromColor(Color color)
        {
            // In this function, R, G, and B values must be scaled 
            // to be between 0 and 1.
            // HsvColor.Hue will be a value between 0 and 360, and 
            // HsvColor.Saturation and value are between 0 and 1.

            double min;
            double max;
            double delta;

            double r = (double) color.R / 255;
            double g = (double) color.G / 255;
            double b = (double) color.B / 255;

            double h;
            double s;
            double v;

            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);
            v = max;
            delta = max - min;

            if (max == 0 || delta == 0)
            {
                // R, G, and B must be 0, or all the same.
                // In this case, S is 0, and H is undefined.
                // Using H = 0 is as good as any...
                s = 0;
                h = 0;
            }
            else
            {
                s = delta / max;
                if (r == max)
                {
                    // Between Yellow and Magenta
                    h = (g - b) / delta;
                }
                else if (g == max)
                {
                    // Between Cyan and Yellow
                    h = 2 + (b - r) / delta;
                }
                else
                {
                    // Between Magenta and Cyan
                    h = 4 + (r - g) / delta;
                }

            }
            // Scale h to be between 0 and 360. 
            // This may require adding 360, if the value
            // is negative.
            h *= 60;

            if (h < 0)
            {
                h += 360;
            }

            // Scale to the requirements of this 
            // application. All values are between 0 and 255.
            return FromHSV((int) h, (int) (s * 100), (int) (v * 100), color.A);
        }

        public ColorHSV ScaleValue(double scale)
        {
            return FromHSV(Hue, Saturation, Math.Max(0, Math.Min(100, (int) Math.Round(Value * scale))), Alpha);
        }

        public ColorHSV WithAlpha(int alpha)
        {
            return FromHSV(Hue, Saturation, Value, Alpha);
        }
    }

    class LambdaConverter<TSource, TResult> : IValueConverter
    {
        private Func<TSource, TResult> _lambda;

        public LambdaConverter(Func<TSource, TResult> lambda)
        {
            _lambda = lambda;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _lambda((TSource) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class LambdaConverter
    {
        public static LambdaConverter<TSource, TResult> New<TSource, TResult>(Func<TSource, TResult> lambda)
        {
            return new LambdaConverter<TSource, TResult>(lambda);
        }
    }

}
