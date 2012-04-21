using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using RT.Util.Xml;

namespace TankIconMaker
{
    enum StyleKind { Original, Current, BuiltIn, User }

    sealed class Style : INotifyPropertyChanged, IComparable<Style>
    {
        /// <summary>The name of the style (chosen by the artist).</summary>
        public string Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged("Name"); NotifyPropertyChanged("Display"); } }
        private string _Name;

        /// <summary>The name of the author of this style.</summary>
        public string Author { get { return _Author; } set { _Author = value; NotifyPropertyChanged("Author"); NotifyPropertyChanged("Display"); } }
        private string _Author;

        public string Display
        {
            get
            {
                switch (Kind)
                {
                    case StyleKind.Original: return "[original]";
                    case StyleKind.Current: return "[current]";
                    case StyleKind.BuiltIn: return "[built-in] {0} (by {1})".Fmt(Name, Author);
                    case StyleKind.User: return "{0} (by {1})".Fmt(Name, Author);
                    default: throw new Exception("9742978");
                }
            }
        }

        /// <summary>A list of layers that this style is made up of.</summary>
        public ObservableCollection<LayerBase> Layers = new ObservableCollection<LayerBase>();

        /// <summary>Determines whether this style is a built-in one. Not saved as a setting.</summary>
        [XmlIgnore]
        public StyleKind Kind { get; set; }

        public Style()
        {
            Kind = StyleKind.User;
        }

        private void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public int CompareTo(Style other)
        {
            if (other == null)
                return -1;
            int result = Kind.CompareTo(other.Kind);
            if (result != 0)
                return result;
            result = StringComparer.OrdinalIgnoreCase.Compare(_Name, other._Name);
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
}
