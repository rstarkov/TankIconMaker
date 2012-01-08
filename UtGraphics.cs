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

        public static BitmapGdi ToGdi(this BitmapSource bmp)
        {
            var result = new BitmapGdi(bmp.PixelWidth, bmp.PixelHeight);
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            bmp.CopyPixels(result.Bytes, result.Stride, 0);
            return result;
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

        public static PixelRect DrawString(this D.Graphics graphics, string text, D.Font font, D.Brush brush,
            int? left = null, int? right = null, int? top = null, int? bottom = null, bool baseline = false)
        {
            var bmp = graphics.TextToBitmap(text, font, brush);
            var size = baseline ? bmp.PreciseWidth().WithTopBottom(0, bmp.PixelHeight - 1) : bmp.PreciseSize();

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

        public static void DrawImage(this DrawingContext context, BitmapGdi bmp)
        {
            context.DrawImage(bmp.ToWpf(), new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        }

        public static void DrawImage(this DrawingContext context, BitmapSource bmp)
        {
            context.DrawImage(bmp, new Rect(0, 0, bmp.Width, bmp.Height));
        }

        public static BitmapGdi DrawImage(this BitmapGdi target, BitmapGdi source)
        {
            using (var g = D.Graphics.FromImage(target.Bitmap))
                g.DrawImageUnscaled(source.Bitmap, 0, 0);
            return target;
        }

        public static BitmapGdi GetOutline(this BitmapGdi srcBitmap, int opacity = 255)
        {
            var tgtBitmap = Ut.NewBitmapGdi();
            var src = srcBitmap.Bytes;
            var tgt = tgtBitmap.Bytes;
            for (int y = 0; y < srcBitmap.PixelHeight; y++)
            {
                int b = y * srcBitmap.Stride;
                int left = 0;
                int cur = src[b + 0 + 3];
                int right;
                for (int x = 0; x < srcBitmap.PixelWidth; x++, b += 4)
                {
                    right = x == srcBitmap.PixelWidth - 1 ? (byte) 0 : src[b + 4 + 3];
                    if (src[b + 3] == 0)
                    {
                        if (left != 0 || right != 0 || (y > 0 && src[b - srcBitmap.Stride + 3] > 0) || ((y < srcBitmap.PixelHeight - 1) && src[b + srcBitmap.Stride + 3] > 0))
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

        public static BitmapGdi GetBlurred(this BitmapGdi srcBitmap)
        {
            var tgtBitmap = Ut.NewBitmapGdi();
            var src = srcBitmap.Bytes;
            var tgt = tgtBitmap.Bytes;
            for (int y = 0; y < srcBitmap.PixelHeight; y++)
            {
                int b = y * srcBitmap.Stride;
                for (int x = 0; x < srcBitmap.PixelWidth; x++, b += 4)
                {
                    tgt[b] = tgt[b + 1] = tgt[b + 2] = 0;
                    tgt[b + 3] = src[b + 3];
                }
            }

            for (int iter = 0; iter < 5; iter++)
                for (int y = 0; y < srcBitmap.PixelHeight; y++)
                {
                    int b = y * srcBitmap.Stride;
                    int left = 0;
                    int cur = tgt[b + 0 + 3];
                    int right;
                    for (int x = 0; x < srcBitmap.PixelWidth; x++, b += 4)
                    {
                        right = x == srcBitmap.PixelWidth - 1 ? (byte) 0 : tgt[b + 4 + 3];
                        if (tgt[b + 3] == 0)
                        {
                            int top = y == 0 ? 0 : tgt[b - srcBitmap.Stride + 3];
                            int bottom = y == srcBitmap.PixelHeight - 1 ? 0 : tgt[b + srcBitmap.Stride + 3];
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
}
