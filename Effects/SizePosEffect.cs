using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RT.Util;
using RT.Util.Lingo;
using RT.Util.Serialization;

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

    [TypeConverter(typeof(FilterTranslation.Conv))]
    enum Filter
    {
        Auto, // Lanczos for downsampling and Mitchell for upsampling
        Mitchell,
        Bicubic,
        Lanczos,
        Sinc256,
        Sinc1024,
    }

    class SizePosEffect : EffectBase
    {
        public override int Version { get { return 3; } }
        public override string TypeName { get { return App.Translation.EffectSizePos.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectSizePos.EffectDescription; } }

        public bool PositionByPixels { get; set; }
        public static MemberTr PositionByPixelsTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.PositionByPixels); }

        public Anchor Anchor { get; set; }
        public static MemberTr AnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.Anchor); }
        public string X { get; set; }
        public static MemberTr XTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.X); }
        public string Y { get; set; }
        public static MemberTr YTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.EffectSizePos.Y); }

        public bool SizeByPixels { get; set; }
        public static MemberTr SizeByPixelsTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.SizeByPixels); }

        public double Percentage { get { return _Percentage; } set { _Percentage = Math.Max(0.0, value); } }
        private double _Percentage;
        public static MemberTr PercentageTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Percentage); }
        public string Width { get; set; }
        public static MemberTr WidthTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Width); }
        public string Height { get; set; }
        public static MemberTr HeightTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Height); }
        public SizeMode2 SizeMode2 { get; set; }
        public static MemberTr SizeMode2Tr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.SizeMode); }
        public GrowShrinkMode GrowShrinkMode { get; set; }
        public static MemberTr GrowShrinkModeTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.GrowShrinkMode); }
        public Filter Filter { get; set; }
        public static MemberTr FilterTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.EffectSizePos.Filter); }

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
        [ClassifyIgnoreIfDefault]
        private int Left, Right, Top, Bottom;
        [ClassifyIgnoreIfDefault]
        private bool LeftAnchor, RightAnchor, TopAnchor, BottomAnchor;
        [ClassifyIgnoreIfDefault]
        private SizeModeOld SizeMode;
        enum SizeModeOld { NoChange, ByPercentage, BySizeWidthOnly, BySizeHeightOnly, BySizeWidthHeightStretch, ByPosLeftRight, ByPosTopBottom, ByPosAllFit, ByPosAllStretch, }
        #endregion

        public SizePosEffect()
        {
            PositionByPixels = true;
            SizeByPixels = true;
            PixelAlphaThreshold = 120;
            X = Y = "0";
            Anchor = Anchor.TopLeft;
            Percentage = 50;
            Width = "30";
            Height = "18";
            SizeMode2 = SizeMode2.NoChange;
            GrowShrinkMode = GrowShrinkMode.GrowAndShrink;
            Filter = Filter.Auto;
        }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            var calculator = new SizeCalculator(renderTask, layer);
            Func<string, string> describe = property =>
                "*{0}:* {1}\n".Fmt(EggsML.Escape(App.Translation.Calculator.ErrLabel_Layer), EggsML.Escape((string.IsNullOrEmpty(Layer.Name) ? "" : (Layer.Name + " – ")) + Layer.TypeName)) +
                "*{0}:* {1}\n".Fmt(EggsML.Escape(App.Translation.Calculator.ErrLabel_Effect), EggsML.Escape((string.IsNullOrEmpty(Name) ? "" : (Name + " – ")) + TypeName)) +
                "*{0}:* {1}".Fmt(EggsML.Escape(App.Translation.Calculator.ErrLabel_Property), EggsML.Escape(property));

            double ParsedWidth = Math.Max(0, calculator.Parse(Width, describe(WidthTr(App.Translation).DisplayName))),
                ParsedHeight = Math.Max(0, calculator.Parse(Height, describe(HeightTr(App.Translation).DisplayName))),
                ParsedX = calculator.Parse(X, describe(XTr(App.Translation).DisplayName)),
                ParsedY = calculator.Parse(Y, describe(YTr(App.Translation).DisplayName));

            Tank tank = renderTask.Tank;
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
                    scaleWidth = scaleHeight = ParsedWidth / (double) sourceWidth;
                    break;
                case SizeMode2.BySizeHeightOnly:
                    scaleWidth = scaleHeight = ParsedHeight / (double) sourceHeight;
                    break;
                case SizeMode2.BySizeFit:
                    scaleWidth = scaleHeight = Math.Min(ParsedWidth / (double) sourceWidth, ParsedHeight / (double) sourceHeight);
                    break;
                case SizeMode2.BySizeStretch:
                    scaleWidth = ParsedWidth / (double) sourceWidth;
                    scaleHeight = ParsedHeight / (double) sourceHeight;
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
            int tgtX = (int) ParsedX - (anchor.HasFlag(AnchorRaw.Right) ? anchorWidth - 1 : anchor.HasFlag(AnchorRaw.Center) ? (anchorWidth - 1) / 2 : 0);
            int tgtY = (int) ParsedY - (anchor.HasFlag(AnchorRaw.Bottom) ? anchorHeight - 1 : anchor.HasFlag(AnchorRaw.Mid) ? (anchorHeight - 1) / 2 : 0);
            // Location of the top left corner of the whole scaled layer image
            double x = tgtX - (PositionByPixels ? pixels.Left * scaleWidth : 0);
            double y = tgtY - (PositionByPixels ? pixels.Top * scaleHeight : 0);
            int offsetX = (PositionByPixels ? pixels.Left : 0);
            int offsetY = (PositionByPixels ? pixels.Top : 0);

            if (ShowLayerBorders || ShowPixelBorders)
            {
                using (var image = layer.ToMagickImage())
                {
                    image.Settings.StrokeWidth = 1;
                    if (ShowLayerBorders)
                    {
                        image.Settings.FillColor = ImageMagick.MagickColors.Transparent;
                        image.Settings.StrokeColor = new ImageMagick.MagickColor("aqua");
                        image.Draw(new ImageMagick.DrawableRectangle(0, 0, layer.Width - 1, layer.Height - 1));
                    }
                    if (ShowPixelBorders && !emptyPixels)
                    {
                        image.Settings.FillColor = ImageMagick.MagickColors.Transparent;
                        image.Settings.StrokeColor = new ImageMagick.MagickColor("red");
                        image.Draw(new ImageMagick.DrawableRectangle(pixels.Left, pixels.Top, pixels.Right, pixels.Bottom));
                    }
                    layer.CopyPixelsFrom(image.ToBitmapSource());
                }
            }
            BitmapResampler.Filter filter;
            switch (Filter)
            {
                case Filter.Auto: filter = null; break;
                case Filter.Mitchell: filter = new BitmapResampler.MitchellFilter(); break;
                case Filter.Bicubic: filter = new BitmapResampler.CatmullRomFilter(); break;
                case Filter.Lanczos: filter = new BitmapResampler.LanczosFilter(); break;
                case Filter.Sinc256: filter = new BitmapResampler.LanczosFilter(8); break;
                case Filter.Sinc1024: filter = new BitmapResampler.LanczosFilter(16); break;
                default: throw new Exception("SizePosEffect.Filter 4107");
            }
            layer = BitmapResampler.SizePos(layer, scaleWidth, scaleHeight, offsetX, offsetY, tgtX, tgtY, Math.Max(layer.Width, Layer.ParentStyle.IconWidth), Math.Max(layer.Height, Layer.ParentStyle.IconHeight), filter);
            if (ShowAnchor)
            {
                using (var image = layer.ToMagickImage())
                {
                    image.Settings.StrokeWidth = 1;
                    image.Settings.StrokeColor = new ImageMagick.MagickColor(255, 255, 0, 120);
                    image.Draw(new ImageMagick.DrawableLine((int) ParsedX - 1, (int) ParsedY, (int) ParsedX + 1, (int) ParsedY));
                    image.Draw(new ImageMagick.DrawableLine((int) ParsedX, (int) ParsedY - 1, (int) ParsedX, (int) ParsedY + 1));
                    layer.CopyPixelsFrom(image.ToBitmapSource());
                }
            }
            return layer;
        }

        protected override void AfterDeserialize(XElement xml)
        {
            base.AfterDeserialize(xml);

            // At one point, a field called "ConvertedFromOld" was introduced instead of increasing Version to 2. The following is a fix for this.
            if (xml.Element("ConvertedFromOld") != null && xml.Element("ConvertedFromOld").Value == "True")
                SavedByVersion = 2;

            // Upgrade to v2
            if (SavedByVersion < 2)
            {
                SavedByVersion = 2;
                AnchorRaw anchor;

                if (LeftAnchor && RightAnchor)
                {
                    X = ((Left + Right) / 2).ToString();
                    anchor = AnchorRaw.Center;
                }
                else if (LeftAnchor)
                {
                    X = (Left).ToString();
                    anchor = AnchorRaw.Left;
                }
                else if (RightAnchor)
                {
                    X = (Right).ToString();
                    anchor = AnchorRaw.Right;
                }
                else
                {
                    X = (80 / 2).ToString(); // ok to hard-code 80 because that was the IconWidth of all styles as old as this one
                    anchor = AnchorRaw.Center;
                }

                if (TopAnchor && BottomAnchor)
                {
                    Y = ((Top + Bottom) / 2).ToString();
                    anchor |= AnchorRaw.Mid;
                }
                else if (TopAnchor)
                {
                    Y = Top.ToString();
                    anchor |= AnchorRaw.Top;
                }
                else if (BottomAnchor)
                {
                    Y = Bottom.ToString();
                    anchor |= AnchorRaw.Bottom;
                }
                else
                {
                    Y = (24 / 2).ToString(); // ok to hard-code 24 because that was the IconHeight of all styles as old as this one
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
                        Width = (Right - Left + 1).ToString();
                        break;
                    case SizeModeOld.ByPosTopBottom:
                        SizeMode2 = SizeMode2.BySizeHeightOnly;
                        Height = (Bottom - Top + 1).ToString();
                        break;
                    case SizeModeOld.ByPosAllFit:
                        SizeMode2 = SizeMode2.BySizeFit;
                        Width = (Right - Left + 1).ToString();
                        Height = (Bottom - Top + 1).ToString();
                        break;
                    case SizeModeOld.ByPosAllStretch:
                        SizeMode2 = SizeMode2.BySizeStretch;
                        Width = (Right - Left + 1).ToString();
                        Height = (Bottom - Top + 1).ToString();
                        break;
                }
            }

            Left = Right = Top = Bottom = 0;
            LeftAnchor = RightAnchor = TopAnchor = BottomAnchor = false;
            SizeMode = default(SizeModeOld);
        }
    }

    class SizeCalculator : Calculator
    {
        private RenderTask _renderTask;
        private BitmapBase _layer;

        public SizeCalculator(RenderTask renderTask, BitmapBase layer)
            : base()
        {
            _renderTask = renderTask;
            _layer = layer;
        }

        protected override double EvalVariable(string variable)
        {
            var m = Regex.Match(variable, @"^([A-Za-z0-9_\-]+)\.(\w+)$");
            if (!m.Success)
                return base.EvalVariable(variable);
            var layerId = m.Groups[1].Value;
            var layerProperty = m.Groups[2].Value;
            BitmapBase varImg;
            if (layerId == "this")
            {
                varImg = _layer;
            }
            else
            {
                var varLayer = _renderTask.Style.Layers.FirstOrDefault(x => x.Id == layerId);
                if (varLayer == null)
                    throw NewParseException(App.Translation.Calculator.Err_NoLayerWithId.Fmt(layerId));
                if (_renderTask.IsLayerAlreadyReferenced(varLayer))
                    throw NewParseException(App.Translation.Calculator.Err_RecursiveLayerReference.Fmt(layerId));
                varImg = _renderTask.RenderLayer(varLayer);
            }
            var pixels = varImg.PreciseSize();
            switch (layerProperty.ToLower())
            {
                case "width": return pixels.Width;
                case "height": return pixels.Height;
                case "top": return pixels.Top;
                case "left": return pixels.Left;
                case "right": return pixels.Right;
                case "bottom": return pixels.Bottom;
                case "centerhorz": return pixels.CenterHorzD;
                case "centervert": return pixels.CenterVertD;
                default:
                    throw NewParseException(App.Translation.Calculator.Err_UnknownLayerProperty.Fmt(layerProperty));
            }

        }
    }
}
