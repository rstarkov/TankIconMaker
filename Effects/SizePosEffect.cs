using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using RT.Util.Lingo;
using RT.Util.Xml;

namespace TankIconMaker.Effects
{
    [TypeConverter(typeof(SizeModeTranslation.Conv))]
    enum SizeMode2
    {
        NoChange,
        ByPercentage,
        BySizeWidthOnly,
        BySizeHeightOnly,
        BySizeFit,
        BySizeStretch,
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
        public override string TypeName { get { return App.Translation.EffectSizePos.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectSizePos.EffectDescription; } }

        public bool PositionByPixels { get; set; }
        public static MemberTr PositionByPixelsTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.PositionByPixels); }

        public Anchor Anchor { get; set; }
        public static MemberTr AnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.Anchor); }
        public int X { get; set; }
        public static MemberTr XTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.X); }
        public int Y { get; set; }
        public static MemberTr YTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.Y); }

        public bool SizeByPixels { get; set; }
        public static MemberTr SizeByPixelsTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.SizeByPixels); }

        public double Percentage { get { return _Percentage; } set { _Percentage = Math.Max(0.0, value); } }
        private double _Percentage;
        public static MemberTr PercentageTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Percentage); }
        public int Width { get { return _Width; } set { _Width = Math.Max(0, value); } }
        private int _Width;
        public static MemberTr WidthTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Width); }
        public int Height { get { return _Height; } set { _Height = Math.Max(0, value); } }
        private int _Height;
        public static MemberTr HeightTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Height); }
        public SizeMode2 SizeMode2 { get; set; }
        public static MemberTr SizeMode2Tr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.SizeMode); }
        public GrowShrinkMode GrowShrinkMode { get; set; }
        public static MemberTr GrowShrinkModeTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.GrowShrinkMode); }

        public int PixelAlphaThreshold { get { return _PixelAlphaThreshold; } set { _PixelAlphaThreshold = Math.Min(255, Math.Max(0, value)); } }
        private int _PixelAlphaThreshold;
        public static MemberTr PixelAlphaThresholdTr(Translation tr) { return new MemberTr(tr.Category.General, tr.EffectSizePos.PixelAlphaThreshold); }

        public bool ShowLayerBorders { get; set; }
        public static MemberTr ShowLayerBordersTr(Translation tr) { return new MemberTr(tr.Category.Debug, tr.EffectSizePos.ShowLayerBorders); }
        public bool ShowPixelBorders { get; set; }
        public static MemberTr ShowPixelBordersTr(Translation tr) { return new MemberTr(tr.Category.Debug, tr.EffectSizePos.ShowPixelBorders); }
        public bool ShowAnchor { get; set; }
        public static MemberTr ShowAnchorTr(Translation tr) { return new MemberTr(tr.Category.Debug, tr.EffectSizePos.ShowAnchor); }

        #region Old
        // Old stuff, to be deleted eventually...
        private bool ConvertedFromOld = false;
        [XmlIgnoreIfDefault]
        private int Left, Right, Top, Bottom;
        [XmlIgnoreIfDefault]
        private bool LeftAnchor, RightAnchor, TopAnchor, BottomAnchor;
        [XmlIgnoreIfDefault]
        private SizeModeOld SizeMode;
        enum SizeModeOld { NoChange, ByPercentage, BySizeWidthOnly, BySizeHeightOnly, BySizeWidthHeightStretch, ByPosLeftRight, ByPosTopBottom, ByPosAllFit, ByPosAllStretch, }
        #endregion

        public SizePosEffect()
        {
            PositionByPixels = true;
            SizeByPixels = true;
            PixelAlphaThreshold = 120;
            X = Y = 0;
            Anchor = Anchor.TopLeft;
            Percentage = 50;
            Width = 30;
            Height = 18;
            SizeMode2 = SizeMode2.NoChange;
            GrowShrinkMode = GrowShrinkMode.GrowAndShrink;

            // Old stuff, to be deleted eventually
            Left = 0;
            Top = 0;
            Right = 79;
            Bottom = 23;
            LeftAnchor = TopAnchor = true;
            SizeMode = SizeModeOld.NoChange;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            var pixels = PixelRect.FromMixed(0, 0, layer.Width, layer.Height);
            if (ShowPixelBorders || PositionByPixels || (SizeByPixels && SizeMode2 != SizeMode2.NoChange && SizeMode2 != SizeMode2.ByPercentage))
                pixels = layer.PreciseSize(PixelAlphaThreshold);
            bool emptyPixels = pixels.Width <= 0 || pixels.Height <= 0;
            if (emptyPixels)
                pixels = PixelRect.FromMixed(0, 0, layer.Width, layer.Height);

            double scaleWidth, scaleHeight;
            int sourceWidth = SizeByPixels ? pixels.Width : layer.Width;
            int sourceHeight = SizeByPixels ? pixels.Height : layer.Height;
            switch (SizeMode2)
            {
                case SizeMode2.NoChange:
                    scaleWidth = scaleHeight = 1;
                    break;
                case SizeMode2.ByPercentage:
                    scaleWidth = scaleHeight = Percentage / 100.0;
                    break;
                case SizeMode2.BySizeWidthOnly:
                    scaleWidth = scaleHeight = Width / (double) sourceWidth;
                    break;
                case SizeMode2.BySizeHeightOnly:
                    scaleWidth = scaleHeight = Height / (double) sourceHeight;
                    break;
                case SizeMode2.BySizeFit:
                    scaleWidth = scaleHeight = Math.Min(Width / (double) sourceWidth, Height / (double) sourceHeight);
                    break;
                case SizeMode2.BySizeStretch:
                    scaleWidth = Width / (double) sourceWidth;
                    scaleHeight = Height / (double) sourceHeight;
                    break;
                default:
                    throw new Exception("7924688");
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

            var anchor = (AnchorRaw) Anchor;
            int anchorWidth = (int) Math.Ceiling((PositionByPixels ? pixels.Width : layer.Width) * scaleWidth);
            int anchorHeight = (int) Math.Ceiling((PositionByPixels ? pixels.Height : layer.Height) * scaleHeight);
            // Location of the top left corner of the anchored rectangle
            int tgtX = X - (anchor.HasFlag(AnchorRaw.Right) ? anchorWidth - 1 : anchor.HasFlag(AnchorRaw.Center) ? (anchorWidth - 1) / 2 : 0);
            int tgtY = Y - (anchor.HasFlag(AnchorRaw.Bottom) ? anchorHeight - 1 : anchor.HasFlag(AnchorRaw.Mid) ? (anchorHeight - 1) / 2 : 0);
            // Location of the top left corner of the whole scaled layer image
            double x = tgtX - (PositionByPixels ? pixels.Left * scaleWidth : 0);
            double y = tgtY - (PositionByPixels ? pixels.Top * scaleHeight : 0);

            var src = layer.ToBitmapGdi();
            if (ShowLayerBorders || ShowPixelBorders)
                using (var dc = System.Drawing.Graphics.FromImage(src.Bitmap))
                {
                    if (ShowLayerBorders)
                        dc.DrawRectangle(System.Drawing.Pens.Aqua, 0, 0, layer.Width - 1, layer.Height - 1);
                    if (ShowPixelBorders && !emptyPixels)
                        dc.DrawRectangle(System.Drawing.Pens.Red, pixels.Left, pixels.Top, pixels.Width - 1, pixels.Height - 1);
                }

#if true
            // Using GDI: sharp-ish downscaling, but imprecise boundaries
            var result = new BitmapGdi(Math.Max(layer.Width, 80), Math.Max(layer.Height, 24));
            using (var dc = Graphics.FromImage(result.Bitmap))
            {
                dc.InterpolationMode = InterpolationMode.HighQualityBicubic;
                dc.DrawImage(src.Bitmap, (float) x, (float) y, (float) (src.Width * scaleWidth), (float) (src.Height * scaleHeight));
                if (ShowAnchor)
                    using (var pen = new Pen(Color.FromArgb(120, Color.Yellow), 0))
                    {
                        dc.DrawLine(pen, X - 1, Y, X + 1, Y);
                        dc.DrawLine(pen, X, Y - 1, X, Y + 1);
                    }
            }
#else
            // Using WPF: precise boundaries but rather blurry downscaling
            var result = Ut.NewBitmapWpf(dc =>
            {
                var img = src.ToBitmapWpf().UnderlyingImage;

                var group = new System.Windows.Media.DrawingGroup();
                System.Windows.Media.RenderOptions.SetBitmapScalingMode(group, System.Windows.Media.BitmapScalingMode.Fant);
                group.Children.Add(new System.Windows.Media.ImageDrawing(img, new System.Windows.Rect(x, y, src.Width * scaleWidth, src.Height * scaleHeight)));
                dc.DrawDrawing(group);

                if (ShowTargetPosition)
                {
                    var pen = new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 0)), 1);
                    dc.DrawLine(pen, new System.Windows.Point(X - 1 + 0.5, Y + 0.5), new System.Windows.Point(X + 1 + 0.5, Y + 0.5));
                    dc.DrawLine(pen, new System.Windows.Point(X + 0.5, Y - 1 + 0.5), new System.Windows.Point(X + 0.5, Y + 1 + 0.5));
                }
            }, Math.Max(layer.Width, 80), Math.Max(layer.Height, 24));
#endif

            GC.KeepAlive(src);
            return result.ToBitmapRam();
        }

        protected override void ActualAfterXmlDeclassify()
        {
            base.ActualAfterXmlDeclassify();

            if (!ConvertedFromOld)
            {
                AnchorRaw anchor;

                if (LeftAnchor && RightAnchor)
                {
                    X = (Left + Right) / 2;
                    anchor = AnchorRaw.Center;
                }
                else if (LeftAnchor)
                {
                    X = Left;
                    anchor = AnchorRaw.Left;
                }
                else if (RightAnchor)
                {
                    X = Right;
                    anchor = AnchorRaw.Right;
                }
                else
                {
                    X = 80 / 2;
                    anchor = AnchorRaw.Center;
                }

                if (TopAnchor && BottomAnchor)
                {
                    Y = (Top + Bottom) / 2;
                    anchor |= AnchorRaw.Mid;
                }
                else if (TopAnchor)
                {
                    Y = Top;
                    anchor |= AnchorRaw.Top;
                }
                else if (BottomAnchor)
                {
                    Y = Bottom;
                    anchor |= AnchorRaw.Bottom;
                }
                else
                {
                    Y = 24 / 2;
                    anchor |= AnchorRaw.Mid;
                }

                Anchor = (Anchor) anchor;

                switch (SizeMode)
                {
                    case SizeModeOld.NoChange:
                        SizeMode2 = SizeMode2.NoChange;
                        break;
                    case SizeModeOld.ByPercentage:
                        SizeMode2 = SizeMode2.ByPercentage;
                        break;
                    case SizeModeOld.BySizeWidthOnly:
                        SizeMode2 = SizeMode2.BySizeWidthOnly;
                        break;
                    case SizeModeOld.BySizeHeightOnly:
                        SizeMode2 = SizeMode2.BySizeHeightOnly;
                        break;
                    case SizeModeOld.BySizeWidthHeightStretch:
                        SizeMode2 = SizeMode2.BySizeStretch;
                        break;
                    case SizeModeOld.ByPosLeftRight:
                        SizeMode2 = SizeMode2.BySizeWidthOnly;
                        Width = Right - Left + 1;
                        break;
                    case SizeModeOld.ByPosTopBottom:
                        SizeMode2 = SizeMode2.BySizeHeightOnly;
                        Height = Bottom - Top + 1;
                        break;
                    case SizeModeOld.ByPosAllFit:
                        SizeMode2 = SizeMode2.BySizeFit;
                        Width = Right - Left + 1;
                        Height = Bottom - Top + 1;
                        break;
                    case SizeModeOld.ByPosAllStretch:
                        SizeMode2 = SizeMode2.BySizeStretch;
                        Width = Right - Left + 1;
                        Height = Bottom - Top + 1;
                        break;
                }
            }

            Left = Right = Top = Bottom = 0;
            LeftAnchor = RightAnchor = TopAnchor = BottomAnchor = false;
            SizeMode = default(SizeModeOld);
            ConvertedFromOld = true;
        }
    }
}
