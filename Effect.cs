using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using RT.Util.Lingo;
using RT.Util.Xml;
using WotDataLib;
using WpfCrutches;

namespace TankIconMaker
{
    abstract class EffectBase : IHasTreeViewItem, IHasTypeNameDescription, IXmlClassifyProcess2, INotifyPropertyChanged
    {
        /// <summary>Describes what this effect type does as concisely as possible.</summary>
        [Browsable(false)]
        public abstract string TypeName { get; }
        /// <summary>Describes what this effect type does in more detail.</summary>
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

        /// <summary>Keeps track of the layer that this effect belongs to. This value is kept up-to-date automatically.</summary>
        [Browsable(false), XmlIgnore]
        public LayerBase Layer;

        public bool Visible { get { return _Visible; } set { _Visible = value; NotifyPropertyChanged("Visible"); } }
        private bool _Visible;
        public static MemberTr VisibleTr(Translation tr) { return new MemberTr(tr.Category.General, tr.LayerAndEffect.EffectVisible); }

        public ValueSelector<BoolWithPassthrough> VisibleFor { get; set; }
        public static MemberTr VisibleForTr(Translation tr) { return new MemberTr(tr.Category.General, tr.LayerAndEffect.EffectVisibleFor); }

        public EffectBase()
        {
            Visible = true;
            VisibleFor = new ValueSelector<BoolWithPassthrough>(BoolWithPassthrough.Yes);
        }

        /// <summary>
        /// Applies the effect to the specified layer. Returns the resulting image. If the layer is writable, may modify it
        /// directly and return the same instance, instead of creating a new one.
        /// </summary>
        public abstract BitmapBase Apply(Tank tank, BitmapBase layer);

        /// <summary>
        /// Stores the <see cref="Version"/> of the maker as it was at the time of saving settings to XML. This may
        /// then be used to apply transformations to the XML produced by old versions of a layer.
        /// </summary>
        public int SavedByVersion;

        void IXmlClassifyProcess2.BeforeXmlClassify(XElement xml) { BeforeXmlClassify(xml); }
        protected virtual void BeforeXmlClassify(XElement xml)
        {
            SavedByVersion = Version;
        }

        void IXmlClassifyProcess2.AfterXmlClassify(XElement xml) { AfterXmlClassify(xml); }
        protected virtual void AfterXmlClassify(XElement xml) { }

        void IXmlClassifyProcess2.BeforeXmlDeclassify(XElement xml) { BeforeXmlDeclassify(xml); }
        protected virtual void BeforeXmlDeclassify(XElement xml) { }

        void IXmlClassifyProcess2.AfterXmlDeclassify(XElement xml) { AfterXmlDeclassify(xml); }
        protected virtual void AfterXmlDeclassify(XElement xml) { }

        [XmlIgnore, Browsable(false)]
        public TreeViewItem TreeViewItem { get; set; }

        protected void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public virtual EffectBase Clone()
        {
            var result = MemberwiseClone() as EffectBase;
            result.Layer = null;
            result.PropertyChanged = (_, __) => { };
            result.VisibleFor = VisibleFor.Clone();
            return result;
        }
    }
}
