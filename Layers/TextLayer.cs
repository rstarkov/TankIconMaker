using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace TankIconMaker.Layers
{
    abstract class TextLayer : LayerBase
    {
        [Category("Font"), DisplayName("Smoothing")]
        [Description("Determines how the tank name should be anti-aliased.")]
        public TextSmoothingStyle FontSmoothing { get; set; }
        [Category("Font"), DisplayName("Family")]
        [Description("Font family.")]
        public string FontFamily { get; set; }
        [Category("Font"), DisplayName("Size")]
        [Description("Font size.")]
        public double FontSize { get; set; }
        [Category("Font"), DisplayName("Bold")]
        [Description("Makes the text bold.")]
        public bool FontBold { get; set; }
        [Category("Font"), DisplayName("Italic")]
        [Description("Makes the text italic.")]
        public bool FontItalic { get; set; }
        [Category("Font"), DisplayName("Color")]
        [Description("Specifies the text color.")]
        public ColorSelector FontColor { get; set; }

        [Category("Position")]
        [Description("X coordinate of the leftmost text pixel. Ignored if \"Left Anchor\" is false.")]
        public int Left { get; set; }
        [Category("Position")]
        [Description("X coordinate of the rightmost text pixel. Ignored if \"Right Anchor\" is false.")]
        public int Right { get; set; }
        [Category("Position")]
        [Description("Y coordinate of the topmost text pixel (but see also \"Align Baselines\"). Ignored if \"Top Anchor\" is false.")]
        public int Top { get; set; }
        [Category("Position")]
        [Description("Y coordinate of the bottommost text pixel (but see also \"Align Baselines\"). Ignored if \"Bottom Anchor\" is false.")]
        public int Bottom { get; set; }

        [Category("Position"), DisplayName("Left Anchor")]
        [Description("If true, the leftmost pixel of the text is anchored at the X coordinate specified by \"Left\". If \"Right Anchor\" is also true, the text is centered between \"Left\" and \"Right\".")]
        public bool LeftAnchor { get; set; }
        [Category("Position"), DisplayName("Right Anchor")]
        [Description("If true, the rightmost pixel of the text is anchored at the X coordinate specified by \"Right\". If \"Left Anchor\" is also true, the text is centered between \"Left\" and \"Right\".")]
        public bool RightAnchor { get; set; }
        [Category("Position"), DisplayName("Top Anchor")]
        [Description("If true, the topmost pixel of the text is anchored at the Y coordinate specified by \"Top\". If \"Bottom Anchor\" is also true, the text is centered between \"Top\" and \"Bottom\".")]
        public bool TopAnchor { get; set; }
        [Category("Position"), DisplayName("Bottom Anchor")]
        [Description("If true, the bottommost pixel of the text is anchored at the Y coordinate specified by \"Bottom\". If \"Top Anchor\" is also true, the text is centered between \"Top\" and \"Bottom\".")]
        public bool BottomAnchor { get; set; }

        [Category("Position"), DisplayName("Align Baselines")]
        [Description("Consider the words \"more\" and \"type\", top-anchored at pixel 0. If \"Align Baselines\" is false, the word \"more\" will be displayed slightly higher, so as to touch pixel 0. If true, the baselines will align instead, and the topmost pixel of \"more\" will actually be below pixel 0.")]
        public bool Baseline { get; set; }

        protected abstract string GetText(Tank tank);

        public TextLayer()
        {
            FontFamily = "Arial";
            FontSize = 8.5;
            Left = 3;
            LeftAnchor = true;
            Top = 3;
            TopAnchor = true;
            Right = 80 - 3;
            Bottom = 24 - 3;
            Baseline = false;
            FontColor = new ColorSelector(Colors.White);
        }

        public override BitmapBase Draw(Tank tank)
        {
            var style = (FontBold ? FontStyle.Bold : 0) | (FontItalic ? FontStyle.Italic : 0);
            dc.TextRenderingHint = FontSmoothing.ToGdi();
            dc.DrawString(GetText(tank), new Font(FontFamily, (float) FontSize, style), new SolidBrush(FontColor.GetColorGdi(tank)),
                LeftAnchor ? (int?) Left : null, RightAnchor ? (int?) Right : null, TopAnchor ? (int?) Top : null, BottomAnchor ? (int?) Bottom : null,
                Baseline);
        }
    }

    sealed class PropertyTextLayer : TextLayer
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Text / Property"; } }
        public override string TypeDescription { get { return "Draws a tank’s custom property using configurable font and color."; } }

        [Category("Text source"), DisplayName("Property")]
        [Description("Choose the name of the property that supplies the data for the bottom right location.")]
        public ExtraPropertyId Property { get; set; }

        public PropertyTextLayer()
        {
            Property = new ExtraPropertyId("NameShortWG", "Ru", "Romkyns");
        }

        protected override string GetText(Tank tank)
        {
            return tank[Property];
        }
    }

    sealed class CustomTextLayer : TextLayer
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Text / Custom"; } }
        public override string TypeDescription { get { return "Draws a fixed string based on a specified property of a tank."; } }

        [Category("Text source")]
        [Description("The string to be displayed.")]
        public ValueSelector<string> Text { get; set; }

        public CustomTextLayer()
        {
            Text = new ValueSelector<string>();

            Text.Single = "Example";

            Text.CountryUSSR = "СССР";
            Text.CountryGermany = "Deutschland";
            Text.CountryUSA = "USA";
            Text.CountryFrance = "France";
            Text.CountryChina = "中国";

            Text.ClassLight = "LT";
            Text.ClassMedium = "MT";
            Text.ClassHeavy = "HT";
            Text.ClassDestroyer = "D";
            Text.ClassArtillery = "A";

            Text.CategNormal = "•";
            Text.CategPremium = "♦";
            Text.CategSpecial = "∞";

            Text.Tier1 = "I";
            Text.Tier2 = "II";
            Text.Tier3 = "III";
            Text.Tier4 = "IV";
            Text.Tier5 = "V";
            Text.Tier6 = "VI";
            Text.Tier7 = "VII";
            Text.Tier8 = "VIII";
            Text.Tier9 = "IX";
            Text.Tier10 = "X";
        }

        protected override string GetText(Tank tank)
        {
            return Text.GetValue(tank);
        }
    }
}
