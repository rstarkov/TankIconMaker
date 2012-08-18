using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class FlipEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.EffectFlip.EffectName; } }
        public override string TypeDescription { get { return Program.Translation.EffectFlip.EffectDescription; } }

        public bool FlipHorz { get; set; }
        public static MemberTr FlipHorzTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectFlip.FlipHorz); }
        public bool FlipVert { get; set; }
        public static MemberTr FlipVertTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectFlip.FlipVert); }

        public FlipEffect()
        {
            FlipHorz = true;
            FlipVert = false;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (FlipHorz)
                layer.FlipHorz();
            if (FlipVert)
                layer.FlipVert();
            return layer;
        }
    }
}
