using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace TankIconMaker.Layers
{
    abstract class TextLayer : LayerBaseGdi
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
        [Category("Font"), DisplayName("Colors")]
        [Description("Makes the text italic.")]
        [ExpandableObject]
        public ConfigColors Colors { get; set; }

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
            Colors = new ConfigColors();
        }

        public override void Draw(Tank tank, Graphics dc)
        {
            var style = (FontBold ? FontStyle.Bold : 0) | (FontItalic ? FontStyle.Italic : 0);
            dc.TextRenderingHint = FontSmoothing.ToGdi();
            dc.DrawString(GetText(tank), new Font(FontFamily, (float) FontSize, style), new SolidBrush(Colors.GetColorGdi(tank)),
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
        [Editor(typeof(DataSourceEditor), typeof(DataSourceEditor))]
        public ExtraPropertyId Property { get; set; }

        [Category("Text source"), DisplayName("Show tier")]
        [Description("Overrides the property selection and shows the tank’s tier instead.")]
        public bool ShowTier { get; set; }

        public PropertyTextLayer()
        {
            Property = new ExtraPropertyId("NameShortWG", "Ru", "Romkyns");
        }

        protected override string GetText(Tank tank)
        {
            return ShowTier ? tank.Tier.ToString() : tank[Property];
        }
    }

    sealed class CountryTextLayer : TextLayer
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Text / By Country"; } }
        public override string TypeDescription { get { return "Draws a fixed string based on the tank’s country using configurable font and color."; } }

        [Category("Text source"), DisplayName("USSR")]
        [Description("The string to be displayed for tanks whose country is USSR.")]
        public string TextUSSR { get; set; }
        [Category("Text source"), DisplayName("Germany")]
        [Description("The string to be displayed for tanks whose country is Germany.")]
        public string TextGermany { get; set; }
        [Category("Text source"), DisplayName("USA")]
        [Description("The string to be displayed for tanks whose country is USA.")]
        public string TextUSA { get; set; }
        [Category("Text source"), DisplayName("France")]
        [Description("The string to be displayed for tanks whose country is France.")]
        public string TextFrance { get; set; }
        [Category("Text source"), DisplayName("China")]
        [Description("The string to be displayed for tanks whose country is China.")]
        public string TextChina { get; set; }

        public CountryTextLayer()
        {
            TextUSSR = "СССР";
            TextGermany = "Deutschland";
            TextUSA = "USA";
            TextFrance = "France";
            TextChina = "中国";
        }

        protected override string GetText(Tank tank)
        {
            return tank.Country.Pick(TextUSSR, TextGermany, TextUSA, TextFrance, TextChina);
        }
    }

    sealed class ClassTextLayer : TextLayer
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Text / By Class"; } }
        public override string TypeDescription { get { return "Draws a fixed string based on the tank’s class (artillery, destoryer, etc) using configurable font and color."; } }

        [Category("Text source"), DisplayName("Light tank")]
        [Description("The string to be displayed for tanks whose class is Light Tank.")]
        public string TextLight { get; set; }
        [Category("Text source"), DisplayName("Medium tank")]
        [Description("The string to be displayed for tanks whose class is Medium Tank.")]
        public string TextMedium { get; set; }
        [Category("Text source"), DisplayName("Heavy tank")]
        [Description("The string to be displayed for tanks whose class is Heavy Tank.")]
        public string TextHeavy { get; set; }
        [Category("Text source"), DisplayName("Destroyer")]
        [Description("The string to be displayed for tanks whose class is Destroyer.")]
        public string TextDestroyer { get; set; }
        [Category("Text source"), DisplayName("Artillery")]
        [Description("The string to be displayed for tanks whose class is Artillery.")]
        public string TextArtillery { get; set; }

        public ClassTextLayer()
        {
            TextLight = "LT";
            TextMedium = "MT";
            TextHeavy = "HT";
            TextDestroyer = "D";
            TextArtillery = "A";
        }

        protected override string GetText(Tank tank)
        {
            return tank.Class.Pick(TextLight, TextMedium, TextHeavy, TextDestroyer, TextArtillery);
        }
    }

    sealed class CategoryTextLayer : TextLayer
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Text / By Category"; } }
        public override string TypeDescription { get { return "Draws a fixed string based on the tank’s category (normal, premium, special) using configurable font and color."; } }

        [Category("Text source"), DisplayName("Normal")]
        [Description("The string to be displayed for tanks whose category is Normal.")]
        public string TextNormal { get; set; }
        [Category("Text source"), DisplayName("Premium")]
        [Description("The string to be displayed for tanks whose category is Premium.")]
        public string TextPremium { get; set; }
        [Category("Text source"), DisplayName("Special")]
        [Description("The string to be displayed for tanks whose category is Special.")]
        public string TextSpecial { get; set; }

        public CategoryTextLayer()
        {
            TextNormal = "•";
            TextPremium = "♦";
            TextSpecial = "∞";
        }

        protected override string GetText(Tank tank)
        {
            return tank.Category.Pick(TextNormal, TextPremium, TextSpecial);
        }
    }
}
