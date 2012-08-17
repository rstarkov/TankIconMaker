using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    [TypeConverter(typeof(SizeModeTranslation.Conv))]
    enum SizeMode
    {
        NoChange,
        ByPercentage,
        BySizeWidthOnly,
        BySizeHeightOnly,
        BySizeWidthHeightStretch,
        ByPosLeftRight,
        ByPosTopBottom,
        ByPosAllFit,
        ByPosAllStretch,
    }

    [TypeConverter(typeof(GrowShrinkModeTranslation.Conv))]
    enum GrowShrinkMode
    {
        GrowAndShrink,
        GrowOnly,
        ShrinkOnly,
    }

    class SizePosEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.EffectSizePos.EffectName; } }
        public override string TypeDescription { get { return Program.Translation.EffectSizePos.EffectDescription; } }

        public bool PositionByPixels { get; set; }
        public static MemberTr PositionByPixelsTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.PositionByPixels); }

        public int Left { get; set; }
        public static MemberTr LeftTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.Left); }
        public int Right { get; set; }
        public static MemberTr RightTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.Right); }
        public int Top { get; set; }
        public static MemberTr TopTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.Top); }
        public int Bottom { get; set; }
        public static MemberTr BottomTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.Bottom); }

        public bool LeftAnchor { get; set; }
        public static MemberTr LeftAnchorTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.LeftAnchor); }
        public bool RightAnchor { get; set; }
        public static MemberTr RightAnchorTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.RightAnchor); }
        public bool TopAnchor { get; set; }
        public static MemberTr TopAnchorTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.TopAnchor); }
        public bool BottomAnchor { get; set; }
        public static MemberTr BottomAnchorTr(Translation tr) { return new MemberTr(Program.Translation.CategoryPosition, Program.Translation.EffectSizePos.BottomAnchor); }

        public bool SizeByPixels { get; set; }
        public static MemberTr SizeByPixelsTr(Translation tr) { return new MemberTr(Program.Translation.CategorySize, Program.Translation.EffectSizePos.SizeByPixels); }

        public double Percentage { get { return _Percentage; } set { _Percentage = Math.Max(0.0, value); } }
        private double _Percentage;
        public static MemberTr PercentageTr(Translation tr) { return new MemberTr(Program.Translation.CategorySize, Program.Translation.EffectSizePos.Percentage); }
        public int Width { get { return _Width; } set { _Width = Math.Max(0, value); } }
        private int _Width;
        public static MemberTr WidthTr(Translation tr) { return new MemberTr(Program.Translation.CategorySize, Program.Translation.EffectSizePos.Width); }
        public int Height { get { return _Height; } set { _Height = Math.Max(0, value); } }
        private int _Height;
        public static MemberTr HeightTr(Translation tr) { return new MemberTr(Program.Translation.CategorySize, Program.Translation.EffectSizePos.Height); }
        public SizeMode SizeMode { get; set; }
        public static MemberTr SizeModeTr(Translation tr) { return new MemberTr(Program.Translation.CategorySize, Program.Translation.EffectSizePos.SizeMode); }
        public GrowShrinkMode GrowShrinkMode { get; set; }
        public static MemberTr GrowShrinkModeTr(Translation tr) { return new MemberTr(Program.Translation.CategorySize, Program.Translation.EffectSizePos.GrowShrinkMode); }

        public int PixelAlphaThreshold { get { return _PixelAlphaThreshold; } set { _PixelAlphaThreshold = Math.Min(255, Math.Max(0, value)); } }
        private int _PixelAlphaThreshold;
        public static MemberTr PixelAlphaThresholdTr(Translation tr) { return new MemberTr(Program.Translation.CategoryGeneral, Program.Translation.EffectSizePos.PixelAlphaThreshold); }

        public bool ShowLayerBorders { get; set; }
        public static MemberTr ShowLayerBordersTr(Translation tr) { return new MemberTr(Program.Translation.CategoryDebug, Program.Translation.EffectSizePos.ShowLayerBorders); }
        public bool ShowPixelBorders { get; set; }
        public static MemberTr ShowPixelBordersTr(Translation tr) { return new MemberTr(Program.Translation.CategoryDebug, Program.Translation.EffectSizePos.ShowPixelBorders); }
        public bool ShowTargetBorders { get; set; }
        public static MemberTr ShowTargetBordersTr(Translation tr) { return new MemberTr(Program.Translation.CategoryDebug, Program.Translation.EffectSizePos.ShowTargetBorders); }

        public SizePosEffect()
        {
            PositionByPixels = true;
            SizeByPixels = true;
            PixelAlphaThreshold = 120;
            Left = 0;
            Top = 0;
            Right = 79;
            Bottom = 23;
            LeftAnchor = TopAnchor = true;
            Percentage = 50;
            Width = 30;
            Height = 18;
            SizeMode = SizeMode.NoChange;
            GrowShrinkMode = GrowShrinkMode.GrowAndShrink;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            PixelRect pixels = PixelRect.FromMixed(0, 0, layer.Width, layer.Height);
            if (ShowPixelBorders || PositionByPixels || (SizeByPixels && SizeMode != SizeMode.NoChange && SizeMode != SizeMode.ByPercentage))
                pixels = layer.PreciseSize(PixelAlphaThreshold);
            double scaleWidth = 1, scaleHeight = 1;

            var size = SizeByPixels ? pixels : PixelRect.FromMixed(0, 0, layer.Width, layer.Height);
            if (SizeMode != SizeMode.NoChange)
            {
                switch (SizeMode)
                {
                    case SizeMode.ByPercentage:
                        scaleWidth = scaleHeight = Percentage / 100.0;
                        break;
                    case SizeMode.BySizeWidthOnly:
                        scaleWidth = scaleHeight = Width / (double) size.Width;
                        break;
                    case SizeMode.BySizeHeightOnly:
                        scaleWidth = scaleHeight = Height / (double) size.Height;
                        break;
                    //case SizeMode.BySizeWidthHeightSmaller:
                    //    scaleWidth = scaleHeight = Math.Min(Width / (double) size.Width, Height / (double) size.Height);
                    //    break;
                    //case SizeMode.BySizeWidthHeightLarger:
                    //    scaleWidth = scaleHeight = Math.Max(Width / (double) size.Width, Height / (double) size.Height);
                    //    break;
                    case SizeMode.BySizeWidthHeightStretch:
                        scaleWidth = Width / (double) size.Width;
                        scaleHeight = Height / (double) size.Height;
                        break;
                    case SizeMode.ByPosLeftRight:
                        scaleWidth = scaleHeight = (Right - Left + 1) / (double) size.Width;
                        break;
                    case SizeMode.ByPosTopBottom:
                        scaleWidth = scaleHeight = (Bottom - Top + 1) / (double) size.Height;
                        break;
                    case SizeMode.ByPosAllFit:
                        scaleWidth = scaleHeight = Math.Min((Right - Left + 1) / (double) size.Width, (Bottom - Top + 1) / (double) size.Height);
                        break;
                    case SizeMode.ByPosAllStretch:
                        scaleWidth = (Right - Left + 1) / (double) size.Width;
                        scaleHeight = (Bottom - Top + 1) / (double) size.Height;
                        break;
                    default:
                        throw new Exception("7924688");
                }
            }

            if (GrowShrinkMode == GrowShrinkMode.GrowOnly)
            {
                scaleWidth = Math.Max(1.0, scaleWidth);
                scaleHeight = Math.Max(1.0, scaleHeight);
            }
            else if (GrowShrinkMode == GrowShrinkMode.ShrinkOnly)
            {
                scaleWidth = Math.Min(1.0, scaleWidth);
                scaleHeight = Math.Min(1.0, scaleHeight);
            }

            size = PositionByPixels ? pixels : PixelRect.FromMixed(0, 0, layer.Width, layer.Height);

            double x = (LeftAnchor && RightAnchor) ? (Left + Right) / 2.0 - size.CenterHorzD * scaleWidth
                : LeftAnchor ? Left - size.Left * scaleWidth
                : RightAnchor ? Right - size.Right * scaleWidth
                : 0;
            double y = (TopAnchor && BottomAnchor) ? (Top + Bottom) / 2.0 - size.CenterVertD * scaleHeight
                : TopAnchor ? Top - size.Top * scaleHeight
                : BottomAnchor ? Bottom - size.Bottom * scaleHeight
                : 0;

            var gdi = layer.ToBitmapGdi();
            if (ShowLayerBorders || ShowPixelBorders)
                using (var dc = Graphics.FromImage(gdi.Bitmap))
                {
                    if (ShowLayerBorders)
                        dc.DrawRectangle(Pens.Aqua, 0, 0, layer.Width - 1, layer.Height - 1);
                    if (ShowPixelBorders)
                        dc.DrawRectangle(Pens.Red, pixels.Left, pixels.Top, pixels.Width - 1, pixels.Height - 1);
                }

            var result = new BitmapGdi(Math.Max(layer.Width, 80), Math.Max(layer.Height, 24));
            using (var dc = Graphics.FromImage(result.Bitmap))
            {
                dc.InterpolationMode = InterpolationMode.HighQualityBicubic;
                if (scaleWidth == 1 && scaleHeight == 1)
                    dc.DrawImage(gdi.Bitmap, (int) (x + 0.5), (int) (y + 0.5));
                else
                    dc.DrawImage(gdi.Bitmap, (float) x, (float) y, (float) (gdi.Width * scaleWidth), (float) (gdi.Height * scaleHeight));

                if (ShowTargetBorders)
                {
                    using (var normal = new Pen(Color.FromArgb(120, Color.Yellow), 0))
                    using (var dashed = new Pen(Color.FromArgb(120, Color.Yellow), 0) { DashStyle = DashStyle.Custom, DashPattern = new[] { 1f, 1f } })
                    {
                        dc.DrawLine(TopAnchor ? normal : dashed, Left, Top, Right - 1, Top);
                        dc.DrawLine(RightAnchor ? normal : dashed, Right, Top, Right, Bottom - 1);
                        dc.DrawLine(BottomAnchor ? normal : dashed, Right, Bottom, Left + 1, Bottom);
                        dc.DrawLine(LeftAnchor ? normal : dashed, Left, Bottom, Left, Top + 1);
                    }
                }
            }
            GC.KeepAlive(gdi);
            return result;
        }
    }
}
