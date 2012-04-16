using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using RT.Util.Xml;
using D = System.Drawing;

namespace TankIconMaker
{
    sealed class Style : INotifyPropertyChanged, IComparable<Style>
    {
        /// <summary>The name of the style (chosen by the artist).</summary>
        public string Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged("Name"); NotifyPropertyChanged("Display"); } }
        private string _Name;

        /// <summary>The name of the author of this style.</summary>
        public string Author { get { return _Author; } set { _Author = value; NotifyPropertyChanged("Author"); NotifyPropertyChanged("Display"); } }
        private string _Author;

        public string Display { get { return "{2}{0} (by {1})".Fmt(Name, Author, BuiltIn ? "[built-in] " : ""); } }

        /// <summary>A list of layers that this style is made up of.</summary>
        public ObservableCollection<LayerBase> Layers = new ObservableCollection<LayerBase>();

        /// <summary>Determines whether this style is a built-in one. Not saved as a setting.</summary>
        [XmlIgnore]
        public bool BuiltIn { get; set; }

        private void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public int CompareTo(Style other)
        {
            if (other == null)
                return -1;
            int result = StringComparer.OrdinalIgnoreCase.Compare(_Name, other._Name);
            if (result != 0)
                return result;
            return StringComparer.OrdinalIgnoreCase.Compare(_Author, other._Author);
        }

        public override string ToString()
        {
            return Display;
        }

        public Style Clone()
        {
            var result = MemberwiseClone() as Style;
            result.PropertyChanged = (_, __) => { };
            result.Layers = new ObservableCollection<LayerBase>(Layers.Select(l => l.Clone()));
            return result;
        }
    }

    /// <summary>
    /// Thrown from a layer or effect implementation to report an error that the user can fix or needs to know about.
    /// </summary>
    class StyleUserError : Exception
    {
        public StyleUserError(string message) : base(message) { }
    }

    enum ColorBy
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

    class ColorScheme
    {
        [DisplayName("By")]
        public ColorBy ColorBy { get; set; }

        [DisplayName("Class: Light tank")]
        public Color ClassLight { get; set; }
        [DisplayName("Class: Medium tank")]
        public Color ClassMedium { get; set; }
        [DisplayName("Class: Heavy tank")]
        public Color ClassHeavy { get; set; }
        [DisplayName("Class: Destroyer")]
        public Color ClassDestroyer { get; set; }
        [DisplayName("Class: Artillery")]
        public Color ClassArtillery { get; set; }

        [DisplayName("Country: USSR")]
        public Color CountryUSSR { get; set; }
        [DisplayName("Country: Germany")]
        public Color CountryGermany { get; set; }
        [DisplayName("Country: USA")]
        public Color CountryUSA { get; set; }
        [DisplayName("Country: France")]
        public Color CountryFrance { get; set; }
        [DisplayName("Country: China")]
        public Color CountryChina { get; set; }

        [DisplayName("Categ.: Normal")]
        public Color CategNormal { get; set; }
        [DisplayName("Categ.: Premium")]
        public Color CategPremium { get; set; }
        [DisplayName("Categ.: Special")]
        public Color CategSpecial { get; set; }

        [DisplayName("Tier:  1")]
        public Color Tier1 { get; set; }
        [DisplayName("Tier:  5")]
        public Color Tier5 { get; set; }
        [DisplayName("Tier: 10")]
        public Color Tier10 { get; set; }

        [DisplayName("Single color")]
        public Color Single { get; set; }

        public ColorScheme()
            : this(Colors.White)
        {
        }

        public ColorScheme(Color color)
        {
            ColorBy = TankIconMaker.ColorBy.Single;
            ClassLight = ClassMedium = ClassHeavy = ClassDestroyer = ClassArtillery
                = CountryUSSR = CountryGermany = CountryUSA = CountryFrance = CountryChina
                = CategNormal = CategPremium = CategSpecial
                = Tier1 = Tier5 = Tier10
                = Single = color;
        }

        public Color GetColorWpf(Tank tank)
        {
            switch (ColorBy)
            {
                case ColorBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery);
                case ColorBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina);
                case ColorBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial);
                case ColorBy.Tier: return tank.Tier <= 5 ? Ut.BlendColors(Tier1, Tier5, (tank.Tier - 1) / 4.0) : Ut.BlendColors(Tier5, Tier10, (tank.Tier - 5) / 5.0);
                case ColorBy.Single: return Single;
                default: throw new Exception();
            }
        }

        public D.Color GetColorGdi(Tank tank)
        {
            return GetColorWpf(tank).ToColorGdi();
        }
    }
}
