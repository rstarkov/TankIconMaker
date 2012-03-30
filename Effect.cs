using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using RT.Util.Xml;
using WpfCrutches;


/// Shift layer by x pixels
/// Position layer (perfect size with tunable alpha threshold)
/// Position between other layers
/// Colorize
/// Opacity multiplier
/// Outline
/// Shadow
/// Blur

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
        [XmlIgnore]
        public LayerBase Layer;

        public double Mix { get; set; }

        /// <summary>Used internally to apply the effect. Hidden from IntelliSense to avoid confusion.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract BitmapSource ApplyInternal(Tank tank, BitmapSource layer);

        public TreeViewItem TreeViewItem { get; set; }

        protected void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }

    abstract class EffectBaseWpf : EffectBase
    {
        public abstract BitmapSource Apply(Tank tank, BitmapSource layer);

        public override BitmapSource ApplyInternal(Tank tank, BitmapSource layer)
        {
            return Apply(tank, layer);
        }
    }

    abstract class EffectBaseGdi : EffectBase
    {
        public abstract BitmapGdi Apply(Tank tank, BitmapGdi layer);

        public override BitmapSource ApplyInternal(Tank tank, BitmapSource layer)
        {
            return Apply(tank, layer.ToGdi()).ToWpfWriteable();
        }
    }
}
