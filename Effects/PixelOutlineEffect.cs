using System.Windows.Media;
using RT.Util.ExtensionMethods;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class PixelOutlineEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectPixelOutline.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectPixelOutline.EffectDescription; } }

        public ColorSelector Color { get; set; }
        public static MemberTr ColorTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectPixelOutline.Color); }

        public int Threshold { get { return _Threshold; } set { _Threshold = value.Clip(0, 255); } }
        private int _Threshold;
        public static MemberTr ThresholdTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectPixelOutline.Threshold); }

        public bool Inside { get; set; }
        public static MemberTr InsideTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectPixelOutline.Inside); }

        public bool KeepImage { get; set; }
        public static MemberTr KeepImageTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectPixelOutline.KeepImage); }

        public PixelOutlineEffect()
        {
            Color = new ColorSelector(Colors.Black);
            Threshold = 0;
            Inside = false;
            KeepImage = true;
        }

        public override EffectBase Clone()
        {
            var result = (PixelOutlineEffect) base.Clone();
            result.Color = Color.Clone();
            return result;
        }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            var outline = new BitmapRam(layer.Width, layer.Height);
            layer.GetOutline(outline, Color.GetColorWpf(tank), Threshold, Inside);
            if (KeepImage)
            {
                layer.DrawImage(outline);
                return layer;
            }
            else
                return outline;
        }
    }
}
