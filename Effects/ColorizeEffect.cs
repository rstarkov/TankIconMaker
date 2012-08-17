using System.Windows.Media;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class ColorizeEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.EffectColorize.EffectName; } }
        public override string TypeDescription { get { return Program.Translation.EffectColorize.EffectDescription; } }

        public ColorSelector Color { get; set; }
        public static MemberTr ColorTr(Translation tr) { return new MemberTr(Program.Translation.CategorySettings, Program.Translation.EffectColorize.Color); }

        public ColorizeEffect()
        {
            Color = new ColorSelector(Colors.White);
        }

        public override EffectBase Clone()
        {
            var result = (ColorizeEffect) base.Clone();
            result.Color = Color.Clone();
            return result;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            var color = ColorHSV.FromColor(Color.GetColorWpf(tank));
            layer.Colorize(color.Hue, color.Saturation / 100.0, color.Value / 100.0 - 0.5, color.Alpha / 255.0);
            return layer;
        }
    }
}
