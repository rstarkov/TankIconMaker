﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using RT.Util.Lingo;
using RT.Util.Serialization;
using WpfCrutches;

namespace TankIconMaker
{
    abstract class EffectBase : IHasTreeViewItem, IHasTypeNameDescription, IClassifyXmlObjectProcessor, INotifyPropertyChanged
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
        [Browsable(false), ClassifyIgnore]
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
        /// Applies the effect to the specified layer. Returns the resulting image. The caller must make sure that the layer
        /// is writable, because all Apply methods expect to be able to modify the layer if necessary and return it, instead of
        /// creating a new instance.
        /// </summary>
        public abstract BitmapBase Apply(RenderTask renderTask, BitmapBase layer);

        /// <summary>
        /// Stores the <see cref="Version"/> of the maker as it was at the time of saving settings to XML. This may
        /// then be used to apply transformations to the XML produced by old versions of a layer.
        /// </summary>
        public int SavedByVersion;

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
        protected virtual void AfterDeserialize(XElement xml) { }

        [ClassifyIgnore, Browsable(false)]
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
