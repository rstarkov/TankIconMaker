using System.Windows.Media;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class PixelOutlineEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.EffectPixelOutline.EffectName; } }
        public override string TypeDescription { get { return Program.Translation.EffectPixelOutline.EffectDescription; } }

        public ColorSelector Color { get; set; }
        public static MemberTr ColorTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectPixelOutline.Color); }

        public PixelOutlineEffect()
        {
            Color = new ColorSelector(Colors.Black);
        }

        public override EffectBase Clone()
        {
            var result = (PixelOutlineEffect) base.Clone();
            result.Color = Color.Clone();
            return result;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            var outline = new BitmapRam(layer.Width, layer.Height);
            layer.GetOutline(outline, Color.GetColorWpf(tank));
            layer.DrawImage(outline);
            return layer;
        }
    }
}
