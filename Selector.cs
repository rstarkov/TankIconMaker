using System;
using System.ComponentModel;
using System.Windows.Media;
using D = System.Drawing;

namespace TankIconMaker
{
    enum SelectValueBy
    {
        [Description("Artillery / Destroyer / Light / ...")]
        Class,
        [Description("USSR / Germany / USA / ...")]
        Country,
        [Description("Normal / premium / special")]
        Category,
        [Description("Single value")]
        Single,
    }

    enum SelectColorBy
    {
        [Description("Artillery / Destroyer / Light / ...")]
        Class,
        [Description("USSR / Germany / USA / ...")]
        Country,
        [Description("Normal / premium / special")]
        Category,
        [Description("Tier (1 / 5 / 10)")]
        Tier,
        [Description("Single color")]
        Single,
    }

    abstract class SelectorBase<T>
    {
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
            ClassLight = ClassMedium = ClassHeavy = ClassDestroyer = ClassArtillery
                = CountryUSSR = CountryGermany = CountryUSA = CountryFrance = CountryChina
                = CategNormal = CategPremium = CategSpecial
                = Single = value;
        }
    }

    sealed class ValueSelector<T> : SelectorBase<T>
    {
        [DisplayName("By")]
        public SelectValueBy By { get; set; }

        public ValueSelector()
            : this(default(T))
        {
        }

        public ValueSelector(T value)
        {
            By = SelectValueBy.Single;
        }

        public T GetValue(Tank tank)
        {
            switch (By)
            {
                case SelectValueBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery);
                case SelectValueBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina);
                case SelectValueBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial);
                case SelectValueBy.Single: return Single;
                default: throw new Exception();
            }
        }
    }

    sealed class ColorSelector : SelectorBase<Color>
    {
        [DisplayName("By")]
        public SelectColorBy By { get; set; }

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
            By = SelectColorBy.Single;
            Tier1 = Tier5 = Tier10 = color;
        }

        public Color GetColorWpf(Tank tank)
        {
            switch (By)
            {
                case SelectColorBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery);
                case SelectColorBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina);
                case SelectColorBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial);
                case SelectColorBy.Tier: return tank.Tier <= 5 ? Ut.BlendColors(Tier1, Tier5, (tank.Tier - 1) / 4.0) : Ut.BlendColors(Tier5, Tier10, (tank.Tier - 5) / 5.0);
                case SelectColorBy.Single: return Single;
                default: throw new Exception();
            }
        }

        public D.Color GetColorGdi(Tank tank)
        {
            return GetColorWpf(tank).ToColorGdi();
        }
    }
}
