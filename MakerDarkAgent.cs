using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using D = System.Drawing;

namespace TankIconMaker
{
    class MakerDarkAgent : MakerBaseWpf
    {
        public override string Name { get { return "Dark Agent (Black Spy replica)"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }
        public override string Description { get { return "Recreates my favourite icons by Black_Spy in TankIconMaker. Kudos to Black_Spy for the design!"; } }

        // Category: Background
        [Category("Background"), DisplayName("Opacity")]
        [Description("The opacity of the colored background.")]
        public int BackOpacity { get { return _opacity; } set { _opacity = Math.Max(0, Math.Min(255, value)); } }
        private int _opacity;

        [Category("Background"), DisplayName("Color: light tank")]
        [Description("The color of the background used for light tanks.")]
        public Color BackColorLight { get; set; }

        [Category("Background"), DisplayName("Color: medium tank")]
        [Description("The color of the background used for medium tanks.")]
        public Color BackColorMedium { get; set; }

        [Category("Background"), DisplayName("Color: heavy tank")]
        [Description("The color of the background used for heavy tanks.")]
        public Color BackColorHeavy { get; set; }

        [Category("Background"), DisplayName("Color: destroyer")]
        [Description("The color of the background used for destroyers.")]
        public Color BackColorDestroyer { get; set; }

        [Category("Background"), DisplayName("Color: artillery")]
        [Description("The color of the background used for artillery.")]
        public Color BackColorArtillery { get; set; }

        // Category: Tank name
        [Category("Tank name"), DisplayName("Data source")]
        [Description("Choose the name of the property that supplies the data for the bottom right location.")]
        [Editor(typeof(DataSourceEditor), typeof(DataSourceEditor))]
        public string NameData { get; set; }

        [Category("Tank name"), DisplayName("Color: normal")]
        [Description("Used to color the name of all tanks that can be freely bought for silver in the game.")]
        public Color NameColorNormal { get; set; }

        [Category("Tank name"), DisplayName("Color: premium")]
        [Description("Used to color the name of all premium tanks, that is tanks that can be freely bought for gold in the game.")]
        public Color NameColorPremium { get; set; }

        [Category("Tank name"), DisplayName("Color: special")]
        [Description("Used to color the name of all special tanks, that is tanks that cannot normally be bought in the game.")]
        public Color NameColorSpecial { get; set; }

        [Category("Tank name"), DisplayName("Rendering style")]
        [Description("Determines how the tank name should be anti-aliased.")]
        public TextAntiAliasStyle NameAntiAlias { get; set; }

        // Category: Tank tier
        [Category("Tank tier"), DisplayName("Tier  1 Color")]
        [Description("The color of the tier text for tier 1 tanks. The color for tiers 2..9 is interpolated based on tier 1, 5 and 10 settings.")]
        public Color Tier1Color { get; set; }

        [Category("Tank tier"), DisplayName("Tier  5 Color")]
        [Description("The color of the tier text for tier 5 tanks. The color for tiers 2..9 is interpolated based on tier 1, 5 and 10 settings.")]
        public Color Tier5Color { get; set; }

        [Category("Tank tier"), DisplayName("Tier 10 Color")]
        [Description("The color of the tier text for tier 10 tanks. The color for tiers 2..9 is interpolated based on tier 1, 5 and 10 settings.")]
        public Color Tier10Color { get; set; }

        [Category("Tank tier"), DisplayName("Rendering style")]
        [Description("Determines how the tank name should be anti-aliased.")]
        public TextAntiAliasStyle TierAntiAlias { get; set; }

        // Category: Tank image
        [Category("Tank image"), DisplayName("Overhang")]
        [Description("Indicates whether the tank picture should overhang above and below the background rectangle, fit strictly inside it or be clipped to its size.")]
        public OverhangStyle Overhang { get; set; }
        public enum OverhangStyle { Overhang, [Description("Fit inside frame")] Fit, [Description("Clip to frame")] Clip }

        [Category("Tank image"), DisplayName("Style")]
        [Description("Specifies one of the built-in image styles to use.")]
        public ImageStyle Style { get; set; }
        public enum ImageStyle { Contour, [Description("3D")] ThreeD }

        private Pen _outline, _outlineInner;
        private Brush _lightBackground, _mediumBackground, _heavyBackground, _destroyerBackground, _artilleryBackground;

        public MakerDarkAgent()
        {
            BackOpacity = 180;
            BackColorLight = Color.FromRgb(35, 140, 35);
            BackColorMedium = Color.FromRgb(150, 127, 37);
            BackColorHeavy = Color.FromRgb(99, 99, 99);
            BackColorDestroyer = Color.FromRgb(41, 83, 160);
            BackColorArtillery = Color.FromRgb(181, 47, 47);

            NameData = "NameShortWG/Ru/Romkyns";
            NameColorNormal = Color.FromRgb(210, 210, 210);
            NameColorPremium = Colors.Yellow;
            NameColorSpecial = Color.FromRgb(242, 98, 103);
            NameAntiAlias = TextAntiAliasStyle.ClearType;

            TierAntiAlias = TextAntiAliasStyle.ClearType;
            Tier1Color = NameColorNormal;
            Tier5Color = Colors.White;
            Tier10Color = Colors.Red;

            Overhang = OverhangStyle.Overhang;
            Style = ImageStyle.ThreeD;

            _outline = new Pen(Brushes.Black, 1); _outline.Freeze();
            _outlineInner = new Pen(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 1); _outlineInner.Freeze();
        }

        public override void Initialize()
        {
            _lightBackground = makeBackgroundBrush(BackColorLight.WithAlpha(BackOpacity));
            _mediumBackground = makeBackgroundBrush(BackColorMedium.WithAlpha(BackOpacity));
            _heavyBackground = makeBackgroundBrush(BackColorHeavy.WithAlpha(BackOpacity));
            _destroyerBackground = makeBackgroundBrush(BackColorDestroyer.WithAlpha(BackOpacity));
            _artilleryBackground = makeBackgroundBrush(BackColorArtillery.WithAlpha(BackOpacity));
        }

        private Brush makeBackgroundBrush(Color color)
        {
            var hsv = ColorHSV.FromColor(color);
            var result = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(hsv.ToColorWpf(), 0.1),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 0.49),
                    new GradientStop(hsv.ScaleValue(0.39).ToColorWpf(), 0.51),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 0.9),
                },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
            };
            result.Freeze();
            return result;
        }

        public override void DrawTank(Tank tank, DrawingContext dc)
        {
            dc.DrawRectangle(tank.Class.Pick(_lightBackground, _mediumBackground, _heavyBackground, _destroyerBackground, _artilleryBackground),
                _outline, new Rect(0.5, 1.5, 79, 21));
            dc.DrawRectangle(null, _outlineInner, new Rect(1.5, 2.5, 77, 19));

            PixelRect nameSize = new PixelRect(), tierSize = new PixelRect();

            var nameFont = new D.Font("Arial", 8.5f);
            var nameBrush = new D.SolidBrush(tank.Category.Pick(NameColorNormal, NameColorPremium, NameColorSpecial).ToColorGdi());
            var nameLayer = Ut.NewBitmapGdi((D.Graphics g) =>
            {
                g.TextRenderingHint = NameAntiAlias.ToGdi();
                nameSize = g.DrawString(tank[NameData], nameFont, nameBrush, right: 80 - 4, bottom: 24 - 3, baseline: true);
            });
            nameLayer.DrawImage(nameLayer.GetOutline(NameAntiAlias == TextAntiAliasStyle.Aliased ? 255 : 180));
            nameLayer = nameLayer.GetBlurred().DrawImage(nameLayer);

            var tierFont = new D.Font("Arial", 8.5f);
            var tierColor = tank.Tier <= 5 ? Ut.BlendColors(Tier1Color, Tier5Color, (tank.Tier - 1) / 4.0) : Ut.BlendColors(Tier5Color, Tier10Color, (tank.Tier - 5) / 5.0);
            var tierBrush = new D.SolidBrush(tierColor.ToColorGdi());
            var tierLayer = Ut.NewBitmapGdi((D.Graphics g) =>
            {
                g.TextRenderingHint = TierAntiAlias.ToGdi();
                tierSize = g.DrawString(tank.Tier.ToString(), tierFont, tierBrush, left: 3, top: 4);
            });
            tierLayer.DrawImage(tierLayer.GetOutline(TierAntiAlias == TextAntiAliasStyle.Aliased ? 255 : 180));
            tierLayer = tierLayer.GetBlurred().DrawImage(tierLayer);

            try
            {
                var image = Style == ImageStyle.Contour ? tank.LoadImageContourWpf() : tank.LoadImage3DWpf();
                var minmax = Ut.PreciseWidth(image, 100);
                if (Overhang != OverhangStyle.Overhang)
                    dc.PushClip(new RectangleGeometry(new Rect(1, 2, 78, 20)));
                else
                {
                    // Fade out the top and bottom couple of pixels
                    unsafe
                    {
                        byte* ptr, end;
                        int h = image.PixelHeight;
                        ptr = (byte*) image.BackBuffer + 0 * image.BackBufferStride + 3; end = ptr + image.PixelWidth * 4; for (; ptr < end; ptr += 4) *ptr = (byte) (*ptr * 0.25);
                        ptr = (byte*) image.BackBuffer + 1 * image.BackBufferStride + 3; end = ptr + image.PixelWidth * 4; for (; ptr < end; ptr += 4) *ptr = (byte) (*ptr * 0.6);
                        ptr = (byte*) image.BackBuffer + (h - 2) * image.BackBufferStride + 3; end = ptr + image.PixelWidth * 4; for (; ptr < end; ptr += 4) *ptr = (byte) (*ptr * 0.6);
                        ptr = (byte*) image.BackBuffer + (h - 1) * image.BackBufferStride + 3; end = ptr + image.PixelWidth * 4; for (; ptr < end; ptr += 4) *ptr = (byte) (*ptr * 0.25);
                    }
                }
                double height = Overhang == OverhangStyle.Fit ? 20 : 24;
                double scale = height / image.Height;
                dc.DrawImage(image, new Rect(
                    Math.Max(10 - minmax.Left * scale, (tierSize.Right + nameSize.Left) / 2 - scale * minmax.CenterHorz),
                    Overhang == OverhangStyle.Fit ? 2 : 0,
                    image.Width * scale, height));
                if (Overhang != OverhangStyle.Overhang)
                    dc.Pop();
            }
            catch { }

            dc.DrawImage(nameLayer);
            dc.DrawImage(tierLayer);
        }
    }

}
