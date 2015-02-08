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
    class WaveEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectWave.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectWave.EffectDescription; } }

        public double Amplitude { get { return _Amplitude; } set { _Amplitude = Math.Max(0.0, value); } }
        private double _Amplitude;
        public static MemberTr AmplitudeTr(Translation tr) { return new MemberTr(tr.Category.Wave, tr.EffectWave.Amplitude); }

        public double Length { get { return _Length; } set { _Length = Math.Max(0.001, value); } }
        private double _Length;
        public static MemberTr LengthTr(Translation tr) { return new MemberTr(tr.Category.Wave, tr.EffectWave.Length); }

        public WaveEffect()
        {
            Amplitude = 0;
            Length = 1;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Amplitude == 0)
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
                image.Wave(Amplitude, Length);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}
