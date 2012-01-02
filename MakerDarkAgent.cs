using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;
using DI = System.Drawing.Imaging;

namespace TankIconMaker
{
    class MakerDarkAgent : MakerBaseWpf
    {
        public override string Name { get { return "Dark Agent (Black Spy replica)"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

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

        [Category("Tank tier"), DisplayName("Rendering style")]
        [Description("Determines how the tank name should be anti-aliased.")]
        public TextAntiAliasStyle TierAntiAlias { get; set; }
        [Category("Tank tier"), DisplayName("Tier  1 Color")]
        [Description("The color of the tier text for tier 1 tanks. The color for tiers 2..9 is interpolated based on tier 1, 5 and 10 settings.")]
        public Color Tier1Color { get; set; }
        [Category("Tank tier"), DisplayName("Tier  5 Color")]
        [Description("The color of the tier text for tier 5 tanks. The color for tiers 2..9 is interpolated based on tier 1, 5 and 10 settings.")]
        public Color Tier5Color { get; set; }
        [Category("Tank tier"), DisplayName("Tier 10 Color")]
        [Description("The color of the tier text for tier 10 tanks. The color for tiers 2..9 is interpolated based on tier 1, 5 and 10 settings.")]
        public Color Tier10Color { get; set; }

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

            NameColorNormal = Colors.White;
            NameColorPremium = Colors.Yellow;
            NameColorSpecial = Color.FromRgb(242, 98, 103);
            NameAntiAlias = TextAntiAliasStyle.AliasedHinted;

            TierAntiAlias = TextAntiAliasStyle.AliasedHinted;
            Tier1Color = Colors.Lime;
            Tier5Color = Colors.White;
            Tier10Color = Colors.Red;

            _outline = new Pen(Brushes.Black, 1); _outline.Freeze();
            _outlineInner = new Pen(new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), 1); _outlineInner.Freeze();
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
                    new GradientStop(hsv.ToColorWpf(), 0),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 0.49),
                    new GradientStop(hsv.ScaleValue(0.39).ToColorWpf(), 0.51),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 1),
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

            var nameFont = new D.Font("Arial", 8.5f);
            var nameBrush = new D.SolidBrush(tank.Category.Pick(NameColorNormal, NameColorPremium, NameColorSpecial).ToColorGdi());
            var nameLayer = Ut.NewGdiBitmap((D.Graphics g) =>
            {
                g.TextRenderingHint = NameAntiAlias.ToGdi();
                g.DrawString(tank["OfficialName"], nameFont, nameBrush, right: 80 - 4, bottom: 24 - 5);
            });
            nameLayer.DrawImage(nameLayer.GetOutline(NameAntiAlias == TextAntiAliasStyle.AliasedHinted ? 255 : 180));
            nameLayer = nameLayer.GetBlurred().DrawImage(nameLayer);
            dc.DrawImage(nameLayer);

            var tierFont = new D.Font("Arial", 8.5f);
            var tierColor = tank.Tier <= 5 ? Ut.BlendColors(Tier1Color, Tier5Color, (tank.Tier - 1) / 4.0) : Ut.BlendColors(Tier5Color, Tier10Color, (tank.Tier - 5) / 5.0);
            var tierBrush = new D.SolidBrush(tierColor.ToColorGdi());
            var tierLayer = Ut.NewGdiBitmap((D.Graphics g) =>
            {
                g.TextRenderingHint = TierAntiAlias.ToGdi();
                g.DrawString(tank.Tier.ToString(), tierFont, tierBrush, left: 3, top: 1);
            });
            tierLayer.DrawImage(tierLayer.GetOutline(TierAntiAlias == TextAntiAliasStyle.AliasedHinted ? 255 : 180));
            tierLayer = tierLayer.GetBlurred().DrawImage(tierLayer);
            dc.DrawImage(tierLayer);
        }
    }

}
