using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class GammaEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectGamma.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectGamma.EffectDescription; } }

        public double Gamma { get { return _Gamma; } set { _Gamma = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _Gamma;
        public static MemberTr GammaTr(Translation tr) { return new MemberTr(tr.Category.Gamma, tr.EffectGamma.gamma); }

        /*public double GammaRed { get { return _GammaRed; } set { _GammaRed = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _GammaRed;
        public static MemberTr GammaRedTr(Translation tr) { return new MemberTr(tr.Category.Gamma, tr.EffectGamma.gammaRed); }

        public double GammaGreen { get { return _GammaGreen; } set { _GammaGreen = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _GammaGreen;
        public static MemberTr GammaGreenTr(Translation tr) { return new MemberTr(tr.Category.Gamma, tr.EffectGamma.gammaGreen); }

        public double GammaBlue { get { return _GammaBlue; } set { _GammaBlue = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _GammaBlue;
        public static MemberTr GammaBlueTr(Translation tr) { return new MemberTr(tr.Category.Gamma, tr.EffectGamma.gammaBlue); }*/

        public GammaEffect()
        {
            _Gamma = 1;
            /*_GammaRed = 1;
            _GammaGreen = 1;
            _GammaBlue = 1;*/
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Gamma == 1/*GammaRed == 1 && GammaGreen == 1 && GammaBlue == 1*/)
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
                image.Gamma(Gamma/*GammaRed, GammaGreen, GammaBlue*/);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}
