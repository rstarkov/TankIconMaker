using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using RT.Util.Xml;

namespace TankIconMaker.Effects
{
    class GaussianBlurEffect : EffectBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Blur: Gaussian"; } }
        public override string TypeDescription { get { return "Blurs the current layer using Gaussian blur."; } }

        [Category("Blur")]
        [Description("Blur radius. Larger values result in more blur.")]
        public double Radius { get { return _Radius; } set { _Radius = Math.Max(1.0, value); } }
        private double _Radius = 2.5;
        [Category("Blur")]
        [Description("Specifies how to sample around the edges: assume the image beyond the edges is transparent, wrap to the other side, or use the same pixel color that touches the edge.")]
        public BlurEdgeMode Edge { get; set; }

        [XmlIgnore]
        private GaussianBlur _blur;

        public GaussianBlurEffect()
        {
            Edge = BlurEdgeMode.Same;
        }

        public override WriteableBitmap Apply(Tank tank, WriteableBitmap layer)
        {
            if (_blur == null || _blur.Radius != Radius)
                lock (this)
                    if (_blur == null || _blur.Radius != Radius)
                        _blur = new GaussianBlur(Radius);
            return _blur.Blur(layer, Edge);
        }
    }
}
