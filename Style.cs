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
    }

    class ConfigColors
    {
        [DisplayName("Color by")]
        public ColorBy ColorBy { get; set; }

        [Category("By class"), DisplayName("Light tank")]
        public Color ClassLight { get; set; }
        [Category("By class"), DisplayName("Medium tank")]
        public Color ClassMedium { get; set; }
        [Category("By class"), DisplayName("Heavy tank")]
        public Color ClassHeavy { get; set; }
        [Category("By class"), DisplayName("Destroyer")]
        public Color ClassDestroyer { get; set; }
        [Category("By class"), DisplayName("Artillery")]
        public Color ClassArtillery { get; set; }

        [Category("By country"), DisplayName("USSR")]
        public Color CountryUSSR { get; set; }
        [Category("By country"), DisplayName("Germany")]
        public Color CountryGermany { get; set; }
        [Category("By country"), DisplayName("USA")]
        public Color CountryUSA { get; set; }
        [Category("By country"), DisplayName("France")]
        public Color CountryFrance { get; set; }
        [Category("By country"), DisplayName("China")]
        public Color CountryChina { get; set; }

        [Category("By category"), DisplayName("Normal")]
        public Color CategNormal { get; set; }
        [Category("By category"), DisplayName("Premium")]
        public Color CategPremium { get; set; }
        [Category("By category"), DisplayName("Special")]
        public Color CategSpecial { get; set; }

        public Color GetColorWpf(Tank tank)
        {
            switch (ColorBy)
            {
                case ColorBy.Class: return tank.Class.Pick(ClassLight, ClassMedium, ClassHeavy, ClassDestroyer, ClassArtillery);
                case ColorBy.Country: return tank.Country.Pick(CountryUSSR, CountryGermany, CountryUSA, CountryFrance, CountryChina);
                case ColorBy.Category: return tank.Category.Pick(CategNormal, CategPremium, CategSpecial);
                default: throw new Exception();
            }
        }

        public D.Color GetColorGdi(Tank tank)
        {
            return GetColorWpf(tank).ToColorGdi();
        }
    }
}
