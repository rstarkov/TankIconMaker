using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class ShiftEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.EffectShift.EffectName; } }
        public override string TypeDescription { get { return Program.Translation.EffectShift.EffectDescription; } }

        public int ShiftX { get; set; }
        public static MemberTr ShiftXTr(Translation tr) { return new MemberTr(Program.Translation.CategoryShift, Program.Translation.EffectShift.ShiftX); }

        public int ShiftY { get; set; }
        public static MemberTr ShiftYTr(Translation tr) { return new MemberTr(Program.Translation.CategoryShift, Program.Translation.EffectShift.ShiftY); }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (ShiftX == 0 && ShiftY == 0)
                return layer;
            var result = new BitmapRam(layer.Width, layer.Height);
            result.DrawImage(layer, ShiftX, ShiftY);
            return result;
        }
    }
}
