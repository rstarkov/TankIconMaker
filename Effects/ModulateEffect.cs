using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class ModulateEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectHueSaturationBrightness.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectHueSaturationBrightness.EffectDescription; } }

        public double Hue { get { return _Hue; } set { _Hue = Math.Min(200.0, Math.Max(0.0, value)); } }
        private double _Hue;
        public static MemberTr HueTr(Translation tr) { return new MemberTr(tr.Category.HueSaturationBrightness, tr.EffectHueSaturationBrightness.Hue); }

        public double Saturation { get { return _Saturation; } set { _Saturation = Math.Max(0.0, value); } }
        private double _Saturation;
        public static MemberTr SaturationTr(Translation tr) { return new MemberTr(tr.Category.HueSaturationBrightness, tr.EffectHueSaturationBrightness.Saturation); }

        public double Brightness { get { return _Brightness; } set { _Brightness = Math.Max(0.0, value); } }
        private double _Brightness;
        public static MemberTr BrightnessTr(Translation tr) { return new MemberTr(tr.Category.HueSaturationBrightness, tr.EffectHueSaturationBrightness.Brightness); }

        public ModulateEffect()
        {
            _Hue = 100;
            _Saturation = 100;
            _Brightness = 100;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Hue == 100 && Saturation == 100 && Brightness == 100)
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
                #region Convertion by itself
                image.BackgroundColor = MagickColor.Transparent;
                image.Modulate(new Percentage(Brightness), new Percentage(Saturation), new Percentage(Hue));
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}
