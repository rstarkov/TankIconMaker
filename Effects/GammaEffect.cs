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

        public double GammaRed { get { return _GammaRed; } set { _GammaRed = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _GammaRed;
        public static MemberTr GammaRedTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectGamma.GammaRed); }

        public double GammaGreen { get { return _GammaGreen; } set { _GammaGreen = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _GammaGreen;
        public static MemberTr GammaGreenTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectGamma.GammaGreen); }

        public double GammaBlue { get { return _GammaBlue; } set { _GammaBlue = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _GammaBlue;
        public static MemberTr GammaBlueTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectGamma.GammaBlue); }

        public GammaEffect()
        {
            _GammaRed = 1;
            _GammaGreen = 1;
            _GammaBlue = 1;
        }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            if (GammaRed == 1 && GammaGreen == 1 && GammaBlue == 1)
                return layer;

            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColors.Transparent;
                image.GammaCorrect(GammaRed, Channels.Red);
                image.GammaCorrect(GammaGreen, Channels.Green);
                image.GammaCorrect(GammaBlue, Channels.Blue);
                //image.GammaCorrect(GammaRed, GammaGreen, GammaBlue);

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}
