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
                    case StyleKind.Original: return App.Translation.Misc.StyleDisplay_Original;
                    case StyleKind.Current: return App.Translation.Misc.StyleDisplay_Current;
                    case StyleKind.BuiltIn: return App.Translation.Misc.StyleDisplay_BuiltIn.Fmt(Name, Author);
                    case StyleKind.User: return App.Translation.Misc.StyleDisplay_Normal.Fmt(Name, Author);
                    default: throw new Exception("9742978");
                }
            }
        }

        /// <summary>A list of layers that this style is made up of.</summary>
        public ObservableCollection<LayerBase> Layers = new ObservableCollection<LayerBase>();

        /// <summary>A link to the forum post by the original author describing this style. Only used for built-in styles.</summary>
        public string ForumLink { get; set; }

        /// <summary>Determines whether this style is a built-in one. Not saved as a setting.</summary>
        [XmlIgnore]
        public StyleKind Kind { get; set; }

        public Style()
        {
            Kind = StyleKind.User;
        }

        private void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public void TranslationChanged()
        {
            NotifyPropertyChanged("Name");
            NotifyPropertyChanged("Author");
            NotifyPropertyChanged("Display");
        }

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
