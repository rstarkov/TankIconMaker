using System;
using RT.Util.Lingo;
using RT.Util.Xml;

namespace TankIconMaker.Effects
{
    class GaussianBlurEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectGaussianBlur.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectGaussianBlur.EffectDescription; } }

        public double Radius { get { return _Radius; } set { _Radius = Math.Max(1.0, value); } }
        private double _Radius = 2.5;
        public static MemberTr RadiusTr(Translation tr) { return new MemberTr(tr.Category.Blur, tr.EffectGaussianBlur.Radius); }

        public BlurEdgeMode Edge { get; set; }
        public static MemberTr EdgeTr(Translation tr) { return new MemberTr(tr.Category.Blur, tr.EffectGaussianBlur.Edge); }

        [XmlIgnore]
        private GaussianBlur _blur;

        public GaussianBlurEffect()
        {
            Edge = BlurEdgeMode.Same;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (_blur == null || _blur.Radius != Radius)
                lock (this)
                    if (_blur == null || _blur.Radius != Radius)
                        _blur = new GaussianBlur(Radius);
            layer.Blur(_blur, Edge);
            return layer;
        }
    }
}
