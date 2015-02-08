using RT.Util.Lingo;
using WotDataLib;
using System;
using System.IO;
using D = System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ImageMagick;

namespace TankIconMaker.Effects
{
    class BrightnessAdjustmentEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectBrightnessAdjustment.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectBrightnessAdjustment.EffectDescription; } }

        public double Strength { get { return _Strength; } set { _Strength = Math.Min(100.0, Math.Max(0.0, value)); } }
        private double _Strength;
        public static MemberTr StrengthTr(Translation tr) { return new MemberTr(tr.Category.BrightnessAdjustment, tr.EffectBrightnessAdjustment.Strength); }

        public double Brightness { get { return _Brightness; } set { _Brightness = Math.Min(100.0, Math.Max(0.0, value)); } }
        private double _Brightness;
        public static MemberTr BrightnessTr(Translation tr) { return new MemberTr(tr.Category.BrightnessAdjustment, tr.EffectBrightnessAdjustment.Brightness); }

        public bool CompensateSaturation { get { return _CompensateSaturation; } set { _CompensateSaturation = value; } }
        private bool _CompensateSaturation;
        public static MemberTr CompensateSaturationTr(Translation tr) { return new MemberTr(tr.Category.BrightnessAdjustment, tr.EffectBrightnessAdjustment.CompensateSaturation); }


        public BrightnessAdjustmentEffect()
        {
            _Strength = 100;
            _Brightness = 50;
            CompensateSaturation = true;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Strength == 0)
            {
                return layer;
            }
            BitmapWpf bitmapwpf = layer.ToBitmapWpf();
            BitmapSource bitmapsource = bitmapwpf.UnderlyingImage;
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
                encoder.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            using (MagickImage image = new MagickImage(bitmap))
            {
                try
                {
                    #region Convertion by itself
                    double averagebrigthness = 0, pixels = 0;
                    for (int i = 0; i < bitmap.Width; ++i)
                    {
                        for (int j = 0; j < bitmap.Height; ++j)
                        {
                            D.Color color = bitmap.GetPixel(i, j);
                            if (color.A != 0)
                            {
                                double pixelStrength = color.A / 255.0;
                                averagebrigthness += (color.R + color.G + color.B) / 7.65 * pixelStrength; // () / 3 / 255 * 100
                                pixels += pixelStrength;
                            }
                        }
                    }
                    if (pixels == 0)
                        return layer;
                    averagebrigthness /= pixels;
                    image.BackgroundColor = MagickColor.Transparent;
                    double fixedStrength = Strength / 100;
                    double compensatevalue = ((Brightness / (double)averagebrigthness - 1) * (fixedStrength) + 1) * 100;
                    double compensatesaturation = 100;
                    if (CompensateSaturation && compensatevalue < 100)
                    {
                        compensatesaturation = (100 - (100 - (compensatevalue * compensatevalue / 100)) * fixedStrength);
                    }
                    image.Modulate(new Percentage(compensatevalue), new Percentage(compensatesaturation), new Percentage(100));
                    image.Label = compensatevalue.ToString("0.0");
                }
                catch (Exception e)
                {
                    return layer;
                }
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}
