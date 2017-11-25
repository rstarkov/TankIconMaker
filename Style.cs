using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using RT.Util.Serialization;

namespace TankIconMaker
{
    enum StyleKind { Original, Current, BuiltIn, User }

    sealed class Style : INotifyPropertyChanged, IComparable<Style>, IClassifyXmlObjectProcessor
    {
        /// <summary>The name of the style (chosen by the artist).</summary>
        public string Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged("Name"); NotifyPropertyChanged("Display"); } }
        private string _Name;

        /// <summary>The name of the author of this style.</summary>
        public string Author { get { return _Author; } set { _Author = value; NotifyPropertyChanged("Author"); NotifyPropertyChanged("Display"); } }
        private string _Author;

        /// <summary>
        /// Contains the TIM version style exported from
        /// </summary>
        public int SavedByVersion
        {
            get { return _SavedByVersion; }
            set
            {
                _SavedByVersion = value;
            }
        }
        private int _SavedByVersion;

        /// <summary>A template for the path where the icons are to be saved.</summary>
        public string PathTemplate { get { return _PathTemplate; } set { _PathTemplate = value; NotifyPropertyChanged("PathTemplate"); } }
        private string _PathTemplate;

        /// <summary>A template for the path where the battleAtlas are to be saved.</summary>
        public string BattleAtlasPathTemplate { get { return _BattleAtlasPathTemplate; } set { _BattleAtlasPathTemplate = value; NotifyPropertyChanged("BattleAtlasPathTemplate"); } }
        private string _BattleAtlasPathTemplate;

        /// <summary>A template for the path where the vehicleMarkersAtlas are to be saved.</summary>
        public string VehicleMarkersAtlasPathTemplate { get { return _VehicleMarkersAtlasPathTemplate; } set { _VehicleMarkersAtlasPathTemplate = value; NotifyPropertyChanged("VehicleMarkersAtlasPathTemplate"); } }
        private string _VehicleMarkersAtlasPathTemplate;

        /// <summary>A template for the path where the custom atlas are to be saved.</summary>
        public string CustomAtlasPathTemplate { get { return _CustomAtlasPathTemplate; } set { _CustomAtlasPathTemplate = value; NotifyPropertyChanged("CustomAtlasPathTemplate"); } }
        private string _CustomAtlasPathTemplate;

        /// <summary>Enable/disable saving the icons.</summary>
        public bool IconsBulkSaveEnabled
        {
            get { return _IconsBulkSaveEnabled; }
            set
            {
                _IconsBulkSaveEnabled = value;
                NotifyPropertyChanged("IconsBulkSaveEnabled");
            }
        }
        private bool _IconsBulkSaveEnabled = true;

        /// <summary>Enable/disable saving the battleAtlas.</summary>
        public bool BattleAtlasBulkSaveEnabled
        {
            get { return _BattleAtlasBulkSaveEnabled; }
            set
            {
                _BattleAtlasBulkSaveEnabled = value;
                NotifyPropertyChanged("BattleAtlasBulkSaveEnabled");
            }
        }
        private bool _BattleAtlasBulkSaveEnabled = true;

        /// <summary>Enable/disable saving the vehicleMarkersAtlas.</summary>
        public bool VehicleMarkersAtlasBulkSaveEnabled
        {
            get { return _VehicleMarkersAtlasBulkSaveEnabled; }
            set
            {
                _VehicleMarkersAtlasBulkSaveEnabled = value;
                NotifyPropertyChanged("VehicleMarkersAtlasBulkSaveEnabled");
            }
        }
        private bool _VehicleMarkersAtlasBulkSaveEnabled = true;

        /// <summary>Enable/disable saving the custom atlas.</summary>
        public bool CustomAtlasBulkSaveEnabled
        {
            get { return _CustomAtlasBulkSaveEnabled; }
            set
            {
                _CustomAtlasBulkSaveEnabled = value;
                NotifyPropertyChanged("CustomAtlasBulkSaveEnabled");
            }
        }
        private bool _CustomAtlasBulkSaveEnabled = false;

        /// <summary>Icon width; defaults to the value used in old clients so that old styles can be loaded correctly.</summary>
        public int IconWidth = 80;
        /// <summary>Icon height; defaults to the value used in old clients so that old styles can be loaded correctly.</summary>
        public int IconHeight = 24;
        /// <summary>
        ///     If true, the number of transparent pixels on the right will be made the same as on the left (up to <see
        ///     cref="IconWidth"/>), so as to make it possible to vertically center the icon. Defaults to the value used in old
        ///     clients so that old styles can be loaded correctly.</summary>
        public bool Centerable = false;

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
        [ClassifyIgnore]
        public StyleKind Kind { get; set; }

        public Style()
        {
            Kind = StyleKind.User;
            Layers.CollectionChanged += updateLayerStyle;
        }

        void updateLayerStyle(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
                foreach (var item in e.NewItems.OfType<LayerBase>())
                    item.ParentStyle = this;
            else if (e.Action == NotifyCollectionChangedAction.Reset)
                foreach (var item in Layers)
                    item.ParentStyle = this;
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
            result.Layers = new ObservableCollection<LayerBase>();
            result.Layers.CollectionChanged += result.updateLayerStyle;
            foreach (var l in Layers)
                result.Layers.Add(l.Clone());
            return result;
        }

        public void AfterDeserialize(XElement xml)
        {
            foreach (var layer in Layers)
                layer.ParentStyle = this;
            Layers.CollectionChanged -= updateLayerStyle;
            Layers.CollectionChanged += updateLayerStyle;
        }

        public void AfterSerialize(XElement xml) { }
        public void BeforeSerialize() { }
        public void BeforeDeserialize(XElement xml) { }
    }

    /// <summary>Thrown from a layer or effect implementation to report an error that the user can fix or needs to know about.</summary>
    class StyleUserError : Exception
    {
        public bool Formatted { get; private set; }
        public StyleUserError(string message) : this(message, false) { }
        public StyleUserError(string message, bool formatted) : base(message) { Formatted = formatted; }
    }
}
