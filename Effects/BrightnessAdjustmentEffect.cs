using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;
using D = System.Drawing;

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

        public bool CompensateSaturation { get { return _CompensateSaturation; } set { _CompensateSaturation = value; } }
        private bool _CompensateSaturation;
        public static MemberTr CompensateSaturationTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectBrightnessAdjustment.CompensateSaturation); }

        public BrightnessAdjustmentEffect()
        {
            _Strength = 100;
            _Brightness = 50;
            CompensateSaturation = true;
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
                double fixedStrength = Strength / 100;
                double compensatevalue = ((Brightness / averageBrightness - 1) * (fixedStrength) + 1) * 100;
                double compensatesaturation = 100;
                if (CompensateSaturation && compensatevalue < 100)
                    compensatesaturation = (100 - (100 - (compensatevalue * compensatevalue / 100)) * fixedStrength);
                image.Modulate(new Percentage(compensatevalue), new Percentage(compensatesaturation), new Percentage(100));

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}
