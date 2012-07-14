using System;
using System.ComponentModel;
using System.Windows.Media;
using D = System.Drawing;

namespace TankIconMaker
{
    enum SelectBy
    {
        [Description("Artillery • Destroyer • Light • etc")]
        Class,
        [Description("USSR • Germany • USA • etc")]
        Country,
        [Description("Normal • premium • special")]
        Category,
        [Description("Tier (1 .. 10)")]
        Tier,
        [Description("Single value")]
        Single,
    }

    abstract class SelectorBase<T>
    {
        [DisplayName("By")]
        [Description("Determines which color set to use. Any colors that are fully transparent black (#00000000) are selected according to \"By (#2)\".")]
        public SelectBy By { get; set; }
        [DisplayName("By (#2)")]
        [Description("Determines which color set to use. Any colors that are fully transparent black (#00000000) are selected according to \"By (#3)\".")]
        public SelectBy By2 { get; set; }
        [DisplayName("By (#3)")]
        [Description("Determines which color set to use. Any colors that are fully transparent black (#00000000) are selected according to \"By (#4)\".")]
        public SelectBy By3 { get; set; }
        [DisplayName("By (#4)")]
        [Description("Determines which color set to use. Any colors that are fully transparent black (#00000000) will remain fully transparent.")]
        public SelectBy By4 { get; set; }

        [DisplayName("Class: Light tank")]
        public T ClassLight { get; set; }
        [DisplayName("Class: Medium tank")]
        public T ClassMedium { get; set; }
        [DisplayName("Class: Heavy tank")]
        public T ClassHeavy { get; set; }
        [DisplayName("Class: Destroyer")]
        public T ClassDestroyer { get; set; }
        [DisplayName("Class: Artillery")]
        public T ClassArtillery { get; set; }

        [DisplayName("Country: USSR")]
        public T CountryUSSR { get; set; }
        [DisplayName("Country: Germany")]
        public T CountryGermany { get; set; }
        [DisplayName("Country: USA")]
        public T CountryUSA { get; set; }
        [DisplayName("Country: France")]
        public T CountryFrance { get; set; }
        [DisplayName("Country: China")]
        public T CountryChina { get; set; }
        [DisplayName("Country: UK")]
        public T CountryUK { get; set; }

        [DisplayName("Categ.: Normal")]
        public T CategNormal { get; set; }
        [DisplayName("Categ.: Premium")]
        public T CategPremium { get; set; }
        [DisplayName("Categ.: Special")]
        public T CategSpecial { get; set; }

        [DisplayName("Single")]
        public T Single { get; set; }

        public SelectorBase()
            : this(default(T))
        {
        }

        public SelectorBase(T value)
        {
            By = By2 = By3 = By4 = SelectBy.Single;
            ClassLight = ClassMedium = ClassHeavy = ClassDestroyer = ClassArtillery
                = CountryUSSR = CountryGermany = CountryUSA = CountryFrance = CountryChina = CountryUK
                = CategNormal = CategPremium = CategSpecial
                = Single = value;
        }
    }

    sealed class ValueSelector<T> : SelectorBase<T>
    {
        [DisplayName("Tier:  1")]
        public T Tier1 { get; set; }
        [DisplayName("Tier:  2")]
        public T Tier2 { get; set; }
        [DisplayName("Tier:  3")]
        public T Tier3 { get; set; }
        [DisplayName("Tier:  4")]
        public T Tier4 { get; set; }
        [DisplayName("Tier:  5")]
        public T Tier5 { get; set; }
        [DisplayName("Tier:  6")]
        public T Tier6 { get; set; }
        [DisplayName("Tier:  7")]
        public T Tier7 { get; set; }
        [DisplayName("Tier:  8")]
        public T Tier8 { get; set; }
        [DisplayName("Tier:  9")]
        public T Tier9 { get; set; }
        [DisplayName("Tier: 10")]
        public T Tier10 { get; set; }

        public ValueSelector()
            : this(default(T))
        {
        }

        public ValueSelector(T value)
            : base(value)
        {
            Tier1 = Tier2 = Tier3 = Tier4 = Tier5 = Tier6 = Tier7 = Tier8 = Tier9 = Tier10 = value;
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
                case SelectBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery);
                case SelectBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina, CountryUK);
                case SelectBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial);
                case SelectBy.Tier:
                    switch (tank.Tier)
                    {
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
            if (typeof(T) == typeof(BoolWithPassthrough) && (BoolWithPassthrough) (object) result == BoolWithPassthrough.Passthrough)
                return true;
            return false;
        }
    }

    sealed class ColorSelector : SelectorBase<Color>
    {
        [DisplayName("Tier:  1")]
        public Color Tier1 { get; set; }
        [DisplayName("Tier:  5")]
        public Color Tier5 { get; set; }
        [DisplayName("Tier: 10")]
        public Color Tier10 { get; set; }

        public ColorSelector()
            : this(Colors.White)
        {
        }

        public ColorSelector(Color color)
            : base(color)
        {
            Tier1 = Tier5 = Tier10 = color;
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
                case SelectBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery);
                case SelectBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina, CountryUK);
                case SelectBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial);
                case SelectBy.Tier: return tank.Tier <= 5 ? Ut.BlendColors(Tier1, Tier5, (tank.Tier - 1) / 4.0) : Ut.BlendColors(Tier5, Tier10, (tank.Tier - 5) / 5.0);
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
