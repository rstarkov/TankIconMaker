using System.Drawing;
using System.Windows.Media;
using System.Xml.Linq;
using RT.Util.Lingo;
using RT.Util.Serialization;
using WotDataLib;
using System.Text.RegularExpressions;

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

        public Anchor Anchor { get; set; }
        public static MemberTr AnchorTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Anchor); }
        public int X { get; set; }
        public static MemberTr XTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.X); }
        public int Y { get; set; }
        public static MemberTr YTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Y); }

        public int Width { get; set; }
        public static MemberTr WidthTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.TextLayer.Width); }
        public int Height { get; set; }
        public static MemberTr HeightTr(Translation tr) { return new MemberTr(tr.Category.Size, tr.TextLayer.Height); }

        public bool Baseline { get; set; }
        public static MemberTr BaselineTr(Translation tr) { return new MemberTr(tr.Category.Position, tr.TextLayer.Baseline); }

        public string Format { get; set; }
        public static MemberTr FormatTr(Translation tr) { return new MemberTr(tr.Category.TextSource, tr.TextLayer.Format); }

        #region Old
        // Old stuff, to be deleted eventually...
        [ClassifyIgnoreIfDefault]
        private int Left, Right, Top, Bottom;
        [ClassifyIgnoreIfDefault]
        private bool LeftAnchor, RightAnchor, TopAnchor, BottomAnchor;
        #endregion

        protected abstract string GetText(Tank tank);

        protected bool isSeveralValue;

        public TextLayer()
        {
            FontFamily = "Arial";
            FontSize = 8.5;
            X = 3;
            Y = 3;
            Width = 999;
            Height = 999;
            Anchor = Anchor.TopLeft;
            Baseline = false;
            FontColor = new ColorSelector(Colors.White);
        }

        public override LayerBase Clone()
        {
            var result = (TextLayer) base.Clone();
            result.FontColor = FontColor.Clone();
            return result;
        }

        private string FormatText(string format, string text)
        {
            int index;
            if ((index = format.IndexOf(":U")) > 0)
            {
                format = format.Remove(index, 2);
                text = text.ToUpper();
            }
            else if ((index = format.IndexOf(":L")) > 0)
            {
                format = format.Remove(index, 2);
                text = text.ToLower();
            }

            long numI;
            decimal numD;
            if (long.TryParse(text, out numI))
                try { text = string.Format(format, numI); }
                catch { throw new StyleUserError(App.Translation.TextLayer.FormatStringInvalidNum.Fmt(format, numI)); }
            else if (decimal.TryParse(text, out numD))
                try { text = string.Format(format, numD); }
                catch { throw new StyleUserError(App.Translation.TextLayer.FormatStringInvalidNum.Fmt(format, numD)); }
            else
                try { text = string.Format(format, text); }
                catch { throw new StyleUserError(App.Translation.TextLayer.FormatStringInvalid.Fmt(format)); }
            return text;
        }

        public override BitmapBase Draw(Tank tank)
        {
            return Ut.NewBitmapGdi(ParentStyle.IconWidth, ParentStyle.IconHeight, dc =>
            {
                string text = GetText(tank);
                if (!string.IsNullOrEmpty(Format) && (text != null))
                {
                    if (isSeveralValue)
                    {
                        string format = Format;
                        MatchCollection args = Regex.Matches(text, @"(\d+)");
                        foreach (Match match in Regex.Matches(Format, @"(\{\d+?\:?[\w\.]*\})"))
                        {
                            int numI;
                            string val = Regex.Match(match.Value, @"((?<=\{)\s*\d+\s*(?=[\:\}]))").Value;
                            if (int.TryParse(val, out numI) && (numI < args.Count))
                            {
                                format = format.Replace(match.Value, FormatText(match.Value.Replace(val, "0"), args[numI].Value));
                            }
                        }
                        text = format;
                    }
                    else
                    {
                        text = FormatText(Format, text);
                    }
                }
                var style = (FontBold ? FontStyle.Bold : 0) | (FontItalic ? FontStyle.Italic : 0);
                dc.TextRenderingHint = FontSmoothing.ToGdi();
                dc.DrawString(text, new SolidBrush(FontColor.GetColorGdi(tank)), FontFamily, FontSize, style, X, Y, Anchor,
                    Width <= 0 ? null : (int?) Width, Height <= 0 ? null : (int?) Height, Baseline);
            });
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
                    X = 80 / 2; // ok to hard-code 80 because that was the IconWidth of all styles as old as this one
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
                    Y = 24 / 2; // ok to hard-code 24 because that was the IconHeight of all styles as old as this one
                    anchor |= AnchorRaw.Mid;
                }

                Anchor = (Anchor) anchor;
            }

            Left = Right = Top = Bottom = 0;
            LeftAnchor = RightAnchor = TopAnchor = BottomAnchor = false;
        }
    }

    sealed class PropertyTextLayer : TextLayer
    {
        public override int Version { get { return 2; } }
        public override string TypeName { get { return App.Translation.PropertyTextLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.PropertyTextLayer.LayerDescription; } }

        public ExtraPropertyId Property { get; set; }
        public static MemberTr PropertyTr(Translation tr) { return new MemberTr(tr.Category.TextSource, tr.PropertyTextLayer.Property); }

        public PropertyTextLayer()
        {
            Property = new ExtraPropertyId("NameShort", null, "Wargaming");
        }

        protected override string GetText(Tank tank)
        {
            isSeveralValue = (Property.FileId == "Armor");
            return tank[Property];
        }
    }

    sealed class CustomTextLayer : TextLayer
    {
        public override int Version { get { return 2; } }
        public override string TypeName { get { return App.Translation.CustomTextLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.CustomTextLayer.LayerDescription; } }

        public ValueSelector<string> Text { get; set; }
        public static MemberTr TextTr(Translation tr) { return new MemberTr(tr.Category.TextSource, tr.CustomTextLayer.Text); }

        public CustomTextLayer()
        {
            Text = new ValueSelector<string>();

            Text.Single = "Example";

            Text.CountryNone = "";
            Text.CountryUSSR = "СССР";
            Text.CountryGermany = "Deutschland";
            Text.CountryUSA = "USA";
            Text.CountryFrance = "France";
            Text.CountryChina = "中国";
            Text.CountryUK = "UK";
            Text.CountryJapan = "日本";
            Text.CountryCzech = "Česko";
            Text.CountrySweden = "Sverige";
            Text.CountryPoland = "Polska";
            Text.CountryItaly = "Italia";

            Text.ClassNone = "";
            Text.ClassLight = "LT";
            Text.ClassMedium = "MT";
            Text.ClassHeavy = "HT";
            Text.ClassDestroyer = "D";
            Text.ClassArtillery = "A";

            Text.CategNormal = "•";
            Text.CategPremium = "♦";
            Text.CategSpecial = "∞";
            Text.CategCollector = "❖";

            Text.TierNone = "";
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
