using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util.Xml;
using WpfCrutches;
using D = System.Drawing;

namespace TankIconMaker
{
    abstract class LayerBase : IHasTreeViewItem, IHasTypeNameDescription, IXmlClassifyProcess, INotifyPropertyChanged
    {
        /// <summary>Describes what this layer type does as concisely as possible.</summary>
        [Browsable(false)]
        public abstract string TypeName { get; }
        /// <summary>Describes what this layer type does in more detail.</summary>
        [Browsable(false)]
        public abstract string TypeDescription { get; }
        /// <summary>Specifies the version of this layer’s settings - incremented on changing layer settings backwards-incompatibly.</summary>
        [Browsable(false)]
        public abstract int Version { get; }

        [Browsable(false)]
        public string Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged("Name"); NotifyPropertyChanged("NameVisibility"); } }
        private string _Name;

        [Browsable(false)]
        public Visibility NameVisibility { get { return string.IsNullOrEmpty(Name) ? Visibility.Collapsed : Visibility.Visible; } }

        [Browsable(false)]
        public ObservableCollection<EffectBase> Effects { get; set; }

        [Category("General")]
        [Description("Allows you to hide this layer without deleting it.")]
        public bool Visible { get { return _Visible; } set { _Visible = value; NotifyPropertyChanged("Visible"); } }
        private bool _Visible;

        public LayerBase()
        {
            Effects = new ObservableCollection<EffectBase>();
            Effects.CollectionChanged += updateEffectLayer;
            Visible = true;
        }

        private void updateEffectLayer(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
                foreach (var item in e.NewItems.OfType<EffectBase>())
                    item.Layer = this;
            else if (e.Action == NotifyCollectionChangedAction.Reset)
                foreach (var item in Effects)
                    item.Layer = this;
        }

        /// <summary>
        /// Returns this layer's image for this tank. Will be called from multiple threads in parallel. The result may be any size. Return
        /// a writable image if it may be modified directly, or mark it as read-only otherwise.
        /// </summary>
        public abstract BitmapBase Draw(Tank tank);

        /// <summary>
        /// Stores the <see cref="Version"/> of the maker as it was at the time of saving settings to XML. This may
        /// then be used to apply transformations to the XML produced by old versions of a maker.
        /// </summary>
        private int SavedByVersion;
        void IXmlClassifyProcess.BeforeXmlClassify() { SavedByVersion = Version; }
        void IXmlClassifyProcess.AfterXmlDeclassify()
        {
            foreach (var effect in Effects)
                effect.Layer = this;
            Effects.CollectionChanged -= updateEffectLayer;
            Effects.CollectionChanged += updateEffectLayer;
        }

        [XmlIgnore, Browsable(false)]
        public TreeViewItem TreeViewItem { get; set; }

        protected void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public LayerBase Clone()
        {
            var result = MemberwiseClone() as LayerBase;
            result.PropertyChanged = (_, __) => { };
            result.Effects = new ObservableCollection<EffectBase>();
            result.Effects.CollectionChanged += result.updateEffectLayer;
            foreach (var e in Effects)
                result.Effects.Add(e.Clone());
            return result;
        }
    }
}
