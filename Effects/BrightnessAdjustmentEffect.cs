using System;
using System.ComponentModel;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class BrightnessAdjustmentEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectBrightnessAdjustment.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectBrightnessAdjustment.EffectDescription; } }

        public double Strength { get { return _Strength; } set { _Strength = Math.Min(100.0, Math.Max(0.0, value)); } }
        private double _Strength;
        public static MemberTr StrengthTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectBrightnessAdjustment.Strength); }

        public double Brightness { get { return _Brightness; } set { _Brightness = Math.Min(100.0, Math.Max(0.0, value)); } }
        private double _Brightness;
        public static MemberTr BrightnessTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectBrightnessAdjustment.Brightness); }

        public SaturationMode Saturation { get; set; }
        public static MemberTr SaturationTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectBrightnessAdjustment.Saturation); }

        [TypeConverter(typeof(EffectBrightnessAdjustmentTranslation.SaturationModeTranslation.Conv))]
        public enum SaturationMode
        {
            NoChange,
            Reduce,
            Zero,
        }

        public BrightnessAdjustmentEffect()
        {
            _Strength = 100;
            _Brightness = 30;
            Saturation = SaturationMode.Reduce;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Strength == 0)
                return layer;

            int totalAlpha = 0;
            long totalBrightness = 0;

            using (layer.UseRead())
                unsafe
                {
                    for (int y = 0; y < layer.Height; y++)
                    {
                        byte* linePtr = layer.Data + y * layer.Stride;
                        byte* lineEndPtr = linePtr + layer.Width * 4;
                        while (linePtr < lineEndPtr)
                        {
                            int brightness = *(linePtr) * 722 + *(linePtr + 1) * 7152 + *(linePtr + 2) * 2126;
                            totalBrightness += brightness * *(linePtr + 3);
                            totalAlpha += *(linePtr + 3);
                            linePtr += 4;
                        }
                    }
                }
            if (totalAlpha == 0)
                return layer;

            using (var image = layer.ToMagickImage())
            {
                var averageBrightness = (double) totalBrightness * 100.0 / (double) totalAlpha / 255.0 / 10000.0;
                image.BackgroundColor = MagickColor.Transparent;
                double strength = Strength / 100;
                double scaleValue = 1 + (Brightness / averageBrightness - 1) * strength;
                double scaleSaturation = Saturation == SaturationMode.Zero ? 0 : 1;
                if (Saturation == SaturationMode.Reduce && scaleValue < 1)
                    scaleSaturation = 1 - (1 - scaleValue * scaleValue) * strength;
                image.Modulate(new Percentage(100 * scaleValue), new Percentage(100 * scaleSaturation), new Percentage(100));

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}
