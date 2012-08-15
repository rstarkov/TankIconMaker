using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TankIconMaker.Effects
{
    enum SizeMode
    {
        [Description("No change")]
        NoChange,
        [Description("By %")]
        ByPercentage,
        [Description("By size: width only")]
        BySizeWidthOnly,
        [Description("By size: height only")]
        BySizeHeightOnly,
        //[Description("By size: smaller of w/h")]
        //BySizeWidthHeightSmaller,
        //[Description("By size: larger of w/h")]
        //BySizeWidthHeightLarger,
        [Description("By size: stretch")]
        BySizeWidthHeightStretch,
        [Description("By pos: left/right")]
        ByPosLeftRight,
        [Description("By pos: top/bottom")]
        ByPosTopBottom,
        [Description("By pos: fit inside")]
        ByPosAllFit,
        [Description("By pos: stretch")]
        ByPosAllStretch,
    }

    enum GrowShrinkMode
    {
        [Description("Grow and shrink")]
        GrowAndShrink,
        [Description("Grow only")]
        GrowOnly,
        [Description("Shrink only")]
        ShrinkOnly,
    }

    class SizePosEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Size / Position"; } }
        public override string TypeDescription { get { return "Adjusts this layer's size and/or position. This effect is always applied before any other effects."; } }

        [Category("Position"), DisplayName("Use pixels")]
        [Description("If checked, transparent areas on the outside of the image will be ignored in position calculations. See also \"Pixel alpha threshold\".")]
        public bool PositionByPixels { get; set; }

        [Category("Position")]
        [Description("X coordinate of the leftmost text pixel. Ignored if \"Left Anchor\" is unchecked.")]
        public int Left { get; set; }
        [Category("Position")]
        [Description("X coordinate of the rightmost text pixel. Ignored if \"Right Anchor\" is unchecked.")]
        public int Right { get; set; }
        [Category("Position")]
        [Description("Y coordinate of the topmost text pixel (but see also \"Align Baselines\"). Ignored if \"Top Anchor\" is unchecked.")]
        public int Top { get; set; }
        [Category("Position")]
        [Description("Y coordinate of the bottommost text pixel (but see also \"Align Baselines\"). Ignored if \"Bottom Anchor\" is unchecked.")]
        public int Bottom { get; set; }

        [Category("Position"), DisplayName("Left Anchor")]
        [Description("If checked, the leftmost pixel of the text is anchored at the X coordinate specified by \"Left\". If \"Right Anchor\" is also checked, the text is centered between \"Left\" and \"Right\".")]
        public bool LeftAnchor { get; set; }
        [Category("Position"), DisplayName("Right Anchor")]
        [Description("If checked, the rightmost pixel of the text is anchored at the X coordinate specified by \"Right\". If \"Left Anchor\" is also checked, the text is centered between \"Left\" and \"Right\".")]
        public bool RightAnchor { get; set; }
        [Category("Position"), DisplayName("Top Anchor")]
        [Description("If checked, the topmost pixel of the text is anchored at the Y coordinate specified by \"Top\". If \"Bottom Anchor\" is also checked, the text is centered between \"Top\" and \"Bottom\".")]
        public bool TopAnchor { get; set; }
        [Category("Position"), DisplayName("Bottom Anchor")]
        [Description("If checked, the bottommost pixel of the text is anchored at the Y coordinate specified by \"Bottom\". If \"Top Anchor\" is also checked, the text is centered between \"Top\" and \"Bottom\".")]
        public bool BottomAnchor { get; set; }

        [Category("Size"), DisplayName("Use pixels")]
        [Description("If checked, transparent areas on the outside of the image will be ignored in size calculations. See also \"Pixel alpha threshold\".")]
        public bool SizeByPixels { get; set; }

        [Category("Size"), DisplayName("Resize %")]
        [Description("When Mode is \"By %\", selects the desired resize percentage.")]
        public double Percentage { get { return _Percentage; } set { _Percentage = Math.Max(0.0, value); } }
        private double _Percentage;
        [Category("Size"), DisplayName("Resize to width")]
        [Description("When Mode is one of \"By size\" modes, selects the desired width, in pixels.")]
        public int Width { get { return _Width; } set { _Width = Math.Max(0, value); } }
        private int _Width;
        [Category("Size"), DisplayName("Resize to height")]
        [Description("When Mode is one of \"By size\" modes, selects the desired height, in pixels.")]
        public int Height { get { return _Height; } set { _Height = Math.Max(0, value); } }
        private int _Height;
        [Category("Size"), DisplayName("Mode")]
        [Description("Selects one of several different resize modes, which determines how the image size is calculated.")]
        public SizeMode SizeMode { get; set; }
        [Category("Size"), DisplayName("Grow/shrink")]
        [Description("Specifies whether the image size is allowed to increase, decrease, or both, as a result of the resize.")]
        public GrowShrinkMode GrowShrinkMode { get; set; }

        [Category("General"), DisplayName("Pixel alpha threshold")]
        [Description("When sizing or positioning by pixels, determines the maximum alpha value which is still deemed as \"transparent\". Range 0..255.")]
        public int PixelAlphaThreshold { get { return _PixelAlphaThreshold; } set { _PixelAlphaThreshold = Math.Min(255, Math.Max(0, value)); } }
        private int _PixelAlphaThreshold;

        [Category("Debug"), DisplayName("Show layer borders")]
        [Description("If enabled, draws a rectangle to show the layer borders. These borders are used for size calculation if \"Size By Pixels\" is disabled.")]
        public bool ShowLayerBorders { get; set; }
        [Category("Debug"), DisplayName("Show pixel borders")]
        [Description("If enabled, draws a rectangle to show the pixel borders of the layer. Adjust the sensitivity using \"Pixel alpha threshold\". These borders are used for size calculation if \"Size By Pixels\" is enabled.")]
        public bool ShowPixelBorders { get; set; }
        [Category("Debug"), DisplayName("Show target borders")]
        [Description("If enabled, draws a rectangle to show the selected target position for the layer. Anchored borders are drawn as solid lines, non-anchored are dotted.")]
        public bool ShowTargetBorders { get; set; }

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
