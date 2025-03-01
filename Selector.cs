using System;
using System.ComponentModel;
using System.Windows.Media;
using RT.Util.Lingo;
using WotDataLib;
using D = System.Drawing;

namespace TankIconMaker
{
    [TypeConverter(typeof(SelectByTranslation.Conv))]
    enum SelectBy
    {
        Class,
        Country,
        Category,
        Tier,
        Single,
    }

    abstract class SelectorBase<T>
    {
        public SelectBy By { get; set; }
        public SelectBy By2 { get; set; }
        public SelectBy By3 { get; set; }
        public SelectBy By4 { get; set; }

        public T ClassLight { get; set; }
        public static MemberTr ClassLightTr(Translation tr) { return new MemberTr(tr.Selector.ClassLight); }
        public T ClassMedium { get; set; }
        public static MemberTr ClassMediumTr(Translation tr) { return new MemberTr(tr.Selector.ClassMedium); }
        public T ClassHeavy { get; set; }
        public static MemberTr ClassHeavyTr(Translation tr) { return new MemberTr(tr.Selector.ClassHeavy); }
        public T ClassDestroyer { get; set; }
        public static MemberTr ClassDestroyerTr(Translation tr) { return new MemberTr(tr.Selector.ClassDestroyer); }
        public T ClassArtillery { get; set; }
        public static MemberTr ClassArtilleryTr(Translation tr) { return new MemberTr(tr.Selector.ClassArtillery); }
        public T ClassNone { get; set; }
        public static MemberTr ClassNoneTr(Translation tr) { return new MemberTr(tr.Selector.ClassNone); }

        public T CountryUSSR { get; set; }
        public static MemberTr CountryUSSRTr(Translation tr) { return new MemberTr(tr.Selector.CountryUSSR); }
        public T CountryGermany { get; set; }
        public static MemberTr CountryGermanyTr(Translation tr) { return new MemberTr(tr.Selector.CountryGermany); }
        public T CountryUSA { get; set; }
        public static MemberTr CountryUSATr(Translation tr) { return new MemberTr(tr.Selector.CountryUSA); }
        public T CountryFrance { get; set; }
        public static MemberTr CountryFranceTr(Translation tr) { return new MemberTr(tr.Selector.CountryFrance); }
        public T CountryChina { get; set; }
        public static MemberTr CountryChinaTr(Translation tr) { return new MemberTr(tr.Selector.CountryChina); }
        public T CountryUK { get; set; }
        public static MemberTr CountryUKTr(Translation tr) { return new MemberTr(tr.Selector.CountryUK); }
        public T CountryJapan { get; set; }
        public static MemberTr CountryJapanTr(Translation tr) { return new MemberTr(tr.Selector.CountryJapan); }
        public T CountryCzech { get; set; }
        public static MemberTr CountryCzechTr(Translation tr) { return new MemberTr(tr.Selector.CountryCzech); }
        public T CountrySweden { get; set; }
        public static MemberTr CountrySwedenTr(Translation tr) { return new MemberTr(tr.Selector.CountrySweden); }
        public T CountryPoland { get; set; }
        public static MemberTr CountryPolandTr(Translation tr) { return new MemberTr(tr.Selector.CountryPoland); }
        public T CountryItaly { get; set; }
        public static MemberTr CountryItalyTr(Translation tr) { return new MemberTr(tr.Selector.CountryItaly); }
        public T CountryIntunion { get; set; }
        public static MemberTr CountryIntunionTr(Translation tr) { return new MemberTr(tr.Selector.CountryIntunion); }
        public T CountryNone { get; set; }
        public static MemberTr CountryNoneTr(Translation tr) { return new MemberTr(tr.Selector.CountryNone); }

        public T CategNormal { get; set; }
        public static MemberTr CategNormalTr(Translation tr) { return new MemberTr(tr.Selector.CategNormal); }
        public T CategPremium { get; set; }
        public static MemberTr CategPremiumTr(Translation tr) { return new MemberTr(tr.Selector.CategPremium); }
        public T CategSpecial { get; set; }
        public static MemberTr CategSpecialTr(Translation tr) { return new MemberTr(tr.Selector.CategSpecial); }
        public T CategCollector { get; set; }
        public static MemberTr CategCollectorTr(Translation tr) { return new MemberTr(tr.Selector.CategCollector); }

        public T Single { get; set; }
        public static MemberTr SingleTr(Translation tr) { return new MemberTr(tr.Selector.Single, tr.Selector.SingleDescription); }

        public SelectorBase()
            : this(default(T))
        {
        }

        public SelectorBase(T value)
        {
            By = By2 = By3 = By4 = SelectBy.Single;
            ClassLight = ClassMedium = ClassHeavy = ClassDestroyer = ClassArtillery = ClassNone
                = CountryUSSR = CountryGermany = CountryUSA = CountryFrance = CountryChina = CountryUK = CountryJapan = CountryCzech = CountrySweden = CountryPoland = CountryItaly = CountryIntunion = CountryNone
                = CategNormal = CategPremium = CategSpecial = CategCollector
                = Single = value;
        }

        public override string ToString()
        {
            return App.Translation.Misc.ExpandablePropertyDesc;
        }
    }

    sealed class ValueSelector<T> : SelectorBase<T>
    {
        public T TierNone { get; set; }
        public static MemberTr TierNoneTr(Translation tr) { return new MemberTr(tr.Selector.TierNone); }
        public T Tier1 { get; set; }
        public static MemberTr Tier1Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(1)); }
        public T Tier2 { get; set; }
        public static MemberTr Tier2Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(2)); }
        public T Tier3 { get; set; }
        public static MemberTr Tier3Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(3)); }
        public T Tier4 { get; set; }
        public static MemberTr Tier4Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(4)); }
        public T Tier5 { get; set; }
        public static MemberTr Tier5Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(5)); }
        public T Tier6 { get; set; }
        public static MemberTr Tier6Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(6)); }
        public T Tier7 { get; set; }
        public static MemberTr Tier7Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(7)); }
        public T Tier8 { get; set; }
        public static MemberTr Tier8Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(8)); }
        public T Tier9 { get; set; }
        public static MemberTr Tier9Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(9)); }
        public T Tier10 { get; set; }
        public static MemberTr Tier10Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(10)); }
        public T Tier11 { get; set; }
        public static MemberTr Tier11Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(11)); }

        public static MemberTr ByTr(Translation tr) { return new MemberTr(tr.Selector.By, getDescriptionString(tr).Fmt(2)); }
        public static MemberTr By2Tr(Translation tr) { return new MemberTr(tr.Selector.ByN.Fmt(2), getDescriptionString(tr).Fmt(3)); }
        public static MemberTr By3Tr(Translation tr) { return new MemberTr(tr.Selector.ByN.Fmt(3), getDescriptionString(tr).Fmt(4)); }
        public static MemberTr By4Tr(Translation tr) { return new MemberTr(tr.Selector.ByN.Fmt(4), getDescriptionLastString(tr)); }

        public ValueSelector()
            : this(default(T))
        {
        }

        public ValueSelector(T value)
            : base(value)
        {
            Tier1 = Tier2 = Tier3 = Tier4 = Tier5 = Tier6 = Tier7 = Tier8 = Tier9 = Tier10 = Tier11 = TierNone = value;
        }

        public T GetValue(Tank tank)
        {
            var result = getValue(tank, By);
            if (!isPassthrough(result)) return result;
            result = getValue(tank, By2);
            if (!isPassthrough(result)) return result;
            result = getValue(tank, By3);
            if (!isPassthrough(result)) return result;
            return getValue(tank, By4);
        }

        private T getValue(Tank tank, SelectBy by)
        {
            switch (by)
            {
                case SelectBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery, ClassNone);
                case SelectBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina, CountryUK, CountryJapan, CountryCzech, CountrySweden, CountryPoland, CountryItaly, CountryIntunion, CountryNone);
                case SelectBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial, CategCollector);
                case SelectBy.Tier:
                    switch (tank.Tier)
                    {
                        case 0: return TierNone;
                        case 1: return Tier1;
                        case 2: return Tier2;
                        case 3: return Tier3;
                        case 4: return Tier4;
                        case 5: return Tier5;
                        case 6: return Tier6;
                        case 7: return Tier7;
                        case 8: return Tier8;
                        case 9: return Tier9;
                        case 10: return Tier10;
                        case 11: return Tier11;
                        default: throw new Exception();
                    }
                case SelectBy.Single: return Single;
                default: throw new Exception();
            }
        }

        private static bool isPassthrough(T result)
        {
            if (typeof(T) == typeof(string) && (string) (object) result == "")
                return true;
            if (typeof(T) == typeof(Filename) && (Filename) (object) result == "")
                return true;
            if (typeof(T) == typeof(BoolWithPassthrough) && (BoolWithPassthrough) (object) result == BoolWithPassthrough.Yes)
                return true;
            return false;
        }

        private static TrString getDescriptionString(Translation tr)
        {
            if (typeof(T) == typeof(string))
                return tr.Selector.By_String_Description;
            if (typeof(T) == typeof(Filename))
                return tr.Selector.By_Filename_Description;
            if (typeof(T) == typeof(BoolWithPassthrough))
                return tr.Selector.By_Bool_Description;
            throw new Exception("397fh2k3");
        }

        private static TrString getDescriptionLastString(Translation tr)
        {
            if (typeof(T) == typeof(string))
                return tr.Selector.By_String_DescriptionLast;
            if (typeof(T) == typeof(Filename))
                return tr.Selector.By_Filename_DescriptionLast;
            if (typeof(T) == typeof(BoolWithPassthrough))
                return tr.Selector.By_Bool_DescriptionLast;
            throw new Exception("397fh2k4");
        }

        public ValueSelector<T> Clone()
        {
            return (ValueSelector<T>) MemberwiseClone();
        }
    }

    sealed class ColorSelector : SelectorBase<Color>
    {
        public Color TierNone { get; set; }
        public static MemberTr TierNoneTr(Translation tr) { return new MemberTr(tr.Selector.TierNone); }
        public Color Tier1 { get; set; }
        public static MemberTr Tier1Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(1)); }
        public Color Tier5 { get; set; }
        public static MemberTr Tier5Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(5)); }
        public Color Tier10 { get; set; }
        public static MemberTr Tier10Tr(Translation tr) { return new MemberTr(tr.Selector.TierN.Fmt(10)); }

        public static MemberTr ByTr(Translation tr) { return new MemberTr(tr.Selector.By, tr.Selector.By_Color_Description.Fmt(2)); }
        public static MemberTr By2Tr(Translation tr) { return new MemberTr(tr.Selector.ByN.Fmt(2), tr.Selector.By_Color_Description.Fmt(3)); }
        public static MemberTr By3Tr(Translation tr) { return new MemberTr(tr.Selector.ByN.Fmt(3), tr.Selector.By_Color_Description.Fmt(4)); }
        public static MemberTr By4Tr(Translation tr) { return new MemberTr(tr.Selector.ByN.Fmt(4), tr.Selector.By_Color_DescriptionLast); }

        public ColorSelector()
            : this(Colors.White)
        {
        }

        public ColorSelector(Color color)
            : base(color)
        {
            Tier1 = Tier5 = Tier10 = TierNone = color;
        }

        public ColorSelector Clone()
        {
            return (ColorSelector) MemberwiseClone();
        }

        public Color GetColorWpf(Tank tank)
        {
            var passthrough = Color.FromArgb(0, 0, 0, 0);
            var result = getColor(tank, By);
            if (result != passthrough) return result;
            result = getColor(tank, By2);
            if (result != passthrough) return result;
            result = getColor(tank, By3);
            if (result != passthrough) return result;
            return getColor(tank, By4);
        }

        private Color getColor(Tank tank, SelectBy by)
        {
            switch (by)
            {
                case SelectBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery, ClassNone);
                case SelectBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina, CountryUK, CountryJapan, CountryCzech, CountrySweden, CountryPoland, CountryItaly, CountryIntunion, CountryNone);
                case SelectBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial, CategCollector);
                case SelectBy.Tier:
                    if (tank.Tier == 0) return TierNone;
                    return tank.Tier <= 5 ? Ut.BlendColors(Tier1, Tier5, (tank.Tier - 1) / 4.0) : Ut.BlendColors(Tier5, Tier10, (tank.Tier - 5) / 5.0);
                case SelectBy.Single: return Single;
                default: throw new Exception();
            }
        }

        public D.Color GetColorGdi(Tank tank)
        {
            return GetColorWpf(tank).ToColorGdi();
        }
    }
}
