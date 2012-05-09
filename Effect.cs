using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using RT.Util.Xml;
using WpfCrutches;

namespace TankIconMaker
{
    abstract class EffectBase : IHasTreeViewItem, IHasTypeNameDescription, INotifyPropertyChanged
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

        /// <summary>Keeps track of the layer that this effect belongs to. This value is kept up-to-date automatically.</summary>
        [Browsable(false), XmlIgnore]
        public LayerBase Layer;

        [Category("General")]
        [Description("Allows you to hide this effect without deleting it.")]
        public bool Visible { get { return _Visible; } set { _Visible = value; NotifyPropertyChanged("Visible"); } }
        private bool _Visible;

        public EffectBase()
        {
            Visible = true;
        }

        /// <summary>
        /// Applies the effect to the specified layer. Returns the resulting image. If the layer is writable, may modify it
        /// directly and return the same instance, instead of creating a new one.
        /// </summary>
        public abstract BitmapBase Apply(Tank tank, BitmapBase layer);

        [XmlIgnore, Browsable(false)]
        public TreeViewItem TreeViewItem { get; set; }

        protected void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        public EffectBase Clone()
        {
            var result = MemberwiseClone() as EffectBase;
            result.PropertyChanged = (_, __) => { };
            return result;
        }
    }
}
