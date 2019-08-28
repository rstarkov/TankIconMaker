using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class RotateEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectRotate.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectRotate.EffectDescription; } }

        public double Angle { get { return _Angle; } set { _Angle = Math.Min(360.0, Math.Max(-360.0, value)); } }
        private double _Angle;
        public static MemberTr AngleTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectRotate.Angle); }

        public double RotateX { get { return _RotateX; } set { _RotateX = value; } }
        private double _RotateX;
        public static MemberTr RotateXTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectRotate.RotateX); }

        public double RotateY { get { return _RotateY; } set { _RotateY = value; } }
        private double _RotateY;
        public static MemberTr RotateYTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectRotate.RotateY); }

        public RotateEffect()
        {
            Angle = 0;
            RotateX = 40;
            RotateY = 12;
        }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            if (Angle == 0)
                return layer;

            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColors.Transparent;
                image.Distort(DistortMethod.ScaleRotateTranslate, new double[] { RotateX, RotateY, Angle });

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}
