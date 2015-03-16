using System;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class HueSaturationLightnessEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectHueSaturationLightness.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectHueSaturationLightness.EffectDescription; } }

        public double Hue { get { return _Hue; } set { _Hue = Math.Min(360, Math.Max(-360, value)); } }
        private double _Hue;
        public static MemberTr HueTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectHueSaturationLightness.Hue); }

        public double Saturation { get { return _Saturation; } set { _Saturation = Math.Max(0.0, value); } }
        private double _Saturation;
        public static MemberTr SaturationTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectHueSaturationLightness.Saturation); }

        public double Lightness { get { return _Lightness; } set { _Lightness = Math.Max(0.0, value); } }
        private double _Lightness;
        public static MemberTr LightnessTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectHueSaturationLightness.Lightness); }

        public HueSaturationLightnessEffect()
        {
            _Hue = 0;
            _Saturation = 100;
            _Lightness = 100;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Hue == 0 && Saturation == 100 && Lightness == 100)
                return layer;

            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColor.Transparent;
                image.Modulate(new Percentage(Lightness), new Percentage(Saturation), new Percentage(Hue * 100.0 / 180.0 + 100));

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}
