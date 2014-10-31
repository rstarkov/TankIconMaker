using System;
using System.Windows.Media;
using RT.Util.Lingo;
using RT.Util.Serialization;

namespace TankIconMaker.Effects
{
    class ShadowEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectShadow.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectShadow.EffectDescription; } }

        public double Radius { get { return _Radius; } set { _Radius = Math.Max(1.0, value); } }
        private double _Radius = 3.5;
        public static MemberTr RadiusTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectShadow.Radius); }

        public double Spread { get { return _Spread; } set { _Spread = Math.Max(0, value); } }
        private double _Spread = 5;
        public static MemberTr SpreadTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectShadow.Spread); }

        public ColorSelector Color { get; set; }
        public static MemberTr ColorTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectShadow.Color); }

        public int ShiftX { get; set; }
        public static MemberTr ShiftXTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectShadow.ShiftX); }
        public int ShiftY { get; set; }
        public static MemberTr ShiftYTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectShadow.ShiftY); }

        [ClassifyIgnore]
        private GaussianBlur _blur;

        public ShadowEffect()
        {
            Color = new ColorSelector(Colors.Black);
        }

        public override EffectBase Clone()
        {
            var result = (ShadowEffect) base.Clone();
            result.Color = Color.Clone();
            return result;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (_blur == null || _blur.Radius != Radius)
                lock (this)
                    if (_blur == null || _blur.Radius != Radius)
                        _blur = new GaussianBlur(Radius);

            BitmapBase shadow = layer.ToBitmapRam();
            var color = Color.GetColorWpf(tank);
            shadow.ReplaceColor(color);
            shadow.Blur(_blur, BlurEdgeMode.Transparent);
            shadow.ScaleOpacity(Spread, OpacityStyle.Additive);
            shadow.Transparentize(color.A);
            layer.DrawImage(shadow, below: true);
            return layer;
        }
    }
}
