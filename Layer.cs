using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using RT.Util.Lingo;
using RT.Util.Serialization;
using WpfCrutches;

namespace TankIconMaker
{
    abstract class LayerBase : IHasTreeViewItem, IHasTypeNameDescription, IClassifyXmlObjectProcessor, INotifyPropertyChanged
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

        /// <summary>Keeps track of the style that this layer belongs to. This value is kept up-to-date automatically.</summary>
        [Browsable(false), ClassifyIgnore]
        public Style ParentStyle { get; set; }
        [Browsable(false)]
        public ObservableCollection<EffectBase> Effects { get; set; }

        /// <summary>Id for use in Layer Mask.</summary>
        private string _Id;
        public string Id { get { return _Id; } set { _Id = Regex.Replace(value, @"[^A-Za-z0-9_]", ""); NotifyPropertyChanged("Id"); } }
        public static MemberTr IdTr(Translation tr) { return new MemberTr(tr.Category.General, tr.LayerAndEffect.LayerId); }

        public bool Visible { get { return _Visible; } set { _Visible = value; NotifyPropertyChanged("Visible"); } }
        private bool _Visible;
        public static MemberTr VisibleTr(Translation tr) { return new MemberTr(tr.Category.General, tr.LayerAndEffect.LayerVisible); }

        public ValueSelector<BoolWithPassthrough> VisibleFor { get; set; }
        public static MemberTr VisibleForTr(Translation tr) { return new MemberTr(tr.Category.General, tr.LayerAndEffect.LayerVisibleFor); }

        public LayerBase()
        {
            Effects = new ObservableCollection<EffectBase>();
            Effects.CollectionChanged += updateEffectLayer;
            Visible = true;
            VisibleFor = new ValueSelector<BoolWithPassthrough>(BoolWithPassthrough.Yes);
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
        /// then be used to apply transformations to the XML produced by old versions of a layer.
        /// </summary>
        protected int SavedByVersion;

        void IClassifyObjectProcessor<XElement>.BeforeSerialize() { BeforeSerialize(); }
        protected virtual void BeforeSerialize()
        {
            SavedByVersion = Version;
        }

        void IClassifyObjectProcessor<XElement>.AfterSerialize(XElement xml) { AfterSerialize(xml); }
        protected virtual void AfterSerialize(XElement xml) { }

        void IClassifyObjectProcessor<XElement>.BeforeDeserialize(XElement xml) { BeforeDeserialize(xml); }
        protected virtual void BeforeDeserialize(XElement xml) { }

        void IClassifyObjectProcessor<XElement>.AfterDeserialize(XElement xml) { AfterDeserialize(xml); }
        protected virtual void AfterDeserialize(XElement xml)
        {
            foreach (var effect in Effects)
                effect.Layer = this;
            var oldSizePosEffects = Effects.Where(e => e is Effects.SizePosEffect && e.SavedByVersion <= 2).ToList();
            if (oldSizePosEffects.Any())
            {
                oldSizePosEffects.Reverse();
                foreach (var e in oldSizePosEffects)
                {
                    Effects.Remove(e);
                    Effects.Insert(0, e);
                }
            }
            Effects.CollectionChanged -= updateEffectLayer;
            Effects.CollectionChanged += updateEffectLayer;
        }

        [ClassifyIgnore, Browsable(false)]
        public TreeViewItem TreeViewItem { get; set; }

        protected void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public virtual LayerBase Clone()
        {
            var result = MemberwiseClone() as LayerBase;
            result.PropertyChanged = (_, __) => { };
            result.TreeViewItem = null;
            result.VisibleFor = VisibleFor.Clone();
            result.Effects = new ObservableCollection<EffectBase>();
            result.Effects.CollectionChanged += result.updateEffectLayer;
            foreach (var e in Effects)
                result.Effects.Add(e.Clone());
            return result;
        }
    }
}
