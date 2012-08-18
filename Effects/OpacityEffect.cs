using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class OpacityEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.EffectOpacity.EffectName; } }
        public override string TypeDescription { get { return Program.Translation.EffectOpacity.EffectDescription; } }

        public double Opacity { get; set; }
        public static MemberTr OpacityTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectOpacity.Opacity); }

        public OpacityStyle Style { get; set; }
        public static MemberTr StyleTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectOpacity.Style); }

        public OpacityEffect()
        {
            Opacity = -1.5;
            Style = OpacityStyle.Auto;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            layer.ScaleOpacity(Opacity, Style);
            return layer;
        }
    }
}
