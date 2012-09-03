using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using RT.Util.Lingo;

namespace TankIconMaker.Layers
{
    abstract class TextLayer : LayerBase
    {
        public TextSmoothingStyle FontSmoothing { get; set; }
        public static MemberTr FontSmoothingTr(Translation tr) { return new MemberTr(tr.Category.Font, tr.TextLayer.FontSmoothing); }
        public string FontFamily { get; set; }
        public static MemberTr FontFamilyTr(Translation tr) { return new MemberTr(tr.Category.Font, tr.TextLayer.FontFamily); }
        public double FontSize { get; set; }
        public static MemberTr FontSizeTr(Translation tr) { return new MemberTr(tr.Category.Font, tr.TextLayer.FontSize); }
        public bool FontBold { get; set; }
        public static MemberTr FontBoldTr(Translation tr) { return new MemberTr(tr.Category.Font, tr.TextLayer.FontBold); }
        public bool FontItalic { get; set; }
        public static MemberTr FontItalicTr(Translation tr) { return new MemberTr(tr.Category.Font, tr.TextLayer.FontItalic); }
        public ColorSelector FontColor { get; set; }
        public static MemberTr FontColorTr(Translation tr) { return new MemberTr(tr.Category.Font, tr.TextLayer.FontColor); }

        public int Left { get; set; }
        public static MemberTr LeftTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Left); }
        public int Right { get; set; }
        public static MemberTr RightTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Right); }
        public int Top { get; set; }
        public static MemberTr TopTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Top); }
        public int Bottom { get; set; }
        public static MemberTr BottomTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Bottom); }

        public bool LeftAnchor { get; set; }
        public static MemberTr LeftAnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.LeftAnchor); }
        public bool RightAnchor { get; set; }
        public static MemberTr RightAnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.RightAnchor); }
        public bool TopAnchor { get; set; }
        public static MemberTr TopAnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.TopAnchor); }
        public bool BottomAnchor { get; set; }
        public static MemberTr BottomAnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.BottomAnchor); }

        public bool Baseline { get; set; }
        public static MemberTr BaselineTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Baseline); }

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

        public override LayerBase Clone()
        {
            var result = (TextLayer) base.Clone();
            result.FontColor = FontColor.Clone();
            return result;
        }

        public override BitmapBase Draw(Tank tank)
        {
            return Ut.NewBitmapGdi(dc =>
            {
                var style = (FontBold ? FontStyle.Bold : 0) | (FontItalic ? FontStyle.Italic : 0);
                dc.TextRenderingHint = FontSmoothing.ToGdi();
                dc.DrawString(GetText(tank), new Font(FontFamily, (float) FontSize, style), new SolidBrush(FontColor.GetColorGdi(tank)),
                    LeftAnchor ? (int?) Left : null, RightAnchor ? (int?) Right : null, TopAnchor ? (int?) Top : null, BottomAnchor ? (int?) Bottom : null,
                    Baseline);
            });
        }
    }

    sealed class PropertyTextLayer : TextLayer
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.PropertyTextLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.PropertyTextLayer.LayerDescription; } }

        public ExtraPropertyId Property { get; set; }
        public static MemberTr PropertyTr(Translation tr) { return new MemberTr(tr.Category.TextSource, tr.PropertyTextLayer.Property); }

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
        public override string TypeName { get { return App.Translation.CustomTextLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.CustomTextLayer.LayerDescription; } }

        public ValueSelector<string> Text { get; set; }
        public static MemberTr TextTr(Translation tr) { return new MemberTr(tr.Category.TextSource, tr.CustomTextLayer.Text); }

        public CustomTextLayer()
        {
            Text = new ValueSelector<string>();

            Text.Single = "Example";

            Text.CountryUSSR = "СССР";
            Text.CountryGermany = "Deutschland";
            Text.CountryUSA = "USA";
            Text.CountryFrance = "France";
            Text.CountryChina = "中国";
            Text.CountryUK = "UK";

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

        public override LayerBase Clone()
        {
            var result = (CustomTextLayer) base.Clone();
            result.Text = Text.Clone();
            return result;
        }

        protected override string GetText(Tank tank)
        {
            return Text.GetValue(tank);
        }
    }
}
